#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH && POKEROLDIMPL
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.AssetGraph;
using UnityEngine.AssetGraph.DataModel.Version2;
using AssetBundleManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;

[CustomNode("Custom/Create Manifest", 1000)]
public class ManifestCreatorNode : Node
{
    #region Private Fields

    [SerializeField] private string _serverBundlesFolder;
    [SerializeField] private string _manifestFileName;

    // TODO: Add MacOS
    private readonly List<string> _ignoreList = new List<string>
    {
        "Windows.manifest",
        "Android.manifest",
        "iOS.manifest",
        "WebGL.manifest",
        "Windows",
        "Android",
        "iOS",
        "WebGL"
    };

    [SerializeField] private string _pathesToStrip;
    private AssetReference _manifestCachePath;
    [SerializeField] private ConnectionPointData _outputPointResult;
    [SerializeField] private ConnectionPointData _outputPointPass;
    [SerializeField] private string _excludeResourcesExtensions;

    #endregion

    #region Properties

    public override string ActiveStyle
    {
        get { return "node 8 on"; }
    }

    public override string InactiveStyle
    {
        get { return "node 8"; }
    }

    public override string Category
    {
        get { return "Custom"; }
    }

    public override NodeOutputSemantics NodeInputType
    {
        get { return NodeOutputSemantics.Any; }
    }

    public override NodeOutputSemantics NodeOutputType
    {
        get { return NodeOutputSemantics.Any; }
    }

    #endregion

    #region Public Members

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
    {
        if (!InternalEditorUtility.inBatchMode)
        {
            _serverBundlesFolder = editor.DrawFolderSelector("Server bundles folder", "Choose server bundles folder", _serverBundlesFolder, string.Empty, s => s);
            EditorGUILayout.HelpBox("At build machine this folder will be re-setted automatically.", MessageType.Info);
        }

        _manifestFileName = EditorGUILayout.TextField("Manifest file name", _manifestFileName);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.HelpBox(
            "Specify pathes that will be stripped from asset pathes. Each path on new line. Example:\n" +
            "You have an asset \"Assets/Resources/GUI/Dialogs/MyDialog.prefab\"\n" + "You want this asset as \"Dialogs/MyDialog\" in bundle.\n" +
            "The stripped path will be \"Recources/GUI", MessageType.Info);
        _pathesToStrip = EditorGUILayout.TextArea(_pathesToStrip);
        EditorGUILayout.Separator();
        EditorGUILayout.Separator();

        EditorGUILayout.HelpBox("Specify files extentions, that will not include in manifest assets. (separator - \",\")", MessageType.Info);
        _excludeResourcesExtensions = EditorGUILayout.TextArea(_excludeResourcesExtensions);
    }

    public override void Initialize(NodeData data)
    {
        data.AddInputPoint("Bundles");
        data.AddInputPoint("Assets Groups");

        _outputPointResult = data.AddOutputPoint("Result");
        _outputPointPass = data.AddOutputPoint("Pass throw");
    }

    public override Node Clone(NodeData newData)
    {
        throw new NotImplementedException();
    }

    public override void Prepare(BuildTarget target, NodeData nodeData, IEnumerable<PerformGraph.AssetGroups> incoming, IEnumerable<ConnectionData> connectionsToOutput,
        PerformGraph.Output outputFunc)
    {
        if (outputFunc != null)
        {
            ConnectionData destinationResult = null;
            ConnectionData destinationPass = null;

            if (connectionsToOutput != null && connectionsToOutput.Count() == 2)
            {
                destinationResult = connectionsToOutput.First(data => data.FromNodeConnectionPointId == _outputPointResult.Id);
                destinationPass = connectionsToOutput.First(data => data.FromNodeConnectionPointId == _outputPointPass.Id);
            }

            if (incoming != null)
            {
                if (destinationPass != null)
                {
                    if (incoming.First() != null)
                    {
                        outputFunc(destinationPass, incoming.First().assetGroups);
                    }
                    else
                    {
                        outputFunc(destinationPass, new Dictionary<string, List<AssetReference>>());
                    }
                }

                if (destinationResult != null)
                {
                    if (incoming.First() != null)
                    {
                        var key = "0";
                        var outputDict = new Dictionary<string, List<AssetReference>>();
                        outputDict[key] = new List<AssetReference>();

                        foreach (var agAssetGroup in incoming.First().assetGroups)
                        {
                            foreach (var assetReference in agAssetGroup.Value)
                            {
                                if (assetReference.extension != ".manifest" && !_ignoreList.Contains(assetReference.fileNameAndExtension))
                                {
                                    var bundle = AssetReferenceDatabase.GetAssetBundleReference(assetReference.path);
                                    outputDict[key].Add(bundle);
                                }
                            }
                        }

                        if (outputDict[key].Count > 0)
                        {
                            var customManifestPath = Path.Combine(Path.GetDirectoryName(outputDict[key][0].path), _manifestFileName + ".json");
                            _manifestCachePath = AssetReferenceDatabase.GetAssetBundleReference(customManifestPath);
                            outputDict[key].Add(_manifestCachePath);
                        }

                        outputFunc(destinationResult, outputDict);
                    }
                    else
                    {
                        outputFunc(destinationResult, new Dictionary<string, List<AssetReference>>());
                    }
                }
            }
        }
    }

    public override void Build(BuildTarget target, NodeData nodeData, IEnumerable<PerformGraph.AssetGroups> incoming, IEnumerable<ConnectionData> connectionsToOutput,
        PerformGraph.Output outputFunc, Action<NodeData, string, float> progressFunc)
    {
        if (incoming == null || incoming.Count() < 2)
        {
            return;
        }

        _serverBundlesFolder = InternalEditorUtility.inBatchMode ? BundlesBuildAgent.ServerBundlesPath : _serverBundlesFolder;
        var newManifest = CreateManifest(incoming);
        var oldManifest = GetOldManifest();
        UpdateBundlesUx(newManifest, oldManifest);
        CopyManifest(newManifest);
    }

    #endregion

    #region Private Members

    private void CopyManifest(AssetBundleManifest manifest)
    {
        if (_manifestCachePath == null)
        {
            throw new Exception("ManifestCachePath is null");
        }

        var jsonSerializer = new JsonSerializer(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.SerializeString(manifest);

        var pathToCopy = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), _manifestCachePath.path);
        File.WriteAllText(pathToCopy, manifestAsJson);
    }

    private AssetBundleManifest GetOldManifest()
    {
        var jsonSerializer = new JsonSerializer(new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }, Encoding.UTF8);
        AssetBundleManifest manifest = null;

        var filePath = Path.Combine(_serverBundlesFolder, _manifestFileName + ".json");
        Debug.Log(string.Format("Loading old manifest by path {0}", filePath));

        if (File.Exists(filePath))
        {
            var manifestContent = File.ReadAllText(filePath);
            manifest = jsonSerializer.DeserializeString<AssetBundleManifest>(manifestContent);
            Debug.Log("Old manifest loaded succesfully.");
        }
        else
        {
            Debug.Log("Old manifest loading failed.");
        }

        return manifest;
    }

    private AssetBundleManifest CreateManifest(IEnumerable<PerformGraph.AssetGroups> incoming)
    {
        var incommingAssets = incoming.First();
        var incomigGroups = incoming.Last(); // ���������� ������
        var customManifest = new AssetBundleManifest();

        foreach (var ag in incommingAssets.assetGroups)
        {
            var assetInfo = ag.Value;
            CreateBundleInfos(assetInfo, customManifest, incomigGroups);
            CreateAssetInfos(assetInfo, customManifest);
        }

        CreateNotBundlesAssetsInfos(customManifest);

        return customManifest;
    }

    private void CreateBundleInfos(List<AssetReference> assetInfos, AssetBundleManifest manifest, PerformGraph.AssetGroups incomigGroups)
    {
        foreach (var assetReference in assetInfos)
        {
            if (IsUnityManifest(assetReference))
            {
                var unityManifest = assetReference;
                var bundleInfo = new BundleInfo
                {
                    Name = unityManifest.fileName,
                    CRC = GetCRCFromManifest(unityManifest.path)
                };

                var rawAssetPathesFromManifest = GetRawAssetPathesFromManifest(unityManifest.path);

                if (rawAssetPathesFromManifest.Any())
                {
                    // ���� � ���������� ���� "������" - ������� � ������ ����� ����� �� �������� ������
                    // ���� �����, �� �������� ������ ����� ��������� ��� ���������� ������, � ������� �����
                    // ���� �����.
                    var rawAssetPath = rawAssetPathesFromManifest[0];
                    rawAssetPath = RemoveAssetsFolderFromPath(rawAssetPath);

                    foreach (var incomigGroup in incomigGroups.assetGroups)
                    {
                        var groupFound = false;

                        foreach (var reference in incomigGroup.Value)
                        {
                            if (reference.path.Contains(rawAssetPath))
                            {
                                groupFound = true;
                                bundleInfo.CustomInfo.Add("GroupName", incomigGroup.Key);
                                break;
                            }
                        }

                        if (groupFound)
                        {
                            break;
                        }
                    }
                }

                foreach (var rawAssetPath in rawAssetPathesFromManifest)
                {
                    var pathWithoutAssetFolder = RemoveAssetsFolderFromPath(rawAssetPath);
                    var trunkatedAssetPath = TruncatePathFromManifest(pathWithoutAssetFolder);
                    bundleInfo.AssetInfos.Add(trunkatedAssetPath);
                }

                Debug.Log(string.Format("Bundle {0} with {1} assets added to manifest.", bundleInfo.Name, bundleInfo.AssetInfos.Count));
                manifest.BundleInfos.Add(bundleInfo.Name, bundleInfo);
            }
        }
    }

    private void CreateAssetInfos(List<AssetReference> assetInfos, AssetBundleManifest manifest)
    {
        foreach (var assetReference in assetInfos)
        {
            if (IsUnityManifest(assetReference))
            {
                var unityManifest = assetReference;
                var rawAssetPathesFromManifest = GetRawAssetPathesFromManifest(unityManifest.path);

                foreach (var rawAssetPath in rawAssetPathesFromManifest)
                {
                    var pathWithoutAssetFolder = RemoveAssetsFolderFromPath(rawAssetPath);
                    var trunkatedAssetPath = TruncatePathFromManifest(pathWithoutAssetFolder);

                    var assetInfo = new AssetInfo();
                    assetInfo.Name = trunkatedAssetPath;
                    assetInfo.GameAssetTypeTag = GetAssetType(trunkatedAssetPath).ToString();
                    assetInfo.CustomInfo.Add("nameInBundle", rawAssetPath);

                    BundleInfo bundleInfoWithAsset = null;

                    foreach (var bundleInfo in manifest.BundleInfos)
                    {
                        if (bundleInfo.Value.AssetInfos.Contains(assetInfo.Name))
                        {
                            bundleInfoWithAsset = bundleInfo.Value;
                            Debug.Log(string.Format("Asset {0} containing in bundle {1}", assetInfo.Name, bundleInfoWithAsset.Name));
                            break;
                        }
                    }

                    var deps = AssetDatabase.GetDependencies(rawAssetPath);
                    Debug.Log(string.Format("Asset {0} ({2}) has {1} dependencies:", assetInfo.Name, deps.Length, rawAssetPath));

                    foreach (var assetDependency in deps)
                    {
                        Debug.Log("Asset Dependency: " + assetDependency);

                        // � ����������� �������� � ��� �����, ����������� ������� �� ����
                        if (assetDependency == rawAssetPath)
                        {
                            continue;
                        }

                        var trunkatedAssetDependencyPath = NormalizePathFromManifest(assetDependency);
                        trunkatedAssetDependencyPath = RemoveAssetsFolderFromPath(trunkatedAssetDependencyPath);
                        trunkatedAssetDependencyPath = TruncatePathFromManifest(trunkatedAssetDependencyPath);

                        foreach (var bundleInfo in manifest.BundleInfos)
                        {
                            if (bundleInfo.Value.AssetInfos.Contains(trunkatedAssetDependencyPath))
                            {
                                var dependentBundleName = bundleInfo.Value.Name;
                                Debug.Log(string.Format("Asset dependency {0} was found in {1} bundle", assetDependency, dependentBundleName));

                                // �� ���������� � ����������� �����, ������� ��� �������� ���� �����. � �������.
                                if (!assetInfo.Dependencies.Contains(dependentBundleName) && bundleInfoWithAsset != bundleInfo.Value)
                                {
                                    Debug.Log(string.Format("Asset dependency added: {0}", dependentBundleName));
                                    assetInfo.Dependencies.Add(dependentBundleName);
                                }
                            }
                        }
                    }

                    Debug.Log(string.Format("Asset {0} with {1} dependencies added to Manifest", assetInfo.Name, assetInfo.Dependencies.Count));
                    manifest.AssetInfos.Add(assetInfo.Name, assetInfo);
                }
            }
        }
    }

    private void CreateNotBundlesAssetsInfos(AssetBundleManifest customManifest)
    {
        var resourcesDirectory = Application.dataPath + "/Resources";
        var resources = new List<string>();
        GetAllResources(resourcesDirectory, ref resources);
        var assetsPath = Application.dataPath;

        foreach (var resource in resources)
        {
            var resourceNormalizedPath = resource;
            var resourceExtension = Path.GetExtension(resourceNormalizedPath);

            // �������� ������ ���� �� �������
            if (resourceNormalizedPath.StartsWith(assetsPath))
            {
                resourceNormalizedPath = resource.Remove(0, assetsPath.Length + 1);
            }

            resourceNormalizedPath = resourceNormalizedPath.Replace("\\", "/");
            var trunkatedAssetPath = TruncatePathFromManifest(resourceNormalizedPath);

            if (!customManifest.AssetInfos.ContainsKey(trunkatedAssetPath))
            {
                var assetInfo = new AssetInfo();
                assetInfo.Name = trunkatedAssetPath;
                assetInfo.GameAssetTypeTag = GetAssetType(trunkatedAssetPath).ToString();

                // �������� ���������� �� ����
                var nameInResources = trunkatedAssetPath;
                if (!string.IsNullOrEmpty(resourceExtension))
                {
                    nameInResources = nameInResources.Replace(resourceExtension, "");
                }

                assetInfo.CustomInfo.Add("nameInResources", nameInResources);

                customManifest.AssetInfos.Add(assetInfo.Name, assetInfo);
            }
        }
    }

    private void GetAllResources(string currenctRootDirectory, ref List<string> resources)
    {
        var files = Directory.GetFiles(currenctRootDirectory);
        var fileteredFiles = new List<string>();

        if (!string.IsNullOrEmpty(_excludeResourcesExtensions))
        {
            var excludeExtensions = _excludeResourcesExtensions.Split(',').ToList();

            foreach (var file in files)
            {
                if (!excludeExtensions.Contains(Path.GetExtension(file).TrimStart('.')))
                {
                    fileteredFiles.Add(file);
                }
            }
        }

        resources.AddRange(fileteredFiles);

        var directories = Directory.GetDirectories(currenctRootDirectory);
        foreach (var directory in directories)
        {
            GetAllResources(directory, ref resources);
        }
    }

    private bool IsUnityManifest(AssetReference asset)
    {
        return asset.extension == ".manifest" && !_ignoreList.Contains(asset.fileNameAndExtension);
    }

    private void UpdateBundlesUx(AssetBundleManifest newManifest, AssetBundleManifest oldManifest)
    {
        Debug.Log("Updating bundles UX");
        if (oldManifest == null)
        {
            Debug.Log("OldManifest was not found. Cant update UXs");
            return;
        }

        foreach (var bundleInfo in newManifest.BundleInfos)
        {
            BundleInfo oldBundleInfo = null;
            if (oldManifest.BundleInfos.TryGetValue(bundleInfo.Key, out oldBundleInfo))
            {
                if (bundleInfo.Value.CRC != oldBundleInfo.CRC || !bundleInfo.Value.CustomInfo.Equals(oldBundleInfo.CustomInfo))
                {
                    Debug.Log(string.Format("Updating {0} bundle UX from {1} to {2}", bundleInfo.Value.Name, bundleInfo.Value.Ux, bundleInfo.Value.Ux + 1));
                    bundleInfo.Value.Ux = oldBundleInfo.Ux + 1;
                }
            }
        }
    }

    private AssetTagCategory GetAssetType(string path)
    {
        var extension = Path.GetExtension(path);
        switch (extension)
        {
            case ".csv":
            case ".json":
            case ".txt":
            case ".html":
            case ".htm":
            case ".xml":
            case ".bytes":
            case ".yaml":
            case ".fnt":
                return AssetTagCategory.Text;

            case ".jpg":
            case ".jpeg":
            case ".tga":
            case ".psd":
            case ".png":
                return AssetTagCategory.Image;

            case ".prefab":
                return AssetTagCategory.Prefab;

            case ".mat":
                return AssetTagCategory.Material;

            case ".mp3":
            case ".ogg":
            case ".wav":
                return AssetTagCategory.Audio;

            case ".shader":
                return AssetTagCategory.Shader;

            case ".3ds":
            case ".fbx":
            case ".obj":
            case ".max":
                return AssetTagCategory.Model3D;
            case ".anim":
                return AssetTagCategory.Animation;

            default:
                return AssetTagCategory.Unknown;
        }
    }

    private string GetCRCFromManifest(string manifestPath)
    {
        foreach (var line in File.ReadAllLines(manifestPath))
        {
            if (line.StartsWith("CRC:"))
            {
                return line.Replace("CRC: ", "");
            }
        }

        Debug.LogError("Not found crc attribute");
        return null;
    }

    private List<string> GetRawAssetPathesFromManifest(string manifestPath)
    {
        var assetsList = new List<string>();
        var nextLineIsAssetPath = false;
        foreach (var line in File.ReadAllLines(manifestPath))
        {
            if (line.StartsWith("Assets:"))
            {
                nextLineIsAssetPath = true;
            }
            else
            {
                if (nextLineIsAssetPath && line.StartsWith("- "))
                {
                    var path = NormalizePathFromManifest(line);
                    if (assetsList.Contains(path))
                    {
                        Debug.LogError(string.Format("path {0} dublicate!", path));
                    }

                    assetsList.Add(path);
                }
                else
                {
                    nextLineIsAssetPath = false;
                }
            }
        }

        return assetsList;
    }

    private string NormalizePathFromManifest(string assetPath)
    {
        var path = assetPath.Replace("\"", "/");
        path = path.Replace("- ", "");
        return path;
    }

    private string RemoveAssetsFolderFromPath(string path)
    {
        if (path.StartsWith("Assets/"))
        {
            return path.Replace("Assets/", "");
        }

        return path;
    }

    private string TruncatePathFromManifest(string path)
    {
        foreach (var resourcesPath in _pathesToStrip.Split('\n'))
        {
            var resourcePathNormalized = resourcesPath.Replace("\"", "/");

            if (!resourcePathNormalized.EndsWith("/"))
            {
                resourcePathNormalized += "/";
            }

            if (path.StartsWith(resourcePathNormalized))
            {
                return path.Replace(resourcePathNormalized, "");
            }
        }

        return path;
    }

    #endregion
}
#endif
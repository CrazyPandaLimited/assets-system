#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrazyPanda.UnityCore.ResourcesSystem;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using CustomManifest = CrazyPanda.UnityCore.ResourcesSystem.AssetBundleManifest;
using Debug = UnityEngine.Debug;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;

[CustomNode("CoreResourceSystem/Bundles manifest creator", 1000)]
public class NodeBundlesManifestCreator : Node
{
    #region Private Fields

    [SerializeField] private SerializableMultiTargetString _enabledOptions;
    [SerializeField] private SerializableMultiTargetString _resourcesPathes;
    [SerializeField] private SerializableMultiTargetString _manifestFileName;


    private string _manifestDirectoryInternalRelative;
    // TODO: Add MacOS
    private readonly List<string> _ignoreList = new List<string>
        {"Windows.manifest", "Android.manifest", "iOS.manifest", "WebGL.manifest", "Windows", "Android", "iOS", "WebGL"};

    #endregion

    #region Properties

    public override string ActiveStyle
    {
        get { return "node 4 on"; }
    }

    public override string InactiveStyle
    {
        get { return "node 4"; }
    }

    public override string Category
    {
        get { return "CoreResourceSystem"; }
    }

    public override Model.NodeOutputSemantics NodeInputType
    {
        get { return Model.NodeOutputSemantics.AssetBundles; }
    }

    public override Model.NodeOutputSemantics NodeOutputType
    {
        get { return Model.NodeOutputSemantics.Any; }
    }

    #endregion

    #region Public Members

    public override void Initialize(Model.NodeData data)
    {
        _enabledOptions = new SerializableMultiTargetString();
        data.AddDefaultInputPoint();
        data.AddDefaultOutputPoint();
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new NodeBundlesManifestCreator();
        newNode._enabledOptions = new SerializableMultiTargetString(_enabledOptions);
        newData.AddDefaultInputPoint();
        newData.AddDefaultOutputPoint();
        return newNode;
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor,
        Action onValueChanged)
    {
        editor.UpdateNodeName(node);

        GUILayout.Space(10f);

        editor.DrawPlatformSelector(node);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            var disabledScope = editor.DrawOverrideTargetToggle(node, _enabledOptions.ContainsValueOf(editor.CurrentEditingGroup),
                b =>
                {
                    using (new RecordUndoScope("Remove Target Platform Settings", node, true))
                    {
                        if (b)
                        {
                            _enabledOptions[editor.CurrentEditingGroup] = _enabledOptions.DefaultValue;
                            _manifestFileName[editor.CurrentEditingGroup] = _manifestFileName.DefaultValue;
                            _resourcesPathes[editor.CurrentEditingGroup] = _resourcesPathes.DefaultValue;
                        }
                        else
                        {
                            _enabledOptions.Remove(editor.CurrentEditingGroup);
                            _manifestFileName.Remove(editor.CurrentEditingGroup);
                            _resourcesPathes.Remove(editor.CurrentEditingGroup);
                        }

                        onValueChanged();
                    }
                });

            using (disabledScope)
            {
                var newValue = EditorGUILayout.TextField("Output file name", _manifestFileName[editor.CurrentEditingGroup]);

                if (_manifestFileName[editor.CurrentEditingGroup] != newValue)
                {
                    using (new RecordUndoScope("Change Output file name", node, true))
                    {
                        _manifestFileName[editor.CurrentEditingGroup] = newValue;
                        onValueChanged();
                    }
                }

                EditorGUILayout.LabelField(
                    "Specify the folders, you want to cut from\nassets name. Use this if you have\none folder for all resources and want to\nmake assets name alb shorter.\nEach folder from a new line\nExample:\nMyStaffs/Dogs\nMyStaffs2/Cats",
                    GUILayout.Height(120));

                newValue = EditorGUILayout.TextArea(_resourcesPathes[editor.CurrentEditingGroup]);

                if (_resourcesPathes[editor.CurrentEditingGroup] != newValue)
                {
                    using (new RecordUndoScope("Change Resources pathes", node, true))
                    {
                        _resourcesPathes[editor.CurrentEditingGroup] = newValue;
                        onValueChanged();
                    }
                }
            }
        }
    }

    public override void Prepare(BuildTarget target, Model.NodeData node,
        IEnumerable<PerformGraph.AssetGroups> incoming, IEnumerable<Model.ConnectionData> connectionsToOutput,
        PerformGraph.Output Output)
    {
        if (Output != null)
        {
            var destination = connectionsToOutput == null || !connectionsToOutput.Any() ? null : connectionsToOutput.First();

            if (incoming != null)
            {
                var key = "0";
                var outputDict = new Dictionary<string, List<AssetReference>>();
                outputDict[key] = new List<AssetReference>();
                foreach (var ag in incoming)
                {
                    foreach (var agAssetGroup in ag.assetGroups)
                    {
                        foreach (var assetReference in agAssetGroup.Value)
                        {
                            if (assetReference.extension != ".manifest" && !_ignoreList.Contains(assetReference.fileNameAndExtension))
                            {
                                var bundle = AssetReferenceDatabase.GetAssetBundleReference(assetReference.path);
                                Debug.Log(bundle.fileName);
                                outputDict[key].Add(bundle);
                            }
                        }
                    }
                }

                var customPath = GetCustomManifestPath(target, node);
                var customManifest = AssetReferenceDatabase.GetAssetBundleManifestReference(customPath);
                outputDict[key].Add(customManifest);

                Output(destination, outputDict);
            }
            else
            {
                Output(destination, new Dictionary<string, List<AssetReference>>());
            }
        }
        else
        {
            Debug.LogError("Output node not set!!! You need attach some node to NodeBundlesManifestCreator!!!");
        }
    }

    public override void Build(BuildTarget target, Model.NodeData node, IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput, PerformGraph.Output outputFunc,
        Action<Model.NodeData, string, float> progressFunc)
    {
        var customManifest = new CustomManifest();
        var assetsToBundleMap = new Dictionary<string, string>();

        foreach (var assetGroups in incoming)
        {
            foreach (var ag in assetGroups.assetGroups)
            {
                var assetInfos = ag.Value;
                ProcessingAssetInfos(target, assetInfos, assetsToBundleMap, customManifest);
            }
        }

        foreach (var assetToBundle in assetsToBundleMap)
        {
            var assetInfo = new AssetInBundleInfo();
            assetInfo.Name = assetToBundle.Key;
            assetInfo.GameAssetTypeTag = GetAssetType(assetToBundle.Key).ToString();

            foreach (var assetDependency in AssetDatabase.GetDependencies(assetToBundle.Key))
            {
                if (assetDependency == assetToBundle.Key)
                {
                    continue;
                }

                if (assetsToBundleMap.ContainsKey(assetDependency))
                {
                    var bundleDep = assetsToBundleMap[assetDependency];

                    if (!assetInfo.Dependencies.Contains(bundleDep) && bundleDep != assetToBundle.Value)
                    {
                        assetInfo.Dependencies.Add(bundleDep);
                    }
                }
            }

            customManifest.AssetInfos.Add(assetInfo.Name, assetInfo);
        }

        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.SerializeString(customManifest);

        var path = GetCustomManifestPath(target, node);
        File.WriteAllText(path, manifestAsJson);
    }

    public override bool IsValidInputConnectionPoint(Model.ConnectionPointData point)
    {
        return true;
    }

    #endregion

    #region Private Members

    private void ProcessingAssetInfos(BuildTarget target, List<AssetReference> assetInfos, Dictionary<string, string> assetsToBundleMap,
        CustomManifest manifest)
    {
        foreach (var assetReference in assetInfos)
        {
            if (assetReference.extension == ".manifest" && !_ignoreList.Contains(assetReference.fileNameAndExtension))
            {
                var bundleInfo = new BundleInfo();
                bundleInfo.Name = assetReference.fileName;
                bundleInfo.CRC = GetCRCFromManifest(assetReference.path);
                bundleInfo.Hash = GetHashFromManifest(assetReference.path);

                foreach (var assetPath in GetAssetPathesFromManifest(target, assetReference.path))
                {
                    bundleInfo.AssetInfos.Add(assetPath);

                    if (assetsToBundleMap.ContainsKey(assetPath))
                    {
                        Debug.LogError(string.Format(" Asset: {0} used in two or more bundles!!", assetPath));
                    }

                    assetsToBundleMap.Add(assetPath, assetReference.fileName);
                }

                manifest.BundleInfos.Add(bundleInfo.Name, bundleInfo);
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
            case ".fnt": return AssetTagCategory.Text;

            case ".jpg":
            case ".jpeg":
            case ".tga":
            case ".psd":
            case ".png": return AssetTagCategory.Image;

            case ".prefab": return AssetTagCategory.Prefab;

            case ".mat": return AssetTagCategory.Material;

            case ".mp3":
            case ".ogg":
            case ".wav": return AssetTagCategory.Audio;

            case ".shader": return AssetTagCategory.Shader;

            case ".3ds":
            case ".fbx":
            case ".obj":
            case ".max": return AssetTagCategory.Model3D;
            case ".anim": return AssetTagCategory.Animation;

            default: return AssetTagCategory.Unknown;
        }
    }

    private string GetCustomManifestPath(BuildTarget target, Model.NodeData node)
    {
        var output = FileUtility.EnsureWorkingCacheDirExists(target, node, "");
        var outputFileName = (string.IsNullOrEmpty(_manifestFileName[target]) ? "RSBBundlesManifest" : _manifestFileName[target]) + ".json";
        return Path.Combine(output, outputFileName);
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
    
    private string GetHashFromManifest(string manifestPath)
    {
        var allLines = File.ReadAllLines(manifestPath);

        if (allLines[2].Trim().StartsWith("Hashes:") &&
            allLines[3].Trim().StartsWith("AssetFileHash:") &&
            allLines[5].Trim().StartsWith("Hash: "))
        {
            return allLines[5].Trim().Replace("Hash: ", "");
        }

        Debug.LogWarning("Not found hash attribute");
        return null;
    }

    private List<string> GetAssetPathesFromManifest(BuildTarget target, string manifestPath)
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
                    var path = ProcessPathFromManifest(target, line);
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

    private string ProcessPathFromManifest(BuildTarget target, string assetPath)
    {
        var path = assetPath.Replace("\"", "/");
        path = path.Replace("- ", "");

        foreach (string resourcesPath in _resourcesPathes[target].Split('\n'))
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
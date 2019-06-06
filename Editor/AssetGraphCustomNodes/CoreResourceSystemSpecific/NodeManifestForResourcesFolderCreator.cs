#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

[CustomNode("Custom/Node Resources folder assets Manifest", 1000)]
public class NodeManifestForResourcesFolderCreator : Node
{
    #region Private Fields

    [SerializeField] private SerializableMultiTargetString m_myValue;

    private string _manifestDirectoryInternalRelative;

    [SerializeField] private string _manifestDirectoryExport;
    [SerializeField] private bool _copyManifestExternal;
    [SerializeField] private string _resourcesPathes;

    // TODO: Add MacOS
    private readonly List<string> _ignoreList = new List<string>
        {"Windows.manifest", "Android.manifest", "iOS.manifest", "WebGL.manifest", "Windows", "Android", "iOS", "WebGL"};

    private string _manifestFileName;

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

    public override Model.NodeOutputSemantics NodeInputType
    {
        get { return Model.NodeOutputSemantics.Assets; }
    }

    public override Model.NodeOutputSemantics NodeOutputType
    {
        get { return Model.NodeOutputSemantics.Any; }
    }

    #endregion

    #region Public Members

    public override void Initialize(Model.NodeData data)
    {
        m_myValue = new SerializableMultiTargetString();
        data.AddDefaultInputPoint();
        data.AddDefaultOutputPoint();
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new NodeManifestForResourcesFolderCreator();
        newNode.m_myValue = new SerializableMultiTargetString(m_myValue);
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
            var disabledScope = editor.DrawOverrideTargetToggle(node, m_myValue.ContainsValueOf(editor.CurrentEditingGroup),
                b =>
                {
                    using (new RecordUndoScope("Remove Target Platform Settings", node, true))
                    {
                        if (b)
                        {
                            m_myValue[editor.CurrentEditingGroup] = m_myValue.DefaultValue;
                        }
                        else
                        {
                            m_myValue.Remove(editor.CurrentEditingGroup);
                        }

                        onValueChanged();
                    }
                });

            using (disabledScope)
            {
                _copyManifestExternal = EditorGUILayout.Toggle("Copy manifest to external folder", _copyManifestExternal);
                _manifestDirectoryExport = editor.DrawFolderSelector("Output Directory", "Select Output Folder",
                    _manifestDirectoryExport, Application.dataPath + "/../", folderSelected => folderSelected);
                _manifestFileName = EditorGUILayout.TextField("Output file name", _manifestFileName);
                EditorGUILayout.LabelField(
                    "Specify the folders, you want to cut from\nassets name. Use this if you have\none folder for all resources and want to\nmake assets name alb shorter.\nEach folder from a new line\nExample:\nMyStaffs/Dogs\nMyStaffs2/Cats",
                    GUILayout.Height(120));
                _resourcesPathes = EditorGUILayout.TextArea(_resourcesPathes);
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
                var internalPath = string.Empty;
                foreach (var ag in incoming)
                {
                    foreach (var agAssetGroup in ag.assetGroups)
                    {
                        foreach (var assetReference in agAssetGroup.Value)
                        {
                            internalPath = assetReference.path;
                            break;
                        }

                        break;
                    }

                    break;
                }

                _manifestDirectoryInternalRelative = Path.GetDirectoryName(internalPath);
                Debug.Log("Custom manifest folder choose: " + _manifestDirectoryInternalRelative);

                var customManifest =
                    AssetReferenceDatabase.GetAssetBundleReference(GetCustomManifestPath(_manifestDirectoryInternalRelative));
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
        var customManifest = new AssetsManifest<AssetInfo>();

        foreach (var assetGroups in incoming)
        {
            foreach (var ag in assetGroups.assetGroups)
            {
                var assetInfos = ag.Value;
                ProcessingAssetInfos(assetInfos, customManifest);
            }
        }

        var jsonSerializer =
            new JsonSerializer(new JsonSerializerSettings {Formatting = Formatting.Indented}, Encoding.UTF8);
        var manifestAsJson = jsonSerializer.SerializeString(customManifest);

        var rootFolder = Path.Combine(Directory.GetParent(Application.dataPath).ToString(),
            _manifestDirectoryInternalRelative);
        if (!Directory.Exists(rootFolder))
        {
            Directory.CreateDirectory(rootFolder);
        }

        if (_copyManifestExternal && !String.IsNullOrEmpty(_manifestDirectoryExport))
        {
            File.WriteAllText(GetCustomManifestPath(_manifestDirectoryExport), manifestAsJson);
            Process.Start(_manifestDirectoryExport);
        }
        else
        {
            File.WriteAllText(GetCustomManifestPath(rootFolder), manifestAsJson);
        }
    }

    public override bool IsValidInputConnectionPoint(Model.ConnectionPointData point)
    {
        return true;
    }

    #endregion

    #region Private Members

    private void ProcessingAssetInfos(List<AssetReference> assetInfos,
        AssetsManifest<AssetInfo> manifest)
    {
        foreach (var assetReference in assetInfos)
        {
            if (!_ignoreList.Contains(assetReference.fileNameAndExtension))
            {
                var assetInfo = new AssetInfo();
                assetInfo.name = ProcessPathFromManifest(assetReference.path).Replace(assetReference.extension, "");
                assetInfo.ext = assetReference.extension;
                manifest._assetsInfos.Add(assetInfo.name, assetInfo);
            }
        }
    }


    private string GetCustomManifestPath(string rootFolder)
    {
        var outputFileName = _manifestFileName ?? "CustomManifest";
        outputFileName += ".json";
        return Path.Combine(rootFolder, outputFileName);
    }

    private string ProcessPathFromManifest(string assetPath)
    {
        var path = assetPath.Replace("\"", "/");
        path = path.Replace("- ", "");

        foreach (string resourcesPath in _resourcesPathes.Split('\n'))
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
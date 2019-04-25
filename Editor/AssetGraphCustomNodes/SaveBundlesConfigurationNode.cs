#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;
using JsonSerializer = CrazyPanda.UnityCore.Serialization.JsonSerializer;

[CustomNode("Configure Bundle/Save bundle config", 1000)]
public class SaveBundlesConfigurationNode : Node
{
    public enum OutputOption : int
    {
        BuildInCacheDirectory,
        ErrorIfNoOutputDirectoryFound
    }

    [SerializeField] private SerializableMultiTargetInt _enabledOptions;
    [SerializeField] private SerializableMultiTargetString _outputDir;
    [SerializeField] private SerializableMultiTargetInt _outputOption;
    [SerializeField] private SerializableMultiTargetString _fileName;

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
        get { return "Configure"; }
    }

    public override Model.NodeOutputSemantics NodeInputType
    {
        get { return Model.NodeOutputSemantics.AssetBundleConfigurations; }
    }

    public override Model.NodeOutputSemantics NodeOutputType
    {
        get { return Model.NodeOutputSemantics.None; }
    }

    public override void Initialize(Model.NodeData data)
    {
        _enabledOptions = new SerializableMultiTargetInt();
        _outputDir = new SerializableMultiTargetString();
        _fileName = new SerializableMultiTargetString();
        _outputOption = new SerializableMultiTargetInt();
        data.AddDefaultInputPoint();
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new SaveBundlesConfigurationNode();
        newNode._enabledOptions = new SerializableMultiTargetInt(_enabledOptions);
        newNode._outputDir = new SerializableMultiTargetString(_outputDir);
        newNode._fileName = new SerializableMultiTargetString(_fileName);
        newNode._outputOption = new SerializableMultiTargetInt(_outputOption);
        newData.AddDefaultInputPoint();
        return newNode;
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
    {
        if (_enabledOptions == null)
        {
            return;
        }

        EditorGUILayout.HelpBox("Сохраняет конфигурацию бандлей в json", MessageType.Info);
        editor.UpdateNodeName(node);

        GUILayout.Space(10f);

        //Show target configuration tab
        editor.DrawPlatformSelector(node);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            var disabledScope = editor.DrawOverrideTargetToggle(node, _enabledOptions.ContainsValueOf(editor.CurrentEditingGroup), (bool enabled) =>
            {
                using (new RecordUndoScope("Remove Target Bundle Options", node, true))
                {
                    if (enabled)
                    {
                        _enabledOptions[editor.CurrentEditingGroup] = _enabledOptions.DefaultValue;
                        _outputDir[editor.CurrentEditingGroup] = _outputDir.DefaultValue;
                        _outputOption[editor.CurrentEditingGroup] = _outputOption.DefaultValue;
                        _fileName[editor.CurrentEditingGroup] = _fileName.DefaultValue;
                    }
                    else
                    {
                        _enabledOptions.Remove(editor.CurrentEditingGroup);
                        _outputDir.Remove(editor.CurrentEditingGroup);
                        _outputOption.Remove(editor.CurrentEditingGroup);
                        _fileName.Remove(editor.CurrentEditingGroup);
                    }

                    onValueChanged();
                }
            });

            using (disabledScope)
            {
                OutputOption opt = (OutputOption) _outputOption[editor.CurrentEditingGroup];
                var newOption = (OutputOption) EditorGUILayout.EnumPopup("Output Option", opt);
                if (newOption != opt)
                {
                    using (new RecordUndoScope("Change Output Option", node, true))
                    {
                        _outputOption[editor.CurrentEditingGroup] = (int) newOption;
                        onValueChanged();
                    }
                }

                using (new EditorGUI.DisabledScope(opt == OutputOption.BuildInCacheDirectory))
                {
                    var newDirPath = editor.DrawFolderSelector("Output Directory", "Select Output Folder",
                        _outputDir[editor.CurrentEditingGroup],
                        Application.dataPath + "/../",
                        (string folderSelected) =>
                        {
                            var projectPath = Directory.GetParent(Application.dataPath).ToString();

                            if (projectPath == folderSelected)
                            {
                                folderSelected = string.Empty;
                            }
                            else
                            {
                                var index = folderSelected.IndexOf(projectPath);
                                if (index >= 0)
                                {
                                    folderSelected = folderSelected.Substring(projectPath.Length + index);
                                    if (folderSelected.IndexOf('/') == 0)
                                    {
                                        folderSelected = folderSelected.Substring(1);
                                    }
                                }
                            }

                            return folderSelected;
                        }
                    );
                    if (newDirPath != _outputDir[editor.CurrentEditingGroup])
                    {
                        using (new RecordUndoScope("Change Output Directory", node, true))
                        {
                            _outputDir[editor.CurrentEditingGroup] = newDirPath;
                            onValueChanged();
                        }
                    }

                    var outputDir = PrepareOutputDirectory(BuildTargetUtility.GroupToTarget(editor.CurrentEditingGroup), node.Data, false, false);

                    if (opt == OutputOption.ErrorIfNoOutputDirectoryFound &&
                        editor.CurrentEditingGroup != BuildTargetGroup.Unknown &&
                        !string.IsNullOrEmpty(_outputDir[editor.CurrentEditingGroup]) &&
                        !Directory.Exists(outputDir))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField(outputDir + " does not exist.");
                            if (GUILayout.Button("Create directory"))
                            {
                                Directory.CreateDirectory(outputDir);
                            }
                        }

                        EditorGUILayout.Space();

                        string parentDir = Path.GetDirectoryName(_outputDir[editor.CurrentEditingGroup]);
                        if (Directory.Exists(parentDir))
                        {
                            EditorGUILayout.LabelField("Available Directories:");
                            string[] dirs = Directory.GetDirectories(parentDir);
                            foreach (string s in dirs)
                            {
                                EditorGUILayout.LabelField(s);
                            }
                        }

                        EditorGUILayout.Space();
                    }

                    using (new EditorGUI.DisabledScope(!Directory.Exists(outputDir)))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
#if UNITY_EDITOR_OSX
                            string buttonName = "Reveal in Finder";
#else
                                string buttonName = "Show in Explorer";
#endif
                            if (GUILayout.Button(buttonName))
                            {
                                EditorUtility.RevealInFinder(outputDir);
                            }
                        }
                    }

                    EditorGUILayout.HelpBox("You can use '{Platform}' variable for Output Directory path to include platform name.", MessageType.Info);
                }

                GUILayout.Space(8f);

                var fileName = _fileName[editor.CurrentEditingGroup];
                var newFileName = EditorGUILayout.TextField("File Name", fileName);
                if (newFileName != fileName)
                {
                    using (new RecordUndoScope("Change File Name", node, true))
                    {
                        _fileName[editor.CurrentEditingGroup] = newFileName;
                        onValueChanged();
                    }
                }
            }
        }
    }

    private string PrepareOutputDirectory(BuildTarget target, Model.NodeData node, bool autoCreate, bool throwException)
    {
        var outputOption = (OutputOption) _outputOption[target];

        if (outputOption == OutputOption.BuildInCacheDirectory)
        {
            return FileUtility.EnsureWorkingCacheDirExists(target, node, "BundlesConfiguration");
        }

        var outputDir = _outputDir[target];

        outputDir = outputDir.Replace("{Platform}", BuildTargetUtility.TargetToAssetBundlePlatformName(target));

        if (throwException)
        {
            if (string.IsNullOrEmpty(outputDir))
            {
                throw new NodeException("Output directory is empty.",
                    "Select valid output directory from inspector.", node);
            }

            if (target != BuildTargetUtility.GroupToTarget(BuildTargetGroup.Unknown) &&
                outputOption == OutputOption.ErrorIfNoOutputDirectoryFound)
            {
                if (!Directory.Exists(outputDir))
                {
                    throw new NodeException("Output directory not found.",
                        "Create output directory or select other valid directory from inspector.", node);
                }
            }
        }

        if (autoCreate && !Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        return outputDir;
    }

    /**
     * Prepare is called whenever graph needs update. 
     */
    public override void Prepare(BuildTarget target,
        Model.NodeData node,
        IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput,
        PerformGraph.Output Output)
    {
        //_outputDir[target] = PrepareOutputDirectory(target, node, false, true);
    }

    /**
     * Build is called when Unity builds assets with AssetBundle Graph. 
     */
    public override void Build(BuildTarget target,
        Model.NodeData node,
        IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput,
        PerformGraph.Output outputFunc,
        Action<Model.NodeData, string, float> progressFunc)
    {

        List<Dictionary<string, List<AssetReference>>> groups = new List<Dictionary<string, List<AssetReference>>>();
        foreach (var assetGroups in incoming)
        {
            groups.Add(assetGroups.assetGroups);
        }
    
        var jsonSerializer =new JsonSerializer( new JsonSerializerSettings{ Formatting = Formatting.Indented }, Encoding.UTF8 );

        var path = PrepareOutputDirectory(target, node, false, true);
        File.WriteAllText( path + _fileName[target] + ".json", jsonSerializer.SerializeString( groups ) );
    }
}
#endif
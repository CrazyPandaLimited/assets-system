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

[CustomNode("Cleanup/Clear directory", 1000)]
public class CleanupFolderNode : Node
{
    public enum OutputOption : int
    {
        BuildInCacheDirectory,
        BuildInBundlesCacheDirectory,
        ManualFolder,
        
    }

    [SerializeField] private SerializableMultiTargetInt _enabledOptions;
    [SerializeField] private SerializableMultiTargetString _outputDir;
    [SerializeField] private SerializableMultiTargetInt _outputOption;

    public override string ActiveStyle
    {
        get { return "node 7 on"; }
    }

    public override string InactiveStyle
    {
        get { return "node 7"; }
    }

    public override string Category
    {
        get { return "Cleanup"; }
    }

    public override Model.NodeOutputSemantics NodeInputType
    {
        get { return Model.NodeOutputSemantics.Any; }
    }

    public override Model.NodeOutputSemantics NodeOutputType
    {
        get { return Model.NodeOutputSemantics.Any; }
    }

    public override void Initialize(Model.NodeData data)
    {
        _enabledOptions = new SerializableMultiTargetInt();
        _outputDir = new SerializableMultiTargetString();
        _outputOption = new SerializableMultiTargetInt();
        data.AddDefaultInputPoint();
        data.AddDefaultOutputPoint();
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new CleanupFolderNode();
        newNode._enabledOptions = new SerializableMultiTargetInt(_enabledOptions);
        newNode._outputDir = new SerializableMultiTargetString(_outputDir);
        newNode._outputOption = new SerializableMultiTargetInt(_outputOption);
        newData.AddDefaultOutputPoint();
        newData.AddDefaultInputPoint();
        return newNode;
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
    {
        if (_enabledOptions == null)
        {
            return;
        }

        EditorGUILayout.HelpBox("Удаляет папку и все файлы в ней", MessageType.Info);
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
                    }
                    else
                    {
                        _enabledOptions.Remove(editor.CurrentEditingGroup);
                        _outputDir.Remove(editor.CurrentEditingGroup);
                        _outputOption.Remove(editor.CurrentEditingGroup);
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

                using (new EditorGUI.DisabledScope(opt == OutputOption.BuildInCacheDirectory || opt == OutputOption.BuildInBundlesCacheDirectory))
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

                    var outputDir = PrepareOutputDirectory(BuildTargetUtility.GroupToTarget(editor.CurrentEditingGroup), node.Data);

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

            }
        }
    }

    private string PrepareOutputDirectory(BuildTarget target, Model.NodeData node)
    {
        var outputOption = (OutputOption) _outputOption[target];

        if (outputOption == OutputOption.BuildInCacheDirectory)
        {
            return FileUtility.EnsureWorkingCacheDirExists(target, node, "");
        }
        if (outputOption == OutputOption.BuildInBundlesCacheDirectory)
        {
            return FileUtility.EnsureAssetBundleCacheDirExists(target, node);
        }

        
        var outputDir = _outputDir[target];

        outputDir = outputDir.Replace("{Platform}", BuildTargetUtility.TargetToAssetBundlePlatformName(target));
        
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
        var directoryPath = PrepareOutputDirectory(target, node);

        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
        }
    }
}
#endif
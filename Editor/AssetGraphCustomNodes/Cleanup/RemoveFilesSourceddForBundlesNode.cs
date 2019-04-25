#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;

[CustomNode("Cleanup/Remove Files, sourced for assetBundles", 1000)]
public class RemoveFilesSourceddForBundlesNode : Node
{
    public enum WorkOption : int
    {
        none,
        RemoveLocatedIntoUnityResourcesFolder,
        RemoveByAssetPathContains,
        RemoveByAssetPathStarts,
        RemoveByAssetNameContains,
        RemoveByAssetNameStarts,
    }

    [SerializeField] private SerializableMultiTargetInt _enabledPerPlatformOptions;
    [SerializeField] private SerializableMultiTargetInt _workingOption;
    [SerializeField] private SerializableMultiTargetString _filterString;

    #region Properties

    public override string Category
    {
        get { return "Cleanup"; }
    }

    public override string ActiveStyle
    {
        get { return "node 7 on"; }
    }

    public override string InactiveStyle
    {
        get { return "node 7"; }
    }

    public override Model.NodeOutputSemantics NodeInputType
    {
        get { return Model.NodeOutputSemantics.AssetBundles; }
    }

    public override Model.NodeOutputSemantics NodeOutputType
    {
        get { return Model.NodeOutputSemantics.AssetBundles; }
    }

    #endregion

    #region Public Members

    public override void Initialize(Model.NodeData data)
    {
        _enabledPerPlatformOptions = new SerializableMultiTargetInt();
        _workingOption = new SerializableMultiTargetInt();
        _filterString = new SerializableMultiTargetString();
        data.AddDefaultOutputPoint();
        data.AddDefaultInputPoint();
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new RemoveFilesSourceddForBundlesNode();
        newNode._enabledPerPlatformOptions = new SerializableMultiTargetInt(_enabledPerPlatformOptions);
        newNode._workingOption = new SerializableMultiTargetInt(_workingOption);
        newNode._filterString = new SerializableMultiTargetString(_filterString);
        newData.AddDefaultOutputPoint();
        newData.AddDefaultInputPoint();
        return newNode;
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
    {
        EditorGUILayout.HelpBox("Will delete all files, specified by bundles manifest and passed node condition." + Environment.NewLine +
                                " \"OnlyInBatchMode\" means node work only if unity run in batch mode", MessageType.Info);

        editor.UpdateNodeName(node);

        GUILayout.Space(10f);

        //Show target configuration tab
        editor.DrawPlatformSelector(node);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            var disabledScope = editor.DrawOverrideTargetToggle(node, _enabledPerPlatformOptions.ContainsValueOf(editor.CurrentEditingGroup), (bool enabled) =>
            {
                using (new RecordUndoScope("Remove Target Bundle Options", node, true))
                {
                    if (enabled)
                    {
                        _enabledPerPlatformOptions[editor.CurrentEditingGroup] = _enabledPerPlatformOptions.DefaultValue;
                        _workingOption[editor.CurrentEditingGroup] = _workingOption.DefaultValue;
                        _filterString[editor.CurrentEditingGroup] = _filterString.DefaultValue;
                    }
                    else
                    {
                        _enabledPerPlatformOptions.Remove(editor.CurrentEditingGroup);
                        _workingOption.Remove(editor.CurrentEditingGroup);
                        _filterString.Remove(editor.CurrentEditingGroup);
                    }

                    onValueChanged();
                }
            });

            using (disabledScope)
            {
                WorkOption opt = (WorkOption) _workingOption[editor.CurrentEditingGroup];
                var newOption = (WorkOption) EditorGUILayout.EnumPopup("Mode:", opt);
                if (newOption != opt)
                {
                    using (new RecordUndoScope("Change node mode", node, true))
                    {
                        _workingOption[editor.CurrentEditingGroup] = (int) newOption;
                        onValueChanged();
                    }
                }

                if (opt == WorkOption.RemoveByAssetPathContains ||
                    opt == WorkOption.RemoveByAssetPathStarts ||
                    opt == WorkOption.RemoveByAssetNameContains ||
                    opt == WorkOption.RemoveByAssetNameStarts)
                {
                    GUILayout.Space(8f);

                    var curRegexValue = _filterString[editor.CurrentEditingGroup];
                    EditorGUILayout.LabelField("Conditions, separated by new line:");

                    var newRegexValue = EditorGUILayout.TextArea(curRegexValue);

                    if (newRegexValue != curRegexValue)
                    {
                        using (new RecordUndoScope("Change Regex", node, true))
                        {
                            _filterString[editor.CurrentEditingGroup] = newRegexValue;
                            onValueChanged();
                        }
                    }
                }
            }
        }
    }

    public override void Prepare(BuildTarget target, Model.NodeData nodeData, IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput, PerformGraph.Output outputFunc)
    {
        if (outputFunc != null)
        {
            var dst = connectionsToOutput == null || !connectionsToOutput.Any() ? null : connectionsToOutput.First();

            if (incoming != null)
            {
                foreach (var ag in incoming)
                {
                    outputFunc(dst, ag.assetGroups);
                }
            }
            else
            {
                outputFunc(dst, new Dictionary<string, List<AssetReference>>());
            }
        }
    }

    private List<string> GetAssetPathesFromManifest(string manifestPath)
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
                    var path = line.Replace("\"", "/");
                    path = path.Replace("- Assets/", "");

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

    public override void Build(BuildTarget target, Model.NodeData nodeData, IEnumerable<PerformGraph.AssetGroups> incoming,
        IEnumerable<Model.ConnectionData> connectionsToOutput, PerformGraph.Output outputFunc, Action<Model.NodeData, string, float> progressFunc)
    {
        if (_workingOption[target] == (int) WorkOption.none)
        {
            return;
        }

        var allResourcesPathesFromMatifest = new List<string>();
        var toDelPaths = new List<string>();
        foreach (var assetGroupse in incoming)
        {
            foreach (var assetGroup in assetGroupse.assetGroups)
            {
                foreach (var assetReference in assetGroup.Value)
                {
                    if (assetReference.extension == ".manifest")
                    {
                        allResourcesPathesFromMatifest.AddRange(GetAssetPathesFromManifest(assetReference.path));
                    }
                }
            }
        }

        if (_workingOption[target] == (int) WorkOption.RemoveLocatedIntoUnityResourcesFolder)
        {
            foreach (var path in allResourcesPathesFromMatifest)
            {
                if (_workingOption[target] == (int) WorkOption.RemoveLocatedIntoUnityResourcesFolder && path.Contains("/Resources/"))
                {
                    if (toDelPaths.Contains(path))
                    {
                        continue;
                    }

                    toDelPaths.Add(path);
                }
            }
        }
        else
        {
            var contitions = _filterString[target].Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in allResourcesPathesFromMatifest)
            {
                foreach (var condition in contitions)
                {
                    if ((_workingOption[target] == (int) WorkOption.RemoveByAssetNameStarts && Path.GetFileName(path).StartsWith(condition)) ||
                        (_workingOption[target] == (int) WorkOption.RemoveByAssetNameContains && Path.GetFileName(path).Contains(condition)) ||
                        (_workingOption[target] == (int) WorkOption.RemoveByAssetPathStarts && path.Replace(Path.GetFileName(path), "").StartsWith(condition)) ||
                        (_workingOption[target] == (int) WorkOption.RemoveByAssetPathContains && path.Replace(Path.GetFileName(path), "").Contains(condition)))
                    {
                        if (toDelPaths.Contains(path))
                        {
                            continue;
                        }

                        toDelPaths.Add(path);
                    }
                }
            }
        }

        Debug.Log("RemoveUnityResourceNode. Selected to remove count: " + toDelPaths.Count);
        foreach (var assetPath in toDelPaths)
        {
            Debug.Log("Remove: " + Application.dataPath + "/" + assetPath);
            File.Delete(Application.dataPath + "/" + assetPath);
        }

        Debug.Log("RemoveUnityResourceNode. All selections removed: ");
    }

    #endregion
}
#endif
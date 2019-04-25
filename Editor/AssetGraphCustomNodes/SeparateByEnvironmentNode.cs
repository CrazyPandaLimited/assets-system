#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_ASSET_GRAPH
using UnityEditor;
using System;
using System.Collections.Generic;
using UnityEngine.AssetGraph;
using Model = UnityEngine.AssetGraph.DataModel.Version2;

[CustomNode("Conditions/Separate by environment", 1000)]
public class SeparateByEnvironmentNode : Node
{
    public override string ActiveStyle
    {
        get { return "node 0 on"; }
    }

    public override string InactiveStyle
    {
        get { return "node 0"; }
    }

    public override string Category
    {
        get { return "Conditions"; }
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
        data.AddDefaultInputPoint();
        data.AddOutputPoint("Any");
        data.AddOutputPoint("OnlyInEditor");
        data.AddOutputPoint("OnlyInBatchMode");
    }

    public override Node Clone(Model.NodeData newData)
    {
        var newNode = new SeparateByEnvironmentNode();
        newData.AddDefaultInputPoint();
        newData.AddOutputPoint("Any");
        newData.AddOutputPoint("OnlyInEditor");
        newData.AddOutputPoint("OnlyInBatchMode");
        return newNode;
    }

    public override void OnInspectorGUI(NodeGUI node, AssetReferenceStreamManager streamManager, NodeGUIEditor editor, Action onValueChanged)
    {
        EditorGUILayout.HelpBox("Separate by environment", MessageType.Info);
        editor.UpdateNodeName(node);
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
        // Pass incoming assets straight to Output
        if (Output == null || connectionsToOutput == null)
        {
            return;
        }

        foreach (var connectionData in connectionsToOutput)
        {
            if (connectionData.Label == "Any" ||
                connectionData.Label == "OnlyInEditor" && !UnityEditorInternal.InternalEditorUtility.inBatchMode ||
                connectionData.Label == "OnlyInBatchMode" && UnityEditorInternal.InternalEditorUtility.inBatchMode)
            {
                if (incoming != null)
                {
                    foreach (var ag in incoming)
                    {
                        Output(connectionData, ag.assetGroups);
                    }
                }
                else
                {
                    // Overwrite output with empty Dictionary when no there is incoming asset
                    Output(connectionData, new Dictionary<string, List<AssetReference>>());
                }
            }
        }
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
        // Do nothing
    }
}
#endif
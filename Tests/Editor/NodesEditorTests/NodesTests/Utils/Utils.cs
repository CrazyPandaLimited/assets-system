using System;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;
using NSubstitute;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    public static class Utils
    {
        public static NodeModel CreateNode< TNodeType >() where TNodeType : BaseNodeType, new()
        {
            return new TNodeType().CreateNode();
        }

        internal static IGraphExecutionResult Execute( NodeModel node, Action< GraphExecutionContext, AssetsStorageModel > onPreExecuteNodeEvent = default )
        {
            var assetStorageModel = new AssetsStorageModel();
            var nodesExecutor = new GraphExecutionContext { Node = node };
            onPreExecuteNodeEvent?.Invoke( nodesExecutor,  assetStorageModel);
            (node.Type as IExecutionNode)?.Execute( nodesExecutor, assetStorageModel );
            return nodesExecutor;
        }

        internal static void SetPortValue< T >( this GraphExecutionContext ctx, string portId, T value )
        {
            var node = ctx.Node;
            var inPort = node.Port( portId );
            var fakeConnection = new ConnectionModel( Substitute.For< IConnectionType >() );
            fakeConnection.To = inPort;
            inPort.Connections.Add( fakeConnection );
            ctx.Connection = fakeConnection;
            (ctx as IConnectionExecutionContext).SetOutput( value );
        }
    }
}
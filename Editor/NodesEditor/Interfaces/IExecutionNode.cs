using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    interface IExecutionNode
    {
        void Execute( INodeExecutionContext executionContext, AssetsStorageModel assetsStorageModel );
        
        void OnPropertyChanged( NodeModel nodeModel, string changedPropertyName );

        void OnNodeCreated( object someData, NodeModel nodeModel );
    }
}
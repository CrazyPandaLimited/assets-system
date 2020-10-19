using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class BaseExecutionNode <TProperty> : BaseNodeType, IExecutionNode where TProperty : PropertyBlock, new()
    {
        public void Execute( INodeExecutionContext executionContext, AssetsStorageModel assetsStorageModel )
        {
            var property = executionContext.Node.PropertyBlock as TProperty;

            Execute( executionContext, assetsStorageModel, property );
            Execute( executionContext, property);
        }

        public void OnPropertyChanged( NodeModel nodeModel, string changedPropertyName )
        {
            OnPropertyChanged( nodeModel, ( TProperty )nodeModel.PropertyBlock, changedPropertyName );
        }

        public void OnNodeCreated( object someData, NodeModel nodeModel )
        {
            OnNodeCreated( someData, ( TProperty )nodeModel.PropertyBlock );
        }

        protected override PropertyBlock CreatePropertyBlock( NodeModel node )
        {
            return new TProperty();
        }

        protected virtual void Execute( INodeExecutionContext executionContext, AssetsStorageModel assetsStorageModel, TProperty property )
        {
            
        }

        protected virtual void Execute( INodeExecutionContext executionContext, TProperty property )
        {
            
        }

        protected virtual void OnPropertyChanged( NodeModel nodeModel, TProperty propertyBlock, string changedPropertyName )
        {
        }

        protected virtual void OnNodeCreated( object someData, TProperty propertyBlock )
        {
            
        }
    }
}
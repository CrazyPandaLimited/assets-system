using CrazyPanda.UnityCore.NodeEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    class BuilderGeneratorNodeView : BaseNodeView
    {
        private readonly Label _exceptionMessageLabel;

        public BuilderGeneratorNodeView( string graphName, NodeModel node, IEdgeConnectorListener edgeConnectorListener ) : base( node, edgeConnectorListener )
        {
            styleSheets.Add( Resources.Load< StyleSheet >( $"Styles/{nameof(BuilderGeneratorNodeView)}" ) );
            _exceptionMessageLabel = new Label { name = "exception-message", text = string.Empty };
            extensionContainer.Add( _exceptionMessageLabel );
            RefreshExpandedState();
            OnNodeCreated( graphName );
        }

        public bool HasException => !string.IsNullOrEmpty( _exceptionMessageLabel.text );
        
        public void SetExecutionResult( IGraphExecutionResult result )
        {
            var exception = result.GetExceptionByNodeId( Node.Id );
            var hasException = exception != null;

            _exceptionMessageLabel.text = hasException ? $"{exception.GetType().Name}: {exception.Message}. \n StackTrace: {exception.StackTrace}" : string.Empty;
            EnableInClassList( "has-exception", hasException );
        }

        protected override void NodePropertyBlockChanged( ObjectPropertiesField changedField, string fieldName )
        {
            (this.Node.Type as IExecutionNode)?.OnPropertyChanged( this.Node, fieldName );
        }

        private void OnNodeCreated( string graphName )
        {
            (this.Node.Type as IExecutionNode)?.OnNodeCreated( graphName, this.Node );
        }
    }
}
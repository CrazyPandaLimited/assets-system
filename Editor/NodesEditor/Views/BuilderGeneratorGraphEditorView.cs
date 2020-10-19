using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class BuilderGeneratorGraphEditorView : BaseGraphEditorView
    {
        private readonly Func<string> _graphNameGetter;

        public BuilderGeneratorGraphEditorView( Func<string> graphNameGetter )
        {
            _graphNameGetter = graphNameGetter;
            CreateToolbarButton( "Generate dll", GenerateDll );
        }

        public override BaseNodeView CreateNodeView( NodeModel node, IEdgeConnectorListener edgeConnectorListener )
        {
            if( node.Type is ProcessorNode )
            {
                return new ProcessorNodeView( _graphNameGetter.Invoke(), node, edgeConnectorListener );
            }

            return new BuilderGeneratorNodeView( _graphNameGetter.Invoke(), node, edgeConnectorListener );
        }

        private void GenerateDll()
        {
            var ( executionResult, dllGenerationResult) = (Graph.Type as BuilderGeneratorGraphType).Execute( Graph, new ProgressReporter() );

            PrintExceptions( executionResult );
            PrintDllGenerationResult( dllGenerationResult );
            
            SetExecutionResult( executionResult );
        }

        private void PrintDllGenerationResult( DllGenerationResult result )
        {
            Debug.Log( $"Generated Content = {Environment.NewLine}{result.GeneratedContent}" );
            
            Debug.Log( $"Does \"{result.CodeGenerationModel.PathToFinalDll}\" dll created success = {result.Success}" );

            foreach( var d in result.Diagnostics )
            {
                var formatted = CSharpDiagnosticFormatter.Instance.Format( d );

                switch( d.Severity )
                {
                    case DiagnosticSeverity.Error:
                        Debug.LogError( formatted );
                        break;
                    case DiagnosticSeverity.Warning:
                        Debug.LogWarning( formatted );
                        break;
                    default:
                        Debug.Log( formatted );
                        break;
                }
            }
        }

        private void PrintExceptions(IGraphExecutionResult result)
        {
            foreach( var exception in result.Exceptions )
            {
                if( !(exception is NodeExecutionException) )
                {
                    Debug.LogException( exception );
                }
            }
        }
        
        private void SetExecutionResult( IGraphExecutionResult result )
        {
            GraphView.graphElements.ForEach( view => (view as BuilderGeneratorNodeView)?.SetExecutionResult( result ) );
            FocusNode( GetViewElements< BuilderGeneratorNodeView >().FirstOrDefault( nodeView => nodeView.HasException ) );
        }
        
        private void FocusNode( GraphElement nodeView )
        {
            if( nodeView == null )
            {
                return;
            }
            
            var nodeRect = nodeView.ChangeCoordinatesTo( GraphView.contentViewContainer, new Rect( 0, 0, nodeView.layout.width, nodeView.layout.height ) );

            UnityEditor.Experimental.GraphView.GraphView.CalculateFrameTransform( nodeRect, GraphView.layout, 30, out var tanslate, out var scale );
            GraphView.UpdateViewTransform( tanslate, scale );
        }

        private IEnumerable< T > GetViewElements< T >() where T : VisualElement
        {
            return GraphView.graphElements.ToList().OfType< T >();
        }

        private void CreateToolbarButton( string name, Action clickEvent )
        {
            Toolbar.Add( new ToolbarButton( clickEvent )
            {
                text = name
            } );
        }
    }
}
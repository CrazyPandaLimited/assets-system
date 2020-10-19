using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class BuilderGeneratorGraphType : IGraphType, IGraphTypeResolver
    {
        private static readonly IConnectionType _defaultConnectionType = new BaseConnectionType();
        
        private readonly NodesRegistry _nodesRegistry = new NodesRegistry();

        public string Name { get; } = "Builder generator graph";

        public IReadOnlyList< INodeType > AvailableNodes => _nodesRegistry.AvailableNodes;

        public IConnectionType FindConnectionType( Type @from, Type to )
        {
            if( to.IsGenericType && from.IsGenericType && to.GetGenericTypeDefinition() == from.GetGenericTypeDefinition())
            {
                var toGenericArguments = to.GetGenericArguments();
                var fromGenericArguments = from.GetGenericArguments();

                bool isGenericTypeCorrect = true;
                
                for( int i = 0; i < to.GetGenericArguments().Length; i++ )
                {
                    if( !( isGenericTypeCorrect = toGenericArguments[ i ].IsAssignableFrom( fromGenericArguments[ i ] )) )
                    {
                        break;
                    }
                }

                return isGenericTypeCorrect ? _defaultConnectionType : null;
            }
            else if (to.IsAssignableFrom( from ))
            {
                return _defaultConnectionType;
            }

            return null;
        }
        
        public void PostLoad( GraphModel graph )
        {
        }

        public (IGraphExecutionResult executionResult, DllGenerationResult dllGenerationResult) Execute( GraphModel graph, IProgressReporter progressReporter )
        {
            var model = new AssetsStorageModel();
            
            var executionResult = graph.Execute( ctx =>
            {
                try
                {
                    (ctx.Node.Type as IExecutionNode)?.Execute( ctx, model );
                }
                catch( Exception e ) when( !(e is NodeExecutionException) )
                {
                    throw new NodeExecutionException( e.Message, ctx.Node.Id, e );
                }
            } );

            if( executionResult.Exceptions.Count == 0 )
            {
               var dllGenerationResult = AssetsStorageGenerator.GenerateDllWithCode( model, progressReporter );
               return (executionResult, dllGenerationResult);
            }

            return (executionResult, DllGenerationResult.Empty);
        }
        
        public T GetInstance< T >( string typeName ) where T : class
        {
             return _nodesRegistry.GetNodeByName( typeName ) as T ?? _nodesRegistry.GetNodeByTypeName( typeName ) as T;
        }

        public string GetTypeName< T >( T instance ) where T : class
        {
            return instance is ProcessorNode node ? node.Name : instance.GetType().FullName;
        }
    }
}
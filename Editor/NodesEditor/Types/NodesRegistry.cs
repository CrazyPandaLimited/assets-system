using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class NodesRegistry  : INodeTypeRegistry
    {
        private static readonly Type _defaultNodeType = typeof( IFlowNode );
        private static readonly Type _extraNodeType = typeof( IExtraNodeType );
        
        private readonly Lazy< IReadOnlyList< INodeType > > _allNodes;
        
        public IReadOnlyList< INodeType > AvailableNodes => _allNodes.Value;

        public NodesRegistry()
        {
            _allNodes = new Lazy< IReadOnlyList< INodeType > >( GetNodes );
        }

        public INodeType GetNodeByName( string nodeName )
        {
            return AvailableNodes.FirstOrDefault( node => node.Name == nodeName );
        }
        
        public INodeType GetNodeByTypeName( string typeName )
        {
            return AvailableNodes.FirstOrDefault( node => node.GetType().FullName == typeName );
        }
        
        private IReadOnlyList< INodeType > GetNodes()
        {
            var ret = new List< INodeType >();

            foreach( var asm in AppDomain.CurrentDomain.GetAssemblies() )
            {
                foreach( var type in asm.GetTypes() )
                {
                    if( ShouldProcessType( type ) )
                    {
                        ret.Add( _defaultNodeType.IsAssignableFrom( type ) ? CreateNode( type ) : CreateExtraNode( type ) );
                    }
                }
            }

            return ret;
        }
        
        private bool ShouldProcessType( Type type )
        {
            return !type.IsAbstract && (_defaultNodeType.IsAssignableFrom( type ) || _extraNodeType.IsAssignableFrom( type ));
        }

        private INodeType CreateNode( Type type )
        {
            return new ProcessorNode( type );
        }

        private INodeType CreateExtraNode( Type type )
        {
            return ( INodeType ) Activator.CreateInstance( type );
        }
    }
}
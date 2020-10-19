using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CrazyPanda.UnityCore.NodeEditor;
using UnityEditor.Experimental.GraphView;
using static UnityEditor.ObjectNames;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class ProcessorNodeView : BuilderGeneratorNodeView
    {
        public ProcessorNodeView( string graphName, NodeModel node, IEdgeConnectorListener edgeConnectorListener ) : base( graphName, node, edgeConnectorListener )
        {
        }

        protected override BasePortView CreatePortView( PortModel port )
        {
            return new ProcessorNodePortView( port, Orientation.Horizontal, EdgeConnectorListener );
        }
        
        private sealed class ProcessorNodePortView : BasePortView
        {
            public ProcessorNodePortView( PortModel port, Orientation portOrientation, IEdgeConnectorListener edgeConnectorListener ) : base( port, portOrientation, edgeConnectorListener )
            {
                portName = GetPortName();
            }

            private string GetPortName()
            {
                ProcessorNode nodeType = ( ProcessorNode ) Port.Node.Type;

                if( ContainsActivePortId( nodeType.GenericPorts ) )
                {
                    return NicifyVariableName( $"{RemovePortPrefixFromPortId( "gp_" )} Generic Argument" );
                }
                else if (ContainsActivePortId( nodeType.CtorParameterPorts ))
                {
                    return NicifyVariableName( $"{RemovePortPrefixFromPortId( "cp_" )} Ctor Parameter" );
                }

                return NicifyVariableName( Port.Id );
            }

            private bool ContainsActivePortId( IEnumerable<PortInfo> portInfos)
            {
                return portInfos.Any( portInfo => portInfo.ID == Port.Id );
            }

            private string RemovePortPrefixFromPortId(string prefixToRemove)
            {
                return Regex.Replace( Port.Id, $@"^{prefixToRemove}", string.Empty );
            }
        }
    }
}
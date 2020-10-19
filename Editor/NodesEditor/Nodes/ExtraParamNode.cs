using System;
using System.Linq;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class ExtraParamNode : BaseExecutionNode <ExtraParamNode.Property>, IExtraNodeType
    {
        private const string PortId = "Out";
        
        public override string Name => nameof(ExtraParamNode);
        
        protected override void Execute( INodeExecutionContext executionContext, AssetsStorageModel assetsStorageModel, Property property )
        {
            var type = property.Type.Type;
            
            if( type == null )
            {
                throw new ArgumentNullException( $"{property.Type.ToString()}\" type not found !" );
            }

            var ctorParameter = CreateCtorParameter( type, property );
            assetsStorageModel.AddCtorParameters( ctorParameter );

            executionContext.SetOutput( PortId, ctorParameter );
        }

        protected override void CreatePorts( NodeModel node )
        {
            var propertyBlock = (Property) node.PropertyBlock;
            var type = propertyBlock.Type.Type;

            CreateOutputPort( type, node );
        }

        protected override PropertyBlock CreatePropertyBlock( NodeModel node )
        {
            var property = new Property();
            property.SetFormattedName( nameof(ExtraParamNode) );
            return property;
        }

        protected override void OnPropertyChanged( NodeModel nodeModel, Property property, string changedFieldName )
        {
            if( changedFieldName != nameof(Property.Type) )
            {
                return;
            }
            
            var port = nodeModel.Port( PortId );
            var oldType = port?.Type?.GetGenericArguments().FirstOrDefault();
            var newType = property.Type.Type;

            if( oldType == newType )
            {
                return;
            }

            DisconnectPort( port,nodeModel );
            CreateOutputPort( newType, nodeModel );
        }

        private void DisconnectPort( PortModel port, NodeModel nodeModel )
        {
            if( port == null )
            {
                return;
            }

            foreach( var connectionModel in port.Connections.ToArray() )
            {
                nodeModel.Graph.Disconnect( connectionModel );
            }

            nodeModel.RemovePort( port );
        }

        private void CreateOutputPort(Type type, NodeModel node)
        {
            if( type == null )
            {
                return;
            }

            node.AddPort( new PortModel( PortId, typeof( CtorParameter<> ).MakeGenericType( type ), PortDirection.Output, PortCapacity.Multiple ) );
        }
        
        private object GetCastedValue( Type type, string value )
        {
            try
            {
                return Convert.ChangeType( value, type );
            }
            catch
            {
                return null;
            }
        }

        private CtorParameter CreateCtorParameter( Type type, Property propertyBlock )
        {
            var ctorType = typeof( CtorParameter<> ).MakeGenericType( type );
            return ( CtorParameter )Activator.CreateInstance( ctorType, propertyBlock.MemberName, propertyBlock.MakeDefaultValue, GetCastedValue( type, propertyBlock.DefaultValue ) );
        }
        
        public sealed class Property : NameProperty
        {
            public TypeProvider Type;
            public string DefaultValue = string.Empty;
            public bool MakeDefaultValue;
        }
    }
}
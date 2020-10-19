using System;
using CrazyPanda.UnityCore.NodeEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class ExtraParamTypeNode : BaseExecutionNode< ExtraParamTypeNode.Property > , IExtraNodeType
    {
        public override string Name => nameof(ExtraParamTypeNode);
        private OutputPort< Type > Out { get; }

        protected override void Execute( INodeExecutionContext executionContext, Property property )
        {
            var type = property.Type.Type;
            
            if( type == null )
            {
                throw new ArgumentNullException( $"{property.Type.ToString()}\" type not found !" );
            }

            Out.Set( executionContext, type );
        }

        public sealed class Property : PropertyBlock
        {
            public TypeProvider Type;
        }
    }
}
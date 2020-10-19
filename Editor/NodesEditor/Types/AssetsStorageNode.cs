using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.NodeEditor;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class AssetsStorageNode : BaseExecutionNode <AssetsStorageNode.Property> , IExtraNodeType
    {
        private const string DefaultNodeName = "AssetsStorage";
        private const string DefaultDllVersion = "1.0.0";
        private const string DefaultNameSpaceFormat = "CrazyPanda.UnityCore.{0}";
        
        private static readonly MethodInfo _processorsConnectionMethod;
        private static readonly Type _processorsInputNodeType;
        private static readonly Type _assetsStorageType  = typeof( BaseAssetsStorage );
        
        static AssetsStorageNode()
        {
            _processorsConnectionMethod = _assetsStorageType.GetMethodInfoByMethodInputParamType( typeof( IInputNode<> ) );
            _processorsInputNodeType = _processorsConnectionMethod.GetParameters()[ 0 ].ParameterType;
        }

        public override string Name { get; } = DefaultNodeName;

        protected override void Execute( INodeExecutionContext executionContext, AssetsStorageModel assetsStorageModel, Property property )
        {
            CheckThatCreateParamsAreCorrect( property );

            assetsStorageModel.ProcessorsLinkerName = _processorsConnectionMethod.Name;
            assetsStorageModel.ExpectedLinkType = _processorsInputNodeType;
            assetsStorageModel.TypeName = property.TypeName;
            assetsStorageModel.NameSpace = property.NameSpace;
            assetsStorageModel.PathToFinalDll = property.PathToFinalDll.Path;
            assetsStorageModel.DllName = Path.GetFileName( property.PathToFinalDll.Path );
            assetsStorageModel.DllVersion = property.DllVersion;
            assetsStorageModel.BaseType = _assetsStorageType;
            
            executionContext.SetOutput( _processorsConnectionMethod.Name, assetsStorageModel );
        }

        protected override PropertyBlock CreatePropertyBlock( NodeModel node )
        {
            return new Property
            {
                TypeName = DefaultNodeName,
                DllVersion = DefaultDllVersion
            };
        }

        protected override void CreatePorts( NodeModel node )
        {
            node.AddPort( new PortModel( _processorsConnectionMethod.Name, typeof( ProcessorLinkInformation ), PortDirection.Output, PortCapacity.Single ) );
        }

        protected override void OnNodeCreated( object someData, Property propertyBlock )
        {
            if( string.IsNullOrEmpty( propertyBlock.NameSpace ) )
            {
                propertyBlock.NameSpace = $"CrazyPanda.UnityCore.{someData}";
            }
        }
        
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private void CheckThatCreateParamsAreCorrect( Property property )
        {
            if( string.IsNullOrEmpty( property.NameSpace ) )
            {
                throw new ArgumentNullException( nameof(property.NameSpace) );
            }

            if( string.IsNullOrEmpty( property.TypeName ) )
            {
                throw new ArgumentNullException(nameof(property.TypeName));
            }
            
            if( !Version.TryParse( property.DllVersion, out _ ) )
            {
                throw new ArgumentNullException( (nameof(property.DllVersion)) );
            }

            if( string.IsNullOrEmpty( property.PathToFinalDll.Path ) )
            {
                throw new ArgumentNullException( nameof(property.PathToFinalDll) );
            }

            var dllFolder = Path.GetDirectoryName( property.PathToFinalDll.Path );

            if( !Directory.Exists( dllFolder ) )
            {
                throw new NotSupportedException( $"It is impossible to generate dll on path \"{property.PathToFinalDll.Path}\", because \"{dllFolder}\" does not exist!" );
            }
        }
        
        public sealed class Property : PropertyBlock
        {
            public string NameSpace = string.Empty;
            public string TypeName = string.Empty;
            public string DllVersion = string.Empty;

            [JsonProperty]
            public PathToFile PathToFinalDll = new PathToFile( "*.dll", string.Empty );
        }
    }
}
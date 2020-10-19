using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.NodeEditor;
using Microsoft.CodeAnalysis.CSharp;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class ProcessorNode : BaseExecutionNode <NameProperty>
    {
        private static readonly Type _defaultInputNodeType = typeof( IInputNode <> );
        private static readonly Type _defaultOutputNodeType = typeof( IOutputNode<> );

        private readonly PortsCollection _genericPorts = new PortsCollection();
        private readonly PortsCollection _mainInputPorts = new PortsCollection();
        private readonly PortsCollection _outputPorts = new PortsCollection();
        private readonly PortsCollection _ctorParameterPorts = new PortsCollection();
        
        private readonly Type _processorType;
        
        public ProcessorNode( Type processorType )
        {
            _processorType = processorType ?? throw new ArgumentNullException( nameof(processorType) );
            GeneratePorts();
        }
        
        public override string Name
        {
            get
            {
                if( _processorType.IsOpenGenericType())
                {
                    return _processorType.GetOpenGenericName();
                }
                else if (_processorType.IsGenericType)
                {
                    return _processorType.GetGenericName();
                }
                else
                {
                    return _processorType.GetShortName();
                }
            }
        }

        public IPortsCollection GenericPorts => _genericPorts;
        public IPortsCollection MainInputPorts => _mainInputPorts;
        public IPortsCollection OutputPorts => _outputPorts;
        public IPortsCollection CtorParameterPorts => _ctorParameterPorts;
        
        protected override void Execute( INodeExecutionContext ctx, AssetsStorageModel assetsStorageModel, NameProperty property )
        {
            var processorModel = CreateProcessorModel( property.MemberName, ctx, assetsStorageModel );

            assetsStorageModel.AddProcessors( processorModel );

            foreach( var info in _outputPorts )
            {
                ctx.SetOutput( info.ID, processorModel.ProcessorsToLink[ info.ID ] );
            }

            foreach( var info in _mainInputPorts )
            {
                var linkInformations = GetMultipleInput< ProcessorLinkInformation >( info.ID, ctx );

                foreach( var processorLinkInformation in linkInformations )
                {
                    var linkProperty = processorModel.ProcessorLinkers[ info.ID ];

                    var finalLinkType = linkProperty.PropertyType;
                    if( !processorLinkInformation.ExpectedLinkType.IsAssignableFrom( finalLinkType ) )
                    {
                        throw new InvalidOperationException( $"It is impossible to generate final builder, because input node with \"{finalLinkType.FullName}\" type can not be cast " +
                                                             $"as \"{processorLinkInformation.ExpectedLinkType.FullName}\" type" );
                    }

                    processorLinkInformation.ProcessorLinkProperty = linkProperty;
                    processorLinkInformation.ProcessorToLink = processorModel;
                }
            }
        }

        protected override void CreatePorts( NodeModel node )
        {
            CreatePorts( node, _outputPorts, PortDirection.Output, PortCapacity.Single );
            CreatePorts( node, _mainInputPorts, PortDirection.Input, PortCapacity.Multiple);
            CreatePorts( node, _genericPorts.Concat( _ctorParameterPorts ), PortDirection.Input, PortCapacity.Single );
        }

        protected override PropertyBlock CreatePropertyBlock( NodeModel node )
        {
            var property = new NameProperty();
            property.SetFormattedName( _processorType.GetShortName(), SyntaxKind.PrivateKeyword );
            return property;
        }

        private void CreatePorts( NodeModel node, IEnumerable< PortInfo > portInfos, PortDirection portDirection, PortCapacity portCapacity )
        {
            foreach( var portInfo in portInfos )
            {
                node.AddPort( new PortModel( portInfo.ID, portInfo.Type, portDirection, portCapacity ) );
            }
        }

        private void GeneratePorts()
        {
            Type mainPortsType = typeof( ProcessorLinkInformation );
            Type closeGenericPortsType = typeof( Type );
            Type defaultCtorParameterPortsType = typeof( CtorParameter );

            foreach( var nodeInfo in GetOutputNodes( _processorType ).OrderBy( nodeInfo => nodeInfo.Name ) )
            {
                _outputPorts.Add( new PortInfo( nodeInfo.Name, mainPortsType ) );
            }
            
            foreach( var nodeInfo  in GetInputNodes( _processorType ) )
            {
                _mainInputPorts.Add( new PortInfo( nodeInfo.Name, mainPortsType ) );
            }
            
            if( _processorType.IsGenericTypeDefinition )
            {
                var generics = _processorType.GetGenericArguments();

                foreach( var generic in generics )
                {
                    _genericPorts.Add( new PortInfo( $"gp_{generic.GetShortName()}", closeGenericPortsType ) );
                }
            }
            
            foreach( var parameter in _processorType.GetConstructors().First().GetParameters() )
            {
                var skipPortCreation = parameter.ParameterType == typeof(RequestToPromiseMap);
                
                if( !skipPortCreation )
                {
                    var portType = parameter.ParameterType.IsOpenGenericType() ? defaultCtorParameterPortsType : typeof( CtorParameter<> ).MakeGenericType( parameter.ParameterType );
                    _ctorParameterPorts.Add( new PortInfo( $"cp_{parameter.Name}", portType ) );
                }
            }
        }
        
        private ProcessorModel CreateProcessorModel(string name, INodeExecutionContext ctx, AssetsStorageModel assetsStorageModel)
        {
            var finalProcessorType = GetFinalProcessorType( ctx );
            var processorModel = new ProcessorModel( name, finalProcessorType, GetInputNodes( finalProcessorType ), GetOutputNodes( finalProcessorType ), GetCtorDeps( finalProcessorType, ctx, assetsStorageModel ) );
            
            processorModel.Keywords.Add( SyntaxKind.PrivateKeyword );
            
            return processorModel;
        }

        private Type GetFinalProcessorType( INodeExecutionContext ctx )
        {
            if( !_processorType.IsGenericTypeDefinition )
            {
                return _processorType;
            }

            return _processorType.MakeGenericType( _genericPorts.Select( pair => GetInput<Type>( pair.ID, ctx ) ).ToArray() );
        }

        private IEnumerable< Parameter > GetCtorDeps( Type finalProcessorType, INodeExecutionContext ctx, AssetsStorageModel assetsStorageModel )
        {
            var ctorParameters = finalProcessorType.GetConstructors().First().GetParameters();

            for( var i = 0; i < ctorParameters.Length; i++ )
            {
                var ctorParameterInfo = ctorParameters[ i ];
                var ctorParameterPortInfo = _ctorParameterPorts.FirstOrDefault(port => port.ID.Contains( ctorParameterInfo.Name ));
                
                if( !ctorParameterPortInfo.Exists )
                {
                    var parameterToReturn = assetsStorageModel.BaseType.GetFields( BindingFlags.NonPublic | BindingFlags.Instance )
                                                              .First(field => field.FieldType == ctorParameterInfo.ParameterType);

                    yield return new Parameter( parameterToReturn.FieldType, parameterToReturn.Name );
                    continue;
                }

                var ctorDepType = GetInput< CtorParameter >( ctorParameterPortInfo.ID, ctx );

                if( !ctorParameterInfo.ParameterType.IsAssignableFrom( ctorDepType.Type ) )
                {
                    throw new InvalidOperationException( $"It is impossible to use \"{ctorDepType.Type.GetGenericName()}\" as constructor dependency for \"{finalProcessorType.GetGenericName()}\" type, " + 
                                                         $"because \"{ctorDepType.Type.GetGenericName()}\" type can not be cast as \"{ctorParameters[i].ParameterType.GetGenericName()}\" type" );
                }
                
                yield return new Parameter( ctorDepType.Type, ctorDepType.Name );
            }
        }

        private IEnumerable<PropertyInfo> GetInputNodes( Type processorType )
        {
            return processorType.GetProperties().Where( info => info.CanRead && info.PropertyType.HasInterface( _defaultInputNodeType ) );
        }
        
        private IEnumerable< PropertyInfo > GetOutputNodes(Type processorType)
        {
            return processorType.GetProperties().Where( info => info.CanRead && info.PropertyType.HasInterface( _defaultOutputNodeType ) );
        }

        private T GetInput< T >( string portId, INodeExecutionContext ctx )
        {
            return  ctx.GetInput<T>( ctx.Node.Port( portId ).Connections.Single() );
        }

        private IEnumerable< T > GetMultipleInput< T >( string portId, INodeExecutionContext ctx )
        {
            return ctx.Node.Port( portId ).Connections.Select( ctx.GetInput< T > );
        }
        
    }
}
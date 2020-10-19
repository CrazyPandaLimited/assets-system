using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    [Serializable]
    public sealed class ProcessorModel : Parameter
    {
        private readonly Dictionary< string, PropertyInfo > _processorLinkers;
        private readonly Dictionary< string, ProcessorLinkInformation > _processorsToLink;
        
        public ProcessorModel( string name, Type type,
                         IEnumerable< PropertyInfo > inputNodes,
                         IEnumerable< PropertyInfo > outputNodes, IEnumerable< Parameter > ctorParameters ) : base( type, name )
        {
            _processorLinkers = new Dictionary< string, PropertyInfo >();
            _processorsToLink = new Dictionary< string, ProcessorLinkInformation >();
            CtorParameters = new HashSet< Parameter >( ctorParameters );
            
            foreach( var inputConnection in inputNodes )
            {
                _processorLinkers[ inputConnection.Name ] = inputConnection;
            }
            
            foreach( var outputNode in outputNodes )
            {
                var expectedInputType = outputNode.PropertyType.GetMethodInfoByMethodInputParamType( typeof( IInputNode<> ) ).GetParameters()[0].ParameterType;

                _processorsToLink[ outputNode.Name ] = new ProcessorLinkInformation
                {
                    ProcessorsLinkerName = outputNode.Name,
                    ExpectedLinkType = expectedInputType
                };
            }
        }

        public IEnumerable< Parameter > CtorParameters { get; }
        public IReadOnlyDictionary< string, PropertyInfo > ProcessorLinkers => _processorLinkers;
        public IReadOnlyDictionary< string, ProcessorLinkInformation > ProcessorsToLink => _processorsToLink;
    }
}
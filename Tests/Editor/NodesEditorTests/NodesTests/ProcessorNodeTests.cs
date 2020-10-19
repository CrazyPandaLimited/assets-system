using System;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.MessagesFlow;
using CrazyPanda.UnityCore.NodeEditor;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using static CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests.Utils;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class ProcessorNodeTests
    {
        private static readonly Type _testProcessorType = typeof( TestProcessor< , > );

        [ Test ]
        public void AllPorts_Should_Succeed_Generate()
        {
            var nodeType = new ProcessorNode( _testProcessorType );

            Assert.IsNotEmpty( nodeType.GenericPorts );
            Assert.IsNotEmpty( nodeType.CtorParameterPorts );
            Assert.IsNotEmpty( nodeType.OutputPorts );
            Assert.IsNotEmpty( nodeType.MainInputPorts );

            foreach( var argument in _testProcessorType.GetGenericArguments() )
            {
                CheckThatCollectionHasValue( nodeType.GenericPorts, $"gp_{argument.Name}", typeof( Type ) );
            }

            foreach( var parameterInfo in _testProcessorType.GetConstructors().Single().GetParameters() )
            {
                if( parameterInfo.ParameterType == typeof( RequestToPromiseMap ) )
                {
                    continue;
                }
                
                CheckThatCollectionHasValue( nodeType.CtorParameterPorts, $"cp_{parameterInfo.Name}", typeof(CtorParameter<> ).MakeGenericType(  parameterInfo.ParameterType ) );
            }

            foreach( var propertyInfo in _testProcessorType.GetProperties().Where( info => info.PropertyType.HasInterface( typeof( IInputNode<> ) ) ) )
            {
                CheckThatCollectionHasValue( nodeType.MainInputPorts, propertyInfo.Name, typeof( ProcessorLinkInformation ) );
            }

            foreach( var propertyInfo in _testProcessorType.GetProperties().Where( info => info.PropertyType.HasInterface( typeof( IOutputNode<> ) ) ) )
            {
                CheckThatCollectionHasValue( nodeType.OutputPorts, propertyInfo.Name, typeof( ProcessorLinkInformation ) );
            }

            void CheckThatCollectionHasValue( IPortsCollection collection, string name, Type type )
            {
                
                Assert.That( collection, Has.Some.Matches< PortInfo >( info => info.ID == name && info.Type == type ) );
            }
        }

        [ Test ]
        public void NodeName_Should_Generated_Success()
        {
            var nodeType = new ProcessorNode( _testProcessorType );
            Assert.That( nodeType.Name, Is.EqualTo( "TestProcessor<,>" ) );
        }

        [ Test ]
        public void Execute_Should_Succeed()
        {
            var baseAssetStorageType = typeof( BaseAssetsStorage );
            var requestToPromiseMapType = baseAssetStorageType.GetField( "_requestToPromiseMap", BindingFlags.NonPublic | BindingFlags.Instance );
            var requestToPromiseMapParameter = new Parameter( requestToPromiseMapType.FieldType, requestToPromiseMapType.Name );

            Type testGenericType = typeof( TestMessageBody );
            var stringCtorParameter = new CtorParameter< string >( "testValue", makeDefaultValue: false );

            var testOutputNode = new TestProcessorLinkInformation
            {
                ProcessorsLinkerName = "OutputNode",
                ExpectedLinkType = typeof( IInputNode< TestMessageBody > )
            };

            var nodeType = new ProcessorNode( _testProcessorType );
            var node = nodeType.CreateNode();

            AssetsStorageModel generatedAssetsStorageModel = default;

            var executionResult = Execute( node, ( context, assetsStorageModel ) =>
            {
                generatedAssetsStorageModel = assetsStorageModel;
                assetsStorageModel.BaseType = baseAssetStorageType;
                assetsStorageModel.AddCtorParameters( stringCtorParameter );

                foreach( var portInfo in nodeType.GenericPorts )
                {
                    context.SetPortValue( portInfo.ID, testGenericType );
                }

                context.SetPortValue( nodeType.CtorParameterPorts.First().ID, stringCtorParameter );

                foreach( var portInfo in nodeType.MainInputPorts )
                {
                    context.SetPortValue( portInfo.ID, testOutputNode );
                }
            } );

            var generatedProcessorModel = generatedAssetsStorageModel.Processors.First();

            var expectedProcessorType = _testProcessorType.MakeGenericType( Enumerable.Repeat( testGenericType, 2 ).ToArray() );

            Assert.IsNotNull( generatedProcessorModel );
            Assert.That( generatedProcessorModel.Type, Is.EqualTo( expectedProcessorType ) );
            Assert.That( generatedProcessorModel.Name, Is.EqualTo( "_testProcessor" ) );
            Assert.That( generatedProcessorModel.Keywords, Has.Some.Matches< SyntaxKind >( keyWord => keyWord == SyntaxKind.PrivateKeyword ) );

            Assert.That( generatedProcessorModel.CtorParameters, Has.Some.EqualTo( stringCtorParameter ) );
            Assert.That( generatedProcessorModel.CtorParameters, Has.Some.EqualTo( requestToPromiseMapParameter ) );

            var inputNodeProperty = expectedProcessorType.GetProperties().First( property => property.Name == "InputNode" );
            Assert.That( generatedProcessorModel.ProcessorLinkers, new DictionaryContainsKeyConstraint( inputNodeProperty.Name ) );
            Assert.That( generatedProcessorModel.ProcessorLinkers, new DictionaryContainsValueConstraint( inputNodeProperty ) );

            var outputNodeProperty = expectedProcessorType.GetProperties().First( property => property.Name == "OutputNode" );
            Assert.That( generatedProcessorModel.ProcessorsToLink, new DictionaryContainsKeyConstraint( outputNodeProperty.Name ) );

            Assert.That( testOutputNode.IsCorrect, Is.True );
            Assert.That( testOutputNode.ProcessorToLink, Is.EqualTo( generatedProcessorModel ) );
            Assert.That( testOutputNode.ProcessorLinkProperty, Is.EqualTo( inputNodeProperty ) );

            foreach( var portInfo in nodeType.OutputPorts )
            {
                executionResult.TryGetPortValue( node.Port( portInfo.ID ), out ProcessorLinkInformation outputValue );

                Assert.IsNotNull( outputValue );
                Assert.That( outputValue.ExpectedLinkType, Is.EqualTo( typeof( IInputNode< TestMessageBody > ) ) );
                Assert.IsFalse( outputValue.IsCorrect );
                Assert.IsNull( outputValue.ProcessorLinkProperty );
                Assert.IsNull( outputValue.ProcessorToLink );
            }
        }

        [ Test ]
        public void Execute_Should_Throw_InvalidOperationException_When_Main_InputPorts_Incorrectly_Connected()
        {
            Type testGenericType = typeof( TestMessageBody );
            var stringCtorParameter = new CtorParameter< string >( "testValue", makeDefaultValue: false );
            var nodeType = new ProcessorNode( _testProcessorType );
            var node = nodeType.CreateNode();

            Assert.Throws< InvalidOperationException >( () => Execute( node, ( context, assetsStorageModel ) =>
            {
                assetsStorageModel.AddCtorParameters( stringCtorParameter );

                foreach( var portInfo in nodeType.GenericPorts )
                {
                    context.SetPortValue( portInfo.ID, testGenericType );
                }

                context.SetPortValue( nodeType.CtorParameterPorts.First().ID, stringCtorParameter );

                foreach( var portInfo in nodeType.MainInputPorts )
                {
                    context.SetPortValue( portInfo.ID, typeof( int ) );
                }
            } ) );
        }

        [ Test ]
        public void Execute_Should_Throw_InvalidOperationException_When_Ctor_Ports_Incorrectly_Connected()
        {
            var testOutputNode = new TestProcessorLinkInformation { ExpectedLinkType = typeof( IInputNode< TestMessageBody > ) };

            Type testGenericType = typeof( TestMessageBody );
            var wrongCtorParameter = new CtorParameter <int>( "testValue", makeDefaultValue: false );
            var nodeType = new ProcessorNode( _testProcessorType );
            var node = nodeType.CreateNode();

            Assert.Throws< InvalidOperationException >( () => Execute( node, ( context, assetsStorageModel ) =>
            {
                assetsStorageModel.AddCtorParameters( wrongCtorParameter );

                foreach( var portInfo in nodeType.GenericPorts )
                {
                    context.SetPortValue( portInfo.ID, testGenericType );
                }

                context.SetPortValue( nodeType.CtorParameterPorts.First().ID, wrongCtorParameter );

                foreach( var portInfo in nodeType.MainInputPorts )
                {
                    context.SetPortValue( portInfo.ID, testOutputNode );
                }
            } ) );
        }

        private sealed class TestProcessor< TIn, TOut > where TOut : IMessageBody
        {
            public IInputNode< TIn > InputNode { get; }

            public IOutputNode< TOut > OutputNode { get; }

            public TestProcessor( string value, RequestToPromiseMap requestToPromiseMap)
            {
            }
        }

        private readonly struct TestMessageBody : IMessageBody
        {
        }

        private sealed class TestProcessorLinkInformation : ProcessorLinkInformation
        {
        }
    }
}
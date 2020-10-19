using System.IO;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.NodeEditor;
using NUnit.Framework;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class GraphExecutionTests : BaseNodeEditorTests
    {
        private static readonly string _pathToGraphContentFolder = "Assets/UnityCoreSystems/Systems/Tests/ResourcesSystem/Editor/NodesEditorTests/Graphs";

        [ Test ]
        public void Execute_Should_Succeed_Generate_Dll()
        {
            var graphType = new BuilderGeneratorGraphType();
            AssetsStorageGeneratorTests.CheckThatDllCreatedCorrectly( graphType.Execute( GetCorrectGraph(), new NullableProgressReporter() ).dllGenerationResult );
        }
        
        [ Test ]
        public void Execute_Should_Succeed_Generate_AssetsStorageModel()
        {
            var testGraph = GetCorrectGraph();
            var ( executedResult, dllGenerationResult ) = new BuilderGeneratorGraphType().Execute( testGraph, new NullableProgressReporter() );
            var generatedAssetsStorageModel = dllGenerationResult.CodeGenerationModel;
            
            Assert.IsNotNull( executedResult );
            Assert.IsNotNull( dllGenerationResult );

            var assetsStorageNode = testGraph.Nodes.First( node => node.Type is AssetsStorageNode );
            var propertyBlock = (AssetsStorageNode.Property) assetsStorageNode.PropertyBlock;
            
            Assert.That( propertyBlock.DllVersion, Is.EqualTo( generatedAssetsStorageModel.DllVersion ) );
            Assert.That( propertyBlock.NameSpace, Is.EqualTo( generatedAssetsStorageModel.NameSpace ) );
            Assert.That( propertyBlock.TypeName, Is.EqualTo( generatedAssetsStorageModel.TypeName ) );
            Assert.That( propertyBlock.PathToFinalDll.Path, Is.EqualTo( generatedAssetsStorageModel.PathToFinalDll ) );
            Assert.That( Path.GetFileName( propertyBlock.PathToFinalDll.Path ), Is.EqualTo( generatedAssetsStorageModel.DllName ) );

            Assert.That( typeof( BaseAssetsStorage ), Is.EqualTo( generatedAssetsStorageModel.BaseType ) );

            Assert.IsTrue( generatedAssetsStorageModel.IsCorrect );
            Assert.That( generatedAssetsStorageModel.ProcessorToLink, Is.Not.Null );
            Assert.That( generatedAssetsStorageModel.ProcessorLinkProperty, Is.Not.Null );
            Assert.That( generatedAssetsStorageModel.ProcessorsLinkerName, Is.Not.Null );
            
            Assert.That( generatedAssetsStorageModel.BaseType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic), 
                         Has.Some.Matches<MethodInfo>( info =>  info.Name.Equals( generatedAssetsStorageModel.ProcessorsLinkerName ) ) );
            
            Assert.That( generatedAssetsStorageModel.ProcessorToLink.ProcessorLinkers.Values, Has.Some.Matches<PropertyInfo>( info => info.Equals( generatedAssetsStorageModel.ProcessorLinkProperty ) ) );
            Assert.That( generatedAssetsStorageModel.ExpectedLinkType, Is.EqualTo( generatedAssetsStorageModel.ProcessorLinkProperty.PropertyType ));

            var processors = generatedAssetsStorageModel.Processors;
            var processorNodes = testGraph.Nodes.Where( node => node.Type is ProcessorNode ).ToArray();
            
            Assert.IsNotNull( processors );
            
            foreach( var processorModel in processors )
            {
                var allProcessorModelProperties = processorModel.Type.GetProperties();
  
                Assert.That( processorNodes, Has.Some.Matches <NodeModel> (node => (node.PropertyBlock as NameProperty).MemberName == processorModel.Name ));
                Assert.That( processorModel.ProcessorLinkers.Values, Has.All.Matches<PropertyInfo>( info => allProcessorModelProperties.Any(property => property.Equals( info )) ) );
                
                foreach( var outputNode in processorModel.ProcessorsToLink.Values )
                {
                    Assert.That( allProcessorModelProperties, Has.Some.Matches< PropertyInfo >( info => info.Name.Equals( outputNode.ProcessorsLinkerName ) ) );
                    Assert.That( outputNode.ExpectedLinkType, Is.EqualTo( outputNode.ProcessorLinkProperty.PropertyType ) );
                    Assert.IsTrue( outputNode.IsCorrect );
                    Assert.That( outputNode.ProcessorToLink.ProcessorLinkers.Values, Has.Some.Matches<PropertyInfo>( info => info.Equals( outputNode.ProcessorLinkProperty ) ) );
                }
            }
        }
        
        [ Test ]
        public void Execute_Should_Fail_To_Generate_Dll_Due_ToNodeExceptions()
        {
            var graph = GetGraph( "WrongGraph.resbuildergengraph" );
            var graphType = graph.Type as BuilderGeneratorGraphType;
            var ( executedResult, dllGenerationResult) = graphType.Execute( graph, new NullableProgressReporter() );
            
            Assert.IsFalse( dllGenerationResult.Success );
            Assert.IsEmpty( dllGenerationResult.GeneratedContent );
            Assert.IsEmpty( dllGenerationResult.Diagnostics );
            
            Assert.IsNotEmpty( executedResult.Exceptions );
            Assert.That( executedResult.Exceptions, Has.All.AssignableFrom( typeof( NodeExecutionException ) ) );
            FileAssert.DoesNotExist( _pathToGeneratedDll );
        }
        
        private GraphModel GetCorrectGraph()
        {
            return GetGraph(  "CorrectGraph.resbuildergengraph" );
        }

        private GraphModel GetGraph(string graphName)
        {
            return GraphSerializer.Deserialize( File.ReadAllText( Path.Combine( _pathToGraphContentFolder, graphName ) ) );
        }
    }
}
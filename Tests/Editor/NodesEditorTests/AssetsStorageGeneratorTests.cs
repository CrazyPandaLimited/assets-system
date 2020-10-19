using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.MessagesFlow;
using Microsoft.CodeAnalysis;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class AssetsStorageGeneratorTests : BaseNodeEditorTests
    {
        [ Test ]
        public void GenerateDllWithCode_Should_Succeed()
        {
            var model = GenerateTestModel();
            CheckThatDllCreatedCorrectly( GenerateDllWithCode( model ) );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Fail()
        {
            var model = GenerateTestModel();
            model.AddProcessors(  new ProcessorModel( "TestProcessor", typeof(PrivateProcessor), Enumerable.Empty<PropertyInfo>(),
                                                      Enumerable.Empty< PropertyInfo >(),Enumerable.Empty<CtorParameter>() ));
            
            GenerateDllWithCode( model );
            FileAssert.DoesNotExist( model.PathToFinalDll );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Throw_ArgumentNullException_WhenGenerationModel_IsNull()
        {
            Assert.Throws< ArgumentNullException >( () => GenerateDllWithCode( null ) );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Throw_ArgumentNullException_WhenDllVersion_DoesNotCorrect()
        {
            Assert.Throws< ArgumentNullException >( () =>
            {
                var model = GenerateTestModel();
                model.DllVersion = "some_value";
                GenerateDllWithCode( model );
            } );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Throw_ArgumentNullException_WhenPathToFinalDll_IsNull()
        {
            Assert.Throws< ArgumentNullException >( () =>
            {
                var model = GenerateTestModel();
                model.PathToFinalDll = null;
                GenerateDllWithCode( model );
            } );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Throw_NotSupportedException_WhenRootFolder_Of_FinalDll_DoesNotExist()
        {
            Assert.Throws< NotSupportedException >( () =>
            {
                var model = GenerateTestModel();
                model.PathToFinalDll = Path.Combine( Application.temporaryCachePath, "Some_Wrong_Dll_Folder", "SomeDll.dll" );
                GenerateDllWithCode( model );
            } );
        }

        [ Test ]
        public void GenerateDllWithCode_Should_Succeed_GenerateDll_With_Correct_AssemblyReferences()
        {
            var model = GenerateTestModel();
            GenerateDllWithCode( model );

            using( var loader = new AssemblyLoaderContext( model ) )
            {
                loader.LoadFromFile( model.PathToFinalDll, ( generatedAssembly, assetsStorageModel ) =>
                {
                    var referencedAssemblies = generatedAssembly.GetReferencedAssemblies();
                    
                    CheckThatReferencedAssembliesContainsAssemblies( assetsStorageModel.BaseType.GetReferenceAssemblies() );
                    CheckThatReferencedAssembliesContainsAssemblies( assetsStorageModel.Processors.SelectMany( processor => processor.Type.GetReferenceAssemblies() ) );

                    CheckThatReferencedAssembliesDoesNotContainAssembly( "Assembly-CSharp" );
                    CheckThatReferencedAssembliesDoesNotContainAssembly( "Assembly-CSharp-Editor" );

                    void CheckThatReferencedAssembliesContainsAssemblies( IEnumerable< Assembly > assemblies )
                    {
                        foreach( var assembly in assemblies )
                        {
                            CheckThatReferencedAssembliesContainsAssembly( assembly );
                        }
                    }

                    void CheckThatReferencedAssembliesContainsAssembly( Assembly assembly )
                    {
                        Assert.That( referencedAssemblies, Has.Some.Matches< AssemblyName >( referencedAssembly => referencedAssembly.Name.Equals( assembly.GetName().Name ) ) );
                    }

                    void CheckThatReferencedAssembliesDoesNotContainAssembly( string assemblyName )
                    {
                        Assert.That( referencedAssemblies, Has.None.Matches< AssemblyName >( referencedAssembly => referencedAssembly.Name.Equals( assemblyName ) ) );
                    }
                } );
            }
        }
        
        public static void CheckThatDllCreatedCorrectly( DllGenerationResult dllGenerationResult )
        {
            Assert.IsTrue( dllGenerationResult.Success );
            Assert.IsNotEmpty( dllGenerationResult.GeneratedContent );
            Assert.That( dllGenerationResult.Diagnostics, Has.None.Matches<Diagnostic>( diagnostic => diagnostic.Severity == DiagnosticSeverity.Error ) );
            
            FileAssert.Exists( dllGenerationResult.CodeGenerationModel.PathToFinalDll );

            using( var loader = new AssemblyLoaderContext( dllGenerationResult.CodeGenerationModel ) )
            {
                loader.LoadFromFile( dllGenerationResult.CodeGenerationModel.PathToFinalDll, ( generatedAssembly, model ) =>
                {
                    var assemblyName = generatedAssembly.GetName();

                    Assert.That( Version.Parse( model.DllVersion ), Is.EqualTo( assemblyName.Version ) );
                    Assert.That( model.DllName, Is.EqualTo( assemblyName.Name ) );
                    Assert.That( Path.GetFullPath( model.PathToFinalDll ), Is.EqualTo( generatedAssembly.Location ) );

                    Assert.IsNotEmpty( generatedAssembly.GetTypes() );
                    Assert.That( generatedAssembly.GetTypes().Length, Is.EqualTo( 1 ) );

                    var generatedType = generatedAssembly.GetTypes()[ 0 ];

                    Assert.IsNotNull( generatedType );
                    Assert.IsTrue( generatedType.IsSealed );
                    Assert.IsTrue( generatedType.IsPublic );
                    Assert.IsTrue( generatedType.IsClass );
                    Assert.IsFalse( generatedType.IsAbstract );

                    Assert.That( model.TypeName, Is.EqualTo( generatedType.Name ) );
                    Assert.That( model.BaseType, Is.EqualTo( generatedType.BaseType ) );
                    Assert.That( model.NameSpace, Is.EqualTo( generatedType.Namespace ) );

                    var generatedCtors = generatedType.GetConstructors();
                    Assert.That( generatedCtors.Length, Is.EqualTo( 1 ) );

                    var generatedCtor = generatedCtors[ 0 ];

                    var generatedCtorParameters = generatedCtor.GetParameters().ToDictionary( ctorParameter => ctorParameter.Name, ctorParameter => ctorParameter );

                    CheckCtorParameters( model.CtorParameters );

                    var generatedMethods = generatedType.GetMethods( BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly );
                    Assert.That( generatedMethods.Length, Is.GreaterThanOrEqualTo( 1 ) );
                    Assert.That( generatedMethods, Has.Some.Matches< MethodInfo >( info => info.Name.Equals( "ConnectAllProcessors" ) && info.GetParameters().Length == 0 && info.IsPrivate && info.ReturnType == typeof( void ) ) );

                    var generatedProcessors = generatedType.GetProperties( BindingFlags.Instance | BindingFlags.NonPublic ).ToDictionary( processor => processor.Name, processor => processor );

                    var processorsToCheck = model.Processors;
                    Assert.That( generatedProcessors.Count, Is.EqualTo( processorsToCheck.Count() ) );

                    foreach( var processor in processorsToCheck )
                    {
                        CheckCtorParameters( processor.CtorParameters.OfType<CtorParameter>() );
                        Assert.That( generatedProcessors, new DictionaryContainsKeyConstraint( processor.Name ) );

                        var generatedProcessor = generatedProcessors[ processor.Name ];

                        Assert.That( generatedProcessor.Name, Is.EqualTo( processor.Name ) );
                        Assert.That( generatedProcessor.PropertyType, Is.EqualTo( processor.Type ) );
                    }

                    void CheckCtorParameters( IEnumerable< CtorParameter > ctorParameters )
                    {
                        foreach( var expectedCtorParameter in ctorParameters )
                        {
                            Assert.That( generatedCtorParameters, new DictionaryContainsKeyConstraint( expectedCtorParameter.Name ) );

                            var generatedCtorParameter = generatedCtorParameters[ expectedCtorParameter.Name ];

                            Assert.That( generatedCtorParameter.Name, Is.EqualTo( expectedCtorParameter.Name ) );
                            Assert.That( generatedCtorParameter.ParameterType, Is.EqualTo( expectedCtorParameter.Type ) );
                            Assert.That( generatedCtorParameter.HasDefaultValue, Is.EqualTo( expectedCtorParameter.MakeValue ) );

                            if( expectedCtorParameter.MakeValue && expectedCtorParameter.Value != default )
                            {
                                Assert.That( generatedCtorParameter.DefaultValue, Is.EqualTo( expectedCtorParameter.Value ) );
                            }
                        }
                    }

                } );
            }
        }

        private DllGenerationResult GenerateDllWithCode( AssetsStorageModel model )
        {
            return AssetsStorageGenerator.GenerateDllWithCode( model, new NullableProgressReporter() );
        }

        private AssetsStorageModel GenerateTestModel()
        {
            var baseModelType = typeof( TestAssetsStorage );
            var nodesConnector = baseModelType.GetMethod( nameof(TestAssetsStorage.LinkTo) );
            var expectedInputType = nodesConnector.GetParameters()[ 0 ].ParameterType;
            var processorModel = GenerateProcessorModel();
            var inputNode = processorModel.ProcessorLinkers.Values.First();
            
            var assetStorageModel = new AssetsStorageModel
            {
                TypeName = "TestType",
                BaseType = baseModelType,
                ProcessorsLinkerName = nodesConnector.Name,
                ExpectedLinkType = expectedInputType,
                ProcessorToLink = processorModel,
                ProcessorLinkProperty = inputNode,
                DllVersion = "1.0.0.1",
                PathToFinalDll = _pathToGeneratedDll,
                DllName = Path.GetFileName( _pathToGeneratedDll ),
                NameSpace = "TestNameSpace.Test"
            };
            
            assetStorageModel.AddProcessors( processorModel );
            assetStorageModel.AddCtorParameters( CreateCtorParameter< string >( "test_value" ), CreateCtorParameter< RequestToPromiseMap >( "map" ) );
            assetStorageModel.AddCtorParameters( processorModel.CtorParameters.Cast<CtorParameter>().ToArray() );

            return assetStorageModel;
        }
        
        private ProcessorModel GenerateProcessorModel()
        {
            var testProcessorType = typeof( TestProcessor );
            var inputProcessorNodes = new[] { testProcessorType.GetProperty( nameof(TestProcessor.TestInput) ) };

            return new ProcessorModel( "TestProcessor", testProcessorType, inputProcessorNodes, Enumerable.Empty< PropertyInfo >(),
                                       new[] { CreateCtorParameter( "value", makeOptional:true, "test_value" ) } );
        }
        

        private CtorParameter CreateCtorParameter< T >( string name, bool makeOptional = false, T value = default )
        {
            return new CtorParameter(typeof(T), name, makeOptional, value );
        }
        
        public abstract class TestAssetsStorage
        {
            
            public void LinkTo( IInputNode< int > node )
            {
            }

            public void Subscribe()
            {
                
            }
            
            protected void SubscribeToNodeStatusChanged( TestProcessor testProcessor )
            {
            }
        }

        public sealed class TestProcessor 
        {
            public IInputNode< int > TestInput { get; }

            public TestProcessor( string value )
            {
            }
        }
        
        private sealed class PrivateProcessor
        {
            public IInputNode< int > TestInput { get; }
        }
        
        private sealed class AssemblyLoaderContext : IDisposable
        {
            private readonly AppDomain _domain;
            private readonly AssetsStorageModel _assetsStorageModel;

            public AssemblyLoaderContext(AssetsStorageModel assetsStorageModel)
            {
                _assetsStorageModel = assetsStorageModel;

                _domain = AppDomain.CreateDomain( "temp_domain" );
                _domain.Load( typeof( Loader ).Assembly.GetName() );

                foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    try
                    {
                        _domain.Load( assembly.GetName() );
                    }
                    catch
                    {
                    }
                }
            }

            public void Dispose()
            {
                AppDomain.Unload( _domain );
            }

            public void LoadFromFile( string path, Action< Assembly, AssetsStorageModel > onAssemblyLoaded )
            {
                Loader loader = ( Loader )_domain.CreateInstanceAndUnwrap( typeof( Loader ).Assembly.FullName, typeof( Loader ).FullName );
                loader.EventToExecute = onAssemblyLoaded;
                loader.AssetsStorageModel = _assetsStorageModel;
                loader.LoadAssembly( path );
            }

            private sealed class Loader : MarshalByRefObject
            {
                public AssetsStorageModel AssetsStorageModel;
                public Action< Assembly, AssetsStorageModel > EventToExecute;
                public string Path = string.Empty;
                
                public void LoadAssembly(string path ) => EventToExecute.Invoke( Assembly.Load( AssemblyName.GetAssemblyName( path ) ), AssetsStorageModel );
            }
        }
    }
}
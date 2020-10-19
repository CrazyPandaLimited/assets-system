using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using CrazyPanda.UnityCore.MessagesFlow;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static CrazyPanda.UnityCore.AssetsSystem.CodeGen.SyntaxFactoryUtils;
using static CrazyPanda.UnityCore.AssetsSystem.CodeGen.EqualsValueClauseSyntaxMapper;
using Assembly = System.Reflection.Assembly;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    static class AssetsStorageGenerator
    {
        private const string ProcessorsConnectionMethodName = "ConnectAllProcessors";
        private const string SubscribeProcessorsToNodeStatusChangedMethodName = "SubscribeProcessorsToNodeStatusChanged";
        
        public static DllGenerationResult GenerateDllWithCode( AssetsStorageModel codeGeneratorModel, IProgressReporter progressReporter )
        {
            CheckModelForCorrectData( codeGeneratorModel );

            using( progressReporter )
            {
                progressReporter.Report( $"Class {codeGeneratorModel.TypeName} is generating now", 0.25f );

                var mainNamespace =  CreateNameSpace( codeGeneratorModel.NameSpace ).AddMembers( CreateClass( codeGeneratorModel ) );
                var codeGenerator = CompilationUnit().AddMembers( mainNamespace )
                                                     .AddAttributeLists( AttributeList( SingletonSeparatedList( CreateDllVersionAttribute( codeGeneratorModel.DllVersion ) ) ) )
                                                     .NormalizeWhitespace();
            
                progressReporter.Report( $"Generated code is compiling now", 0.5f );

                var codeCompiler = CSharpCompilation.Create( codeGeneratorModel.DllName )
                                                                     .AddReferences( CreateMetadataReferences( codeGeneratorModel ) )
                                                                     .WithOptions( GetCompilerOptions() )
                                                                     .AddSyntaxTrees( codeGenerator.SyntaxTree );

                progressReporter.Report( $"\"{codeGeneratorModel.DllName}\" dll is building now", 0.75f );
                
                var result = codeCompiler.Emit( codeGeneratorModel.PathToFinalDll );

                if( result.Success )
                {
                    progressReporter.Report( $"Refreshing assets now", 0.9f );
                    AssetDatabase.Refresh();
                }
                else
                {
                    File.Delete( codeGeneratorModel.PathToFinalDll );
                }

                return new DllGenerationResult
                {
                    CodeGenerationModel = codeGeneratorModel,
                    Diagnostics = result.Diagnostics,
                    Success = result.Success,
                    GeneratedContent = codeGenerator.ToFullString()
                };
            }
        }

        private static CSharpCompilationOptions GetCompilerOptions()
        {
            return new CSharpCompilationOptions( outputKind: OutputKind.DynamicallyLinkedLibrary, optimizationLevel: OptimizationLevel.Release, warningLevel: 1 );
        }
        
        [ MethodImpl( MethodImplOptions.AggressiveInlining ) ]
        private static void CheckModelForCorrectData( AssetsStorageModel codeGeneratorModel)
        {
            if( codeGeneratorModel == null )
            {
                throw new ArgumentNullException( nameof(codeGeneratorModel) );
            }

            if( !Version.TryParse( codeGeneratorModel.DllVersion, out _ ) )
            {
                throw new ArgumentNullException(( nameof(codeGeneratorModel.DllVersion) ));
            }

            if( string.IsNullOrEmpty( codeGeneratorModel.PathToFinalDll ) )
            {
                throw new ArgumentNullException( nameof(codeGeneratorModel.PathToFinalDll) );
            }

            var dllFolder = Path.GetDirectoryName( codeGeneratorModel.PathToFinalDll );

            if( !Directory.Exists( dllFolder ) )
            {
                throw new NotSupportedException( $"It is impossible to generate dll on path \"{codeGeneratorModel.PathToFinalDll}\", because \"{dllFolder}\" does not exist!" );
            }
        }

        private static IEnumerable< MetadataReference > CreateMetadataReferences( AssetsStorageModel codeGeneratorModel )
        {
            return GetPathsToAssemblyReferences( GetAssemblyReferences( codeGeneratorModel ) ).Distinct().Select( path => MetadataReference.CreateFromFile( path ) );
        }
        
        private static IEnumerable< Assembly > GetAssemblyReferences(AssetsStorageModel codeGeneratorModel)
        {
            var result = new HashSet< Assembly >();

            result.UnionWith( codeGeneratorModel.BaseType.GetReferenceAssemblies() );
            
            foreach( var ctorParameter in codeGeneratorModel.CtorParameters )
            {
                result.UnionWith( ctorParameter.Type.GetReferenceAssemblies() );
            }
            
            foreach( var processor in codeGeneratorModel.Processors )
            {
                result.UnionWith( processor.Type.GetReferenceAssemblies() );
            }

            result.Add( typeof( object ).Assembly );

            return result;
        }

        private static IEnumerable< string > GetPathsToAssemblyReferences( IEnumerable< Assembly > assemblies )
        {
            var unityAssemblyBuilder = new AssemblyBuilder( "Test.dll", "Test.cs" ) { flags = AssemblyBuilderFlags.DevelopmentBuild, referencesOptions = ReferencesOptions.UseEngineModules };
            var assembliesToReplace = unityAssemblyBuilder.defaultReferences.ToDictionary( Path.GetFileNameWithoutExtension, reference => reference );

            foreach( var assembly in assemblies )
            {
                foreach( var referencedAssembly in assembly.GetReferencedAssemblies() )
                {
                    if( assembliesToReplace.TryGetValue( referencedAssembly.Name, out var pathToAssembly ) )
                    {
                        yield return pathToAssembly;
                    }
                }
                
                if( assembliesToReplace.TryGetValue( assembly.GetName().Name, out var assemblyPathToReplace ) )
                {
                    yield return assemblyPathToReplace;
                }
                else
                {
                    yield return assembly.Location;
                }
            }

            if( assembliesToReplace.TryGetValue( "netstandard", out var result ) )
            {
                yield return result;
            }
        }

        private static ClassDeclarationSyntax CreateClass( AssetsStorageModel assetsStorageModel )
        {
            var localProperties =  CreateProperties( assetsStorageModel.Processors ).ToArray();

            var constructorParams = CreateConstructorParams( assetsStorageModel );

            var constructor = CreateConstructor( assetsStorageModel.TypeName, constructorParams)
                                    .WithBody( Block( CreateConstructorBody( assetsStorageModel ) ) );

            return CreateClassDeclarationSyntax( assetsStorageModel.TypeName )
                                   .AddBaseListTypes( CreateBaseType( assetsStorageModel.BaseType ) )
                                   .AddMembers( constructor )
                                   .AddMembers( localProperties )
                                   .AddMembers( CreateProcessorsConnectionMethod( assetsStorageModel ) )
                                   .AddMembers( CreateSubscribeProcessorsToNodeStatusChangedMethod(assetsStorageModel) )
                                   .NormalizeWhitespace(  );
        }

        private static IEnumerable< StatementSyntax > CreateConstructorBody( AssetsStorageModel assetsStorageModel )
        {
            //At first we need to generate if checking for null
            foreach( var ctorParameter in assetsStorageModel.CtorParameters )
            {
                if( ctorParameter.Type.IsValueType || ctorParameter.MakeValue )
                {
                    continue;
                }
                
                var ifCondition = BinaryExpression( SyntaxKind.EqualsExpression, ParseName( ctorParameter.Name ), LiteralExpression( SyntaxKind.NullLiteralExpression ) );
                var ifBody = ParseStatement( $"throw new System.ArgumentNullException(nameof({ctorParameter.Name}));" ).NormalizeWhitespace();
                yield return IfStatement( ifCondition, ifBody );
            }
            
            //Then we need to create all processors instances

            foreach( var processor in assetsStorageModel.Processors )
            {
                var processorField = ParseName( processor.Name );
                var constructorCalling = CallConstructor( processor.Type, ToArguments( processor.CtorParameters ) );
                yield return ExpressionStatement( AssignmentExpression( SyntaxKind.SimpleAssignmentExpression, processorField, constructorCalling ) );            
            }
            
            //Then we need to call processors connection method
            yield return CallMethod( ProcessorsConnectionMethodName );
            yield return CallMethod( SubscribeProcessorsToNodeStatusChangedMethodName );
        }

        private static MethodDeclarationSyntax CreateSubscribeProcessorsToNodeStatusChangedMethod( AssetsStorageModel assetsStorageModel )
        {
            return CreateMethod( SubscribeProcessorsToNodeStatusChangedMethodName, CreateSubscribeProcessorsToNodeStatusChangedMethodBody( assetsStorageModel ) );
        }
        
        private static MethodDeclarationSyntax CreateProcessorsConnectionMethod( AssetsStorageModel assetsStorageModel )
        {
            return CreateMethod( ProcessorsConnectionMethodName, CreateProcessorsConnectionMethodBody( assetsStorageModel ));
        }

        private static MethodDeclarationSyntax CreateMethod( string method, IEnumerable< StatementSyntax > body )
        {
            return CreateVoidMethod( method ).WithBody( Block( body ) );
        }

        private static IEnumerable< StatementSyntax > CreateSubscribeProcessorsToNodeStatusChangedMethodBody(AssetsStorageModel assetsStorageModel)
        {
            foreach( var processorModel in assetsStorageModel.Processors )
            {
                yield return CallMethod( "SubscribeToNodeStatusChanged", CreateArgument( processorModel.Name ) );
            }
        }
        
        private static IEnumerable< StatementSyntax > CreateProcessorsConnectionMethodBody( AssetsStorageModel assetsStorageModel )
        {
            if( assetsStorageModel.IsCorrect )
            {
                //At first we must to connect assetsstorage class with processor
                yield return CallMethod( assetsStorageModel.ProcessorsLinkerName, CreateMemberAccessArgument( assetsStorageModel.ProcessorToLink.Name, assetsStorageModel.ProcessorLinkProperty.Name ) );
            }

            //Then, we must to connect processors nodes
            foreach( var processor in assetsStorageModel.Processors )
            {
                var processorProperties = processor.Type.GetProperties();
                
                foreach( var processorLinkInformation in processor.ProcessorsToLink.Values )
                {
                    if( !processorLinkInformation.IsCorrect )
                    {
                        continue;
                    }
                    
                    var processorLinker = GetMemberAccess( processor.Name, processorLinkInformation.ProcessorsLinkerName ).ToString();
                    var processorLinkMethod = processorProperties.First( property => property.Name == processorLinkInformation.ProcessorsLinkerName ).PropertyType.GetMethodInfoByMethodInputParamType( typeof( IInputNode<> ) );    
                    var processorLinkMember = CreateMemberAccessArgument( processorLinkInformation.ProcessorToLink.Name, processorLinkInformation.ProcessorLinkProperty.Name );

                    yield return CallMemberMethod( processorLinker, processorLinkMethod.Name, processorLinkMember );
                }
            }
        }

        private static IEnumerable< ParameterSyntax > CreateConstructorParams( AssetsStorageModel assetsStorageModel )
        {
            var parameters = new List< ParameterSyntax >();

            foreach( var ctorParameter in assetsStorageModel.CtorParameters )
            {
                var parameter = CreateParameter( ctorParameter.Type, ctorParameter.Name );

                if( ctorParameter.MakeValue)
                {
                    parameters.Add( parameter.WithDefault( GetEqualsValueClauseSyntax( ctorParameter.Type, ctorParameter.Value ) ) );
                }
                else
                {
                    parameters.Insert( 0, parameter );
                }
            }

            return parameters;
        }

        private static IEnumerable< PropertyDeclarationSyntax > CreateProperties(IEnumerable<Parameter> parameters)
        {
            return parameters.Select( model => CreateProperty( model.Type, model.Name, model.Keywords ) );
        }
        
        private static IEnumerable< ArgumentSyntax > ToArguments( IEnumerable< Parameter > parameters )
        {
            return  parameters.Select( ctorParameter => CreateArgument( ctorParameter.Name ) );
        }
    }
}
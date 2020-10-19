using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using CrazyPanda.UnityCore.MessagesFlow;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using UnityEngine;
using static CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests.Utils;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class AssetsStorageNodeTests
    {
        [ Test ]
        public void Execute_Should_Succeed()
        {
            const string testDllVersion = "1.0.0.0";
            const string testNameSpace = "Some.Test.NameSpace";
            const string testTypeName = "TestType";
            string pathToFinalDll = Path.GetFullPath( $"{Application.temporaryCachePath}/Test.dll" );
            string testDllName = Path.GetFileName( pathToFinalDll );

            var node = CreateNode< AssetsStorageNode >();
            var property = ( AssetsStorageNode.Property )node.PropertyBlock;

            property.DllVersion = testDllVersion;
            property.NameSpace = testNameSpace;
            property.TypeName = testTypeName;
            property.PathToFinalDll = new PathToFile( "*.dll", pathToFinalDll );

            Execute( node ).TryGetPortValue( node.Ports.Single(), out ProcessorLinkInformation outputNode );

            var generatedAssetStorageModel = ( AssetsStorageModel )outputNode;

            Assert.IsNotNull( generatedAssetStorageModel );
            Assert.That( generatedAssetStorageModel.TypeName, Is.EqualTo( testTypeName ) );
            Assert.That( generatedAssetStorageModel.DllVersion, Is.EqualTo( testDllVersion ) );
            Assert.That( generatedAssetStorageModel.NameSpace, Is.EqualTo( testNameSpace ) );
            Assert.That( Path.GetFullPath( generatedAssetStorageModel.PathToFinalDll ), Is.EqualTo( pathToFinalDll ) );
            Assert.That( generatedAssetStorageModel.DllName, Is.EqualTo( testDllName ) );
            var assetsStorageType = typeof( BaseAssetsStorage );

            Assert.That( generatedAssetStorageModel.BaseType, Is.EqualTo( assetsStorageType ) );

            var processorsConnectionMethod = assetsStorageType.GetMethodInfoByMethodInputParamType( typeof( IInputNode<> ) );

            Assert.That( generatedAssetStorageModel.ProcessorsLinkerName, Is.EqualTo( processorsConnectionMethod.Name ) );
            Assert.That( generatedAssetStorageModel.ExpectedLinkType, Is.EqualTo( processorsConnectionMethod.GetParameters()[ 0 ].ParameterType ) );
        }

        [ Test ]
        public void Execute_Should_Throw_ArgumentNullException()
        {
            var node = CreateNode< AssetsStorageNode >();
            var property = ( AssetsStorageNode.Property )node.PropertyBlock;

            property.NameSpace = null;
            CheckThatExecutingThrowException();

            property.NameSpace = "SomeNamespace";
            property.TypeName = null;
            CheckThatExecutingThrowException();

            property.TypeName = "Test";
            property.DllVersion = "some_version";
            CheckThatExecutingThrowException();

            property.DllVersion = "1.0.0.0";
            property.PathToFinalDll = new PathToFile();
            CheckThatExecutingThrowException();

            void CheckThatExecutingThrowException()
            {
                Assert.Throws< ArgumentNullException >( () => Execute( node ) );
            }
        }

        [ Test ]
        public void Execute_Should_Throw_NotSupportedException()
        {
            var node = CreateNode< AssetsStorageNode >();
            var property = ( AssetsStorageNode.Property )node.PropertyBlock;
            property.DllVersion = "1.0.0.0";
            property.NameSpace = "Test";
            property.TypeName = "Test";
            property.PathToFinalDll = new PathToFile( "*.dll", Path.Combine( "SomeFolder", "SomePath.dll" ) );

            Assert.Throws< NotSupportedException >( () => Execute( node ) );
        }
    }
}
using System;
using System.Linq;
using NUnit.Framework;
using static CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests.Utils;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class ExtraParamTypeNodeTests
    {
        [ Test ]
        public void Execute_Should_Succeed()
        {
            var testType = typeof( string );
            var node = CreateNode< ExtraParamTypeNode >();
            (node.PropertyBlock as ExtraParamTypeNode.Property).Type = new TypeProvider( testType.FullName );

            Execute( node ).TryGetPortValue( node.Ports.Single(), out Type resultType );
            Assert.That( resultType, Is.EqualTo( testType ) );
        }

        [ Test ]
        public void Execute_Should_Throw_ArgumentNullException()
        {
            var node = CreateNode< ExtraParamTypeNode >();
            (node.PropertyBlock as ExtraParamTypeNode.Property).Type = new TypeProvider( null );
            Assert.Throws< ArgumentNullException >( () => Execute( node ) );
        }
    }
}
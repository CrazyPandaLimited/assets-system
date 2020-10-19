using System;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class NamePropertyTests
    {
        [ TestCase(" Test Wrong Value   ","testWrongValue", SyntaxKind.None) ] 
        [ TestCase("   test Wrong Value   ","TestWrongValue", SyntaxKind.PublicKeyword) ] 
        [ TestCase("   Test     Wrong Value !!!!!!!! ;;;;  ","_testWrongValue", SyntaxKind.PrivateKeyword) ] 
        public void GetFormattedName_Should_Succeed(string wrongValue, string expectedValue, SyntaxKind visibility)
        {
            var property = new NameProperty();
            property.SetFormattedName( wrongValue, visibility );
            Assert.That( expectedValue, Is.EqualTo( property.MemberName ) );
        }

        [ Test ]
        public void GetFormattedName_Should_ThrowArgumentNullException()
        {
            var property = new NameProperty();
            Assert.Throws< ArgumentNullException >( () => property.SetFormattedName( "         " ) );
        }
    }
}
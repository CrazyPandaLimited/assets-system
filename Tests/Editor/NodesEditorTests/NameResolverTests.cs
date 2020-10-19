using System.Linq;
using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class NameResolverTests
    {
        [ Test ]
        public void GetFixedName_Should_Succeed()
        {
            var nameToTest = "someName";
            var expectedName = nameToTest;

            var nameResolver = new NameResolver();
            
            foreach( var name in Enumerable.Repeat( nameToTest, 10 ) )
            {
                Assert.That( expectedName, Is.EqualTo( nameResolver.GetFixedName( name ) ) );
                expectedName = $"_{expectedName}";
            }
        }
    }
}
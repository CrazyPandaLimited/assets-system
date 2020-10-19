using System;
using System.Linq;
using System.Numerics;
using CrazyPanda.UnityCore.AssetsSystem.CodeGen;
using NUnit.Framework;
using static CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests.Utils;

namespace CrazyPanda.UnityCore.AssetsSystem.NodesEditorTests
{
    [ Category( "LocalTests" ), TestFixture ]
    public sealed class ExtraParamNodeTests
    {
        [ TestCase( typeof( int ), -500 ) ]
        [ TestCase( typeof( uint ), 500 ) ]
        [ TestCase( typeof( decimal ), 5.4 ) ]
        [ TestCase( typeof( float ), 5.50f ) ]
        [ TestCase( typeof( double ), 5.50555 ) ]
        [ TestCase( typeof( long ), -500000 ) ]
        [ TestCase( typeof( ulong ), 500000 ) ]
        [ TestCase( typeof( char ), 'm' ) ]
        [ TestCase( typeof( bool ), true ) ]
        [ TestCase( typeof( bool ), false ) ]
        [ TestCase( typeof( Vector3 ) ) ]
        [ TestCase( typeof( string ), "" ) ]
        [ TestCase( typeof( string ), "some_value" ) ]
        public void Execute_Should_Succeed( Type testType, object testValue = default)
        {
            var needToGenerateDefaultValue = EqualsValueClauseSyntaxMapper.TypeCanBeConvertedToEqualsValueClause( testType );
            var node = CreateNode< ExtraParamNode >();

            var propertyBlock = node.PropertyBlock as ExtraParamNode.Property;
            propertyBlock.MakeDefaultValue = needToGenerateDefaultValue;
            propertyBlock.DefaultValue = testValue?.ToString() ?? string.Empty;
            propertyBlock.Type = new TypeProvider( testType.FullName );
            (node.Type as IExecutionNode).OnPropertyChanged( node, nameof(ExtraParamNode.Property.Type) );

            Execute( node ).TryGetPortValue( node.Ports.Single(), out CtorParameter result );

            Assert.That( result.Type, Is.EqualTo( testType ) );
            Assert.That( result.MakeValue, Is.EqualTo( needToGenerateDefaultValue ) );

            if( needToGenerateDefaultValue )
            {
                Assert.That( result.Value, Is.EqualTo( testValue ) );
            }
        }

        [ Test ]
        public void OnPropertyChanged_Should_Succeed_Recreate_OutputPort_AfterTypeWasChanged()
        {
            var node = CreateNode< ExtraParamNode >();

            var testTypes = new[] { typeof( string ), typeof( Vector3 ), typeof( int ), typeof( char ) };

            foreach( var type in testTypes )
            {
                ChangePortType( type.FullName );
             
                Assert.That( node.Ports.Count, Is.EqualTo( 1 ) );
                Assert.That( node.Ports.Single().Type.GetGenericArguments()[0], Is.EqualTo( type ) );
            }
            
            ChangePortType( string.Empty );
            Assert.That( node.Ports, Is.Empty );

            void ChangePortType(string fullTypeName)
            {
                var propertyBlock = node.PropertyBlock as ExtraParamNode.Property;
                propertyBlock.Type = new TypeProvider( fullTypeName );
                (node.Type as IExecutionNode).OnPropertyChanged( node, nameof(ExtraParamNode.Property.Type) );
            }
        }

        [ Test ]
        public void Execute_Should_Throw_ArgumentNullException()
        {
            var node = CreateNode< ExtraParamNode >();
            (node.PropertyBlock as ExtraParamNode.Property).Type = new TypeProvider( null );
            Assert.Throws< ArgumentNullException >( () => Execute( node ) );
        }
    }
}
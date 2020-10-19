using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    static class EqualsValueClauseSyntaxMapper
    {
        private static readonly IReadOnlyDictionary< Type, Func< object, EqualsValueClauseSyntax > > _mapperStorage = 
            new Dictionary< Type, Func< object, EqualsValueClauseSyntax > >
            {
                [typeof(int)] = value => GetValueFromNumber( Literal( Convert.ToInt32( value ) ) ),
                [typeof(uint)] = value => GetValueFromNumber( Literal( Convert.ToUInt32( value ) ) ),
                [typeof(decimal)] = value => GetValueFromNumber( Literal( Convert.ToDecimal( value ) ) ),
                [typeof(float)] = value => GetValueFromNumber( Literal( Convert.ToSingle( value ) ) ),
                [typeof(double)] = value => GetValueFromNumber( Literal( Convert.ToDouble( value ) ) ),
                [typeof(long)] = value => GetValueFromNumber( Literal( Convert.ToInt64( value ) ) ),
                [typeof(ulong)] = value => GetValueFromNumber( Literal( Convert.ToUInt64( value ) ) ),
                [typeof(char)] = value => GetValue( SyntaxKind.CharacterLiteralExpression, Literal( Convert.ToChar( value ) ) ),
                [typeof(string)] = value => GetValue( SyntaxKind.StringLiteralExpression, Literal( Convert.ToString( value ) ) ),
                [typeof(bool)] = value =>
                {
                    var boolValue = Convert.ToBoolean( value );
                    return GetValue( boolValue ? SyntaxKind.TrueLiteralExpression : SyntaxKind.FalseLiteralExpression );
                },
            };
        
        public static EqualsValueClauseSyntax GetEqualsValueClauseSyntax( Type type, object value )
        {
            if( value != null && _mapperStorage.TryGetValue( type, out var mapper ) )
            {
                return mapper.Invoke( value );
            }

            return GetValue( SyntaxKind.DefaultLiteralExpression );
        }
        
        public static bool TypeCanBeConvertedToEqualsValueClause( Type type )
        {
            return _mapperStorage.ContainsKey( type );
        }

        private static EqualsValueClauseSyntax GetValueFromNumber( SyntaxToken value )
        {
            return EqualsValueClause( LiteralExpression( SyntaxKind.NumericLiteralExpression, value ) );
        }

        private static EqualsValueClauseSyntax GetValue( SyntaxKind kind, SyntaxToken value )
        {
            return EqualsValueClause( LiteralExpression( kind, value ) );
        }

        private static EqualsValueClauseSyntax GetValue( SyntaxKind kind )
        {
            return EqualsValueClause( LiteralExpression( kind ) );
        }

    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    static class SyntaxFactoryUtils
    {
        private static readonly SyntaxToken _publicTokenKeyWord = Token( SyntaxKind.PublicKeyword );
        private static readonly SyntaxToken _privateTokenKeyWord = Token( SyntaxKind.PrivateKeyword );

        public static NamespaceDeclarationSyntax CreateNameSpace( string nameSpaceName )
        {
            return NamespaceDeclaration( IdentifierName( nameSpaceName ) );
        }

        public static BaseTypeSyntax CreateBaseType( Type type )
        {
            return SimpleBaseType( ParseTypeName( type.GetFullGenericName() ) );
        }

        public static ClassDeclarationSyntax CreateClassDeclarationSyntax(string typeName)
        {
            return ClassDeclaration( typeName ).AddModifiers( _publicTokenKeyWord, Token( SyntaxKind.SealedKeyword ) ) ;
        }

        public static ConstructorDeclarationSyntax CreateConstructor( string typeName, IEnumerable< ParameterSyntax > ctorParams )
        {
            return ConstructorDeclaration( typeName ).AddModifiers( _publicTokenKeyWord ).WithParameterList( ParameterList( SeparatedList( ctorParams ) ) );
        }

        public static ParameterSyntax CreateParameter( Type type, string name )
        {
            return Parameter( Identifier( name ) ).WithType( ParseTypeName( type.GetFullGenericName() ) );
        }

        public static PropertyDeclarationSyntax CreateProperty( Type type, string name, IEnumerable< SyntaxKind > modifiers )
        {
            return PropertyDeclaration( ParseTypeName( type.GetFullGenericName() ), Identifier( name ) )
                   .WithModifiers( TokenList(modifiers.Select( Token )) )
                   .WithAccessorList( AccessorList( List( new[]
                   {
                       AccessorDeclaration( SyntaxKind.GetAccessorDeclaration ).WithSemicolonToken( Token( SyntaxKind.SemicolonToken ) )
                   } ) ) )
                   .NormalizeWhitespace();
        }

        public static ArgumentSyntax CreateArgument( string name )
        {
            return Argument( ParseName( name ) );
        }

        public static ArgumentSyntax CreateMemberAccessArgument( string name, string memberName )
        {
            return Argument( GetMemberAccess( name, memberName ) );
        }

        public static AttributeSyntax CreateDllVersionAttribute(string dllVersion)
        {
            return Attribute( ParseName(  typeof( AssemblyVersionAttribute ).GetFullGenericName() ) )
                .WithArgumentList( AttributeArgumentList( SingletonSeparatedList( AttributeArgument( LiteralExpression( SyntaxKind.StringLiteralExpression, Literal( dllVersion ) ) ) ) ) ) ;
        }

        public static MethodDeclarationSyntax CreateVoidMethod(string name)
        {
            return MethodDeclaration( PredefinedType(Token(SyntaxKind.VoidKeyword)), name ).AddModifiers( _privateTokenKeyWord );
        }

        public static StatementSyntax CallMethod( string methodName)
        {
            return CallMethod( methodName, Enumerable.Empty< ArgumentSyntax >() );
        }

        public static StatementSyntax CallMethod( string methodName, params ArgumentSyntax[] arguments )
        {
            return CallMethod( methodName, arguments.AsEnumerable() );
        }
        
        public static StatementSyntax CallMethod( string methodName, IEnumerable< ArgumentSyntax > arguments )
        {
            return ExpressionStatement( InvocationExpression( ParseName( methodName ), ArgumentList( SeparatedList( arguments ) ) ) );
        }

        public static StatementSyntax CallMemberMethod(string memberName, string methodName, params ArgumentSyntax[] arguments)
        {
            return CallMemberMethod( memberName, methodName, arguments.AsEnumerable() );
        }
        
        public static StatementSyntax CallMemberMethod(string memberName, string methodName, IEnumerable<ArgumentSyntax> arguments)
        {
            var memberAccess = GetMemberAccess( memberName,methodName );
            return ExpressionStatement( InvocationExpression( memberAccess, ArgumentList( SeparatedList( arguments ) ) ) );
        }

        public static MemberAccessExpressionSyntax GetMemberAccess(string className, string classMemberName)
        {
            return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ParseName( className ), IdentifierName( classMemberName ));
        }

        public static ObjectCreationExpressionSyntax CallConstructor( Type typeName, IEnumerable<ArgumentSyntax> arguments )
        {
            return ObjectCreationExpression( ParseTypeName( typeName.GetFullGenericName() ) ).WithArgumentList( ArgumentList( SeparatedList( arguments ) ) );
        }
    }
}
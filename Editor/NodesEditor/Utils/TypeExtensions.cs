using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    static class TypeExtensions
    {
        internal static string GetOpenGenericName( this Type type )
        {
            string shortTypeName = GetShortName( type );
            var genericArgsLength = type.GetGenericArguments().Length;
            string genericArgs = genericArgsLength > 0 ? new string( ',', genericArgsLength - 1 ) : string.Empty;
            return $"{shortTypeName}{(genericArgsLength > 0 ? $"<{genericArgs}>" : string.Empty)}";
        }
        
        internal static string GetGenericName( this Type type )
        {
            string genericTypeName = GetShortName( type );
            string genericArgs = string.Join( ",", type.GetGenericArguments().Select( GetGenericName ).ToArray() );
            return $"{genericTypeName}{(genericArgs.Length > 0 ? $"<{genericArgs}>" : string.Empty)}";
        }

        internal static string GetFullGenericName( this Type type )
        {
            string genericTypeName = GetShortName( type );
            string genericArgs = string.Join( ",", type.GetGenericArguments().Select( GetFullGenericName ).ToArray() );
            
            return $"{type.Namespace}.{type.GetGenericDeclaringTypesName()}.{genericTypeName}{(genericArgs.Length > 0 ? $"<{genericArgs}>" : string.Empty)}".Replace( "..", "." );
        }

        internal static string GetShortName( this Type type )
        {
            const char delimiter = '`';
            var typeName = type.Name;
            return typeName.Contains( delimiter ) ? typeName.Substring( 0, typeName.IndexOf( delimiter ) ) : typeName;
        }

        internal static bool HasInterface( this Type type, Type interfaceTypeToCheck )
        {
            return type.GetInterfaces().Prepend( type ).Any( typeToCheck => 
                    typeToCheck == interfaceTypeToCheck || (typeToCheck.IsGenericType && typeToCheck.GetGenericTypeDefinition() == interfaceTypeToCheck) );
        }
        
        internal static bool IsOpenGenericType( this Type type )
        {
            return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
        }

        internal static MethodInfo GetMethodInfoByMethodInputParamType( this Type type, Type methodParamType)
        {
            return type.GetMethods( BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic ).First( method =>
            {
                var methodParams = method.GetParameters();

                if( methodParams.Length != 1 )
                {
                    return false;
                }

                var firstMethodParamType = methodParams[ 0 ].ParameterType;
                return methodParamType.IsAssignableFrom( firstMethodParamType ) || ( methodParamType.IsInterface && firstMethodParamType.HasInterface( methodParamType ) );
            } );
        }

        internal static IEnumerable< Assembly > GetReferenceAssemblies( this Type type )
        {
            var loadedAssembly = type.Assembly;

            if( !loadedAssembly.IsDynamic )
            {
                yield return type.Assembly;
            }

            foreach( var genericArgument in type.GetGenericArguments() )
            {
                foreach( var assembly in genericArgument.GetReferenceAssemblies() )
                {
                    yield return assembly;
                }
            }
        }
        
        private static string GetGenericDeclaringTypesName( this Type type )
        {
            if( !type.IsNested )
            {
                return string.Empty;
            }

            var result = new List<string>();
            
            for( Type iterationType = type.DeclaringType; iterationType != null; iterationType = iterationType.DeclaringType )
            {
                result.Add( iterationType.GetGenericName() );
            }

            result.Reverse();

            return string.Join( ".", result );
        }
    }
}
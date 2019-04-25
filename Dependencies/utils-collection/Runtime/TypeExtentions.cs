using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyPanda.UnityCore.Utils
{
	public static class TypeExtentions
	{
		#region Private Fields
		private static Dictionary< string, string > _valueTypeNameToKeywordMap = new Dictionary< string, string >
		{
			{
				"String", "string"
			},
			{
				"Boolean", "bool"
			},
			{
				"Byte", "byte"
			},
			{
				"Char", "char"
			},
			{
				"Decimal", "decimal"
			},
			{
				"Double", "double"
			},
			{
				"Single", "float"
			},
			{
				"Int32", "int"
			},
			{
				"SByte", "sbyte"
			},
			{
				"UInt32", "uint"
			},
			{
				"UInt64", "ulong"
			},
			{
				"UInt16", "ushort"
			},
			{
				"Int16", "short"
			},
			{
				"Int64", "long"
			}
		};
		#endregion

		#region Public Members
		/// <summary>
		/// Вернет красивое представление generic типа. Например, вместо List`1 => List<int>
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static string CSharpName( this Type type )
		{
			if( type == typeof( void ) )
			{
				return "void";
			}

			if( type == typeof( object ) )
			{
				return "object";
			}

			var name = type.Name;

			if( name[ name.Length - 1 ] == '&' )
			{
				name = "ref " + name.Substring( 0, name.Length - 1 );
			}

			var sb = new StringBuilder();
			if( !type.IsGenericType )
			{
				return ValueTypeNameToKeyword( name );
			}
			var indexOf = name.IndexOf( '`' );
			sb.Append( indexOf != -1 ? name.Substring( 0, indexOf ) : name );
			sb.Append( "<" );
			sb.Append( string.Join( ", ", type.GetGenericArguments().Select( t => t.CSharpName() ).ToArray() ) );
			sb.Append( ">" );

			return sb.ToString();
		}
		#endregion

		#region Private Members
		public static string ValueTypeNameToKeyword( string valueTypeName )
		{
			string map;
			if( _valueTypeNameToKeywordMap.TryGetValue( valueTypeName, out map ) )
			{
				return map;
			}
			return valueTypeName;
		}
		#endregion
	}
}

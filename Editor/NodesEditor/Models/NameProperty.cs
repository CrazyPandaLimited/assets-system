using System;
using System.Text;
using System.Text.RegularExpressions;
using CrazyPanda.UnityCore.NodeEditor;
using Microsoft.CodeAnalysis.CSharp;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class NameProperty : PropertyBlock
    {
        public string MemberName = string.Empty;
        
        public void SetFormattedName( string name, SyntaxKind? syntaxKind = null )
        {
            var memberNameWithoutSpaces = name.Replace( " ", string.Empty );

            if( string.IsNullOrEmpty( memberNameWithoutSpaces ) )
            {
                throw new ArgumentNullException( nameof(name) );
            }

            var memberNameBuilder = new StringBuilder();
            
            switch( syntaxKind )
            {
                default:
                    memberNameBuilder.Append( char.ToLower( memberNameWithoutSpaces[ 0 ] ) );
                    break;
                case SyntaxKind.PublicKeyword:
                    memberNameBuilder.Append( char.ToUpper( memberNameWithoutSpaces[ 0 ] ) );
                    break;
                case SyntaxKind.PrivateKeyword:
                    memberNameBuilder.Append( "_" ).Append( char.ToLower( memberNameWithoutSpaces[ 0 ] ) );
                    break;
            }

            if( memberNameWithoutSpaces.Length > 1 )
            {
                memberNameBuilder.Append( memberNameWithoutSpaces.Substring( 1 ) );
            }

            MemberName = Regex.Replace( memberNameBuilder.ToString(), @"([^\w\\_\d]+)*", string.Empty );
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis.CSharp;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    [Serializable]
    public sealed class NameResolver
    {
        private readonly HashSet<string> _alreadyCreatedNames = new HashSet< string >();
        
        //Here we fix same bug name
        public string GetFixedName( string name )
        {
            var fixedName = name;
            
            while( _alreadyCreatedNames.Contains( fixedName ) )
            {
                fixedName = $"_{fixedName}";
            }

            _alreadyCreatedNames.Add( fixedName );
            return fixedName;
        }
    }
}
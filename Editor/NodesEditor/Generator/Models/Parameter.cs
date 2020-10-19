using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    [Serializable]
    public class Parameter : IEquatable<Parameter>
    {
        private readonly HashSet< SyntaxKind > _keywords = new HashSet< SyntaxKind >();

        public Parameter( Type type, string name, params SyntaxKind[] keywords )
        {
            Type = type;
            Name = name;
            _keywords.UnionWith( keywords );
        }
        
        public Type Type { get; set; }

        public string Name { get; set; } = string.Empty;

        public ICollection< SyntaxKind > Keywords => _keywords;

        public bool Equals( Parameter other )
        {
            if( ReferenceEquals( null, other ) )
            {
                return false;
            }

            if( ReferenceEquals( this, other ) )
            {
                return true;
            }

            return Type == other.Type && Name == other.Name;
        }

        public override bool Equals( object obj )
        {
            if( ReferenceEquals( null, obj ) )
            {
                return false;
            }

            if( ReferenceEquals( this, obj ) )
            {
                return true;
            }

            if( obj.GetType() != this.GetType() )
            {
                return false;
            }

            return Equals( ( Parameter ) obj );
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ( ( Type != null ? Type.GetHashCode() : 0 ) * 397 ) ^ ( Name != null ? Name.GetHashCode() : 0 );
            }
        }
    }
}
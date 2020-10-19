using System;

namespace CrazyPanda.UnityCore.AssetsSystem.CodeGen
{
    [Serializable]
    public class CtorParameter : Parameter
    {
        private readonly bool _makeValue = false;
        
        public CtorParameter( Type type, string name, bool makeDefaultValue = false, object value = default ) : base( type, name )
        {
            Value = value;
            _makeValue = makeDefaultValue;
        }

        public object Value { get; }

        public bool MakeValue => _makeValue && EqualsValueClauseSyntaxMapper.TypeCanBeConvertedToEqualsValueClause( Type );
    }

    [Serializable]
    public sealed class CtorParameter< T > : CtorParameter
    {
        public CtorParameter( string name, bool makeDefaultValue = false, object value = default ) : base( typeof( T ), name, makeDefaultValue, value )
        {
        }
    }
}
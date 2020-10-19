using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.NodeEditor;
using Newtonsoft.Json;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public readonly struct TypeProvider
    {
        private static readonly Lazy<IReadOnlyDictionary<string,Type> > _allLoadedTypes = new Lazy< IReadOnlyDictionary<string,Type> >( () =>
        {
            var dictionary = new Dictionary<string,Type>();
            var loadedTypes = AppDomain.CurrentDomain.GetAssemblies().SelectMany( assembly => assembly.GetTypes() );
            
            foreach( var type in loadedTypes )
            {
                dictionary[ type.FullName ] = type;
            }
            
            return dictionary;
        });
        
        [ JsonProperty ] 
        private readonly string _fullTypeName;

        static TypeProvider()
        {
            ObjectPropertiesField.RegisterEditorMapping( typeof( TypeProvider ), CreateVisualItem );
        }
        
        public TypeProvider( string fullTypeName )
        {
            _fullTypeName = fullTypeName;
        }
        
        [JsonIgnore]
        public Type Type
        {
            get
            {
                if( string.IsNullOrEmpty( _fullTypeName ) || !_allLoadedTypes.Value.TryGetValue( _fullTypeName.Trim(), out var type ) )
                {
                    return null;
                }

                return type;
            }
        }

        public override string ToString()
        {
            return _fullTypeName;
        }

        private static VisualElement CreateVisualItem( string label, object value, Action< object > setter )
        {
            return ObjectPropertiesField.CreateFieldEditor< TextField, string >( label, value.ToString(), s => setter( new TypeProvider( ( string )s ) ) );
        }
    }
}
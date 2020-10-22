using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class SearchTypesWindowProvider : ScriptableObject, ISearchWindowProvider
    {
        private static readonly Lazy< Dictionary< string, Dictionary< string, List< Type > > > > _allLoadedTypes = 
            new Lazy< Dictionary< string, Dictionary< string, List< Type > > > >( () =>
        {
            var result = new Dictionary< string, Dictionary< string, List< Type > > >();

            foreach( var assembly in AppDomain.CurrentDomain.GetAssemblies().OrderBy( assembly => assembly.GetName().Name ) )
            {
                if( !ShouldProcessAssembly( assembly ) )
                {
                    continue;
                }
                
                var typesDictionary = new Dictionary< string, List< Type > >();

                foreach( var type in assembly.GetTypes() )
                {
                    if( !ShouldProcessType( type ) )
                    {
                        continue;
                    }

                    var nameSpace = type.Namespace ?? "Types Without Namespace";
                    
                    if( !typesDictionary.TryGetValue( nameSpace, out var typesCollection ) )
                    {
                        typesCollection = new List< Type >();
                        typesDictionary[ nameSpace ] = typesCollection;
                    }

                    typesCollection.Add( type );
                }

                if( typesDictionary.Count > 0 )
                {
                    result[ assembly.GetName().Name ] = typesDictionary;
                }
            }

            return result;
        } );

        public event Action< Type > OnTypeSelected;
        
        public List< SearchTreeEntry > CreateSearchTree( SearchWindowContext context )
        {
            const int assemblyNameSearchGroupLevel = 1;
            const int nameSpaceSearchGroupLevel = 2;
            const int typeSearchTreeLevel = 3;

            var searchTrees = new List< SearchTreeEntry >
            {
                CreateSearchTreeGroupEntry( "Choose Type" )
            };

            foreach( var pair in _allLoadedTypes.Value )
            {
                searchTrees.Add( CreateSearchTreeGroupEntry( $"{pair.Key}.dll", assemblyNameSearchGroupLevel ) );

                foreach( var typeInfoPair in pair.Value )
                {
                    searchTrees.Add( CreateSearchTreeGroupEntry( typeInfoPair.Key, nameSpaceSearchGroupLevel ) );

                    foreach( var type in typeInfoPair.Value )
                    {
                        searchTrees.Add( CreateSearchTreeEntry( type, typeSearchTreeLevel) );
                    }
                }
            }

            return searchTrees;
        }

        public bool OnSelectEntry( SearchTreeEntry searchTreeEntry, SearchWindowContext context )
        {
            OnTypeSelected?.Invoke( ( Type ) searchTreeEntry.userData );
            return true;
        }

        private SearchTreeGroupEntry CreateSearchTreeGroupEntry( string label, int level = 0 )
        {
            return new SearchTreeGroupEntry(new GUIContent( label ), level );
        } 
        
        private SearchTreeEntry CreateSearchTreeEntry( Type type, int level )
        {
            return new SearchTreeEntry( new GUIContent( type.Name ) )
            {
                userData = type,
                level = level
            };
        }

        private static bool ShouldProcessType(Type type)
        {
            return !(type.IsAbstract && type.IsSealed) && !type.IsGenericType && (type.IsNestedPublic || type.IsPublic);
        }

        private static bool ShouldProcessAssembly( Assembly assembly )
        {
            var assemblyName = assembly.GetName().Name;
            return !assemblyName.StartsWith( "UnityEditor" ) && !assemblyName.Contains( "Editor" ) && !assemblyName.Contains( "Tests" );
        }
    }
}
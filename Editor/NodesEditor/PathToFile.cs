using System;
using System.IO;
using CrazyPanda.UnityCore.NodeEditor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public readonly struct PathToFile
    {
        private static readonly Lazy< string > _rootProjectPath = new Lazy< string >( () => new DirectoryInfo( Application.dataPath ).Parent.FullName.Replace( "\\", "/" ) );
        
        [JsonProperty]
        private readonly string _fileExtension;
        [JsonProperty]
        private readonly string _pathToFile;

        [JsonIgnore]
        public string Path => System.IO.Path.GetFullPath( _pathToFile );

        static PathToFile()
        {
            ObjectPropertiesField.RegisterEditorMapping( typeof( PathToFile ), CreatePathToFileVisualItem );
        }

        public PathToFile( string fileExtension, string pathToFile )
        {
            _fileExtension = fileExtension;
            _pathToFile = pathToFile;
        }

        public override string ToString()
        {
            return _pathToFile;
        }

        private static VisualElement CreatePathToFileVisualItem( string label, object value, Action< object > setter )
        {
            var fileExtension = ((PathToFile) value)._fileExtension;
            var view = ObjectPropertiesField.CreateFieldEditor< TextField, string >( label, value.ToString(), s => setter( new PathToFile( fileExtension, ( string )s ) ) );

            var button = new Button( () =>
            {
                var selectedPath = EditorUtility.SaveFilePanel( "Select file", view.value, string.Empty, fileExtension );

                if( !string.IsNullOrEmpty( selectedPath ) )
                {
                    view.value = FixProjectPath(selectedPath);
                }
            } ) { name = "browse-button", text = "..." };

            view.Add( button );

            return view;
        }

        private static string FixProjectPath( string selectedPath )
        {
            var fixedPath = selectedPath;
            
            if( fixedPath.StartsWith( _rootProjectPath.Value ) )
            {
                fixedPath = fixedPath.Replace( _rootProjectPath.Value, string.Empty );

                if( fixedPath.StartsWith( "/" ) )
                {
                    fixedPath = fixedPath.Substring( 1 );
                }
            }

            return fixedPath;
        }
    }
}
using System;
using CrazyPanda.UnityCore.NodeEditor;
using UnityEditor;
using UnityEditor.Experimental.AssetImporters;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    [ScriptedImporter(0, ConstExtension, -1)]
    public sealed class BuilderGeneratorGraphAssetImporter : BaseGraphAssetImporter
    {
        private const string ConstExtension = "resbuildergengraph";

        public override string Extension => ConstExtension;
        public override Type EditorWindowType => typeof( BuilderGeneratorGraphEditorWindow );

        [ MenuItem( "Assets/Create/Resources Builder Generator Graph", false, 208 ) ]
        public static void CreateGraph()
        {
            CreateNewGraphAsset( new GraphModel( new BuilderGeneratorGraphType() ), $"DefaultBuilder.{ConstExtension}" );
        }
    }
    
}
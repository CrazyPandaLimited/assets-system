using System;
using System.IO;
using CrazyPanda.UnityCore.NodeEditor;
using UnityEditor;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    sealed class BuilderGeneratorGraphEditorWindow : BaseGraphEditorWindow
    {
        protected override BaseGraphEditorView CreateEditorView()
        {
            return new BuilderGeneratorGraphEditorView( () => Path.GetFileNameWithoutExtension( AssetDatabase.GUIDToAssetPath( base.GraphAssetGuid ) ) );
        }
    }
}
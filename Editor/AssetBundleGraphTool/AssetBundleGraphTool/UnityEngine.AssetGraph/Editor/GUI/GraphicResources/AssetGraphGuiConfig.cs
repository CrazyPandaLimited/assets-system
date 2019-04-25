using System;
using Boo.Lang;
using UnityEditor;

namespace UnityEngine.AssetGraph
{
    [CreateAssetMenu(fileName = "AssetGraphGuiResourcesConfig", menuName = "Resource system/AssetGraphGuiSettings", order = 1)]
    public class AssetGraphGuiConfig: ScriptableObject 
    {
        #region Load config
        private const string GRAPHICS_SETTINGS_FILE_NAME = "t:AssetGraphGuiConfig";
        public static AssetGraphGuiConfig LoadGraphicsSettings()
        {
            string path = null;
            foreach (var findAsset in AssetDatabase.FindAssets(GRAPHICS_SETTINGS_FILE_NAME))
            {
                if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".asset"))
                {
                    path = AssetDatabase.GUIDToAssetPath(findAsset);
                    break;
                }
            }

            return AssetDatabase.LoadAssetAtPath<AssetGraphGuiConfig>(path);
        }
        #endregion

        public Texture2D AssetGraphWindow;
        public Texture2D ConfigGraphIcon;
        public Texture2D ConnectionPoint;
        public Texture2D d_AssetGraphWindow;
        public Texture2D InputBG;
        public Texture2D OutputBG;
        public GUISkin NodeSkin;
        public GraphNode[] NodesGraphics;

        public float NODE_BASE_WIDTH = 120f;
        public float NODE_BASE_HEIGHT = 40f;
        public float NODE_WIDTH_MARGIN = 48f;
        public float NODE_TITLE_HEIGHT_MARGIN = 8f;

        public float CONNECTION_ARROW_WIDTH = 12f;
        public float CONNECTION_ARROW_HEIGHT = 15f;

        public float INPUT_POINT_WIDTH = 21f;
        public float INPUT_POINT_HEIGHT = 29f;

        public float OUTPUT_POINT_WIDTH = 10f;
        public float OUTPUT_POINT_HEIGHT = 23f;

        public float FILTER_OUTPUT_SPAN = 32f;

        public float CONNECTION_POINT_MARK_SIZE = 16f;

        public float CONNECTION_CURVE_LENGTH = 20f;

        public float TOOLBAR_HEIGHT = 20f;
        public float TOOLBAR_GRAPHNAMEMENU_WIDTH = 150f;
        public int TOOLBAR_GRAPHNAMEMENU_CHAR_LENGTH = 20;

        public Color COLOR_ENABLED = new Color(0.43f, 0.65f, 1.0f, 1.0f);
        public Color COLOR_CONNECTED = new Color(0.9f, 0.9f, 0.9f, 1.0f);
        public Color COLOR_NOT_CONNECTED = Color.grey;
        public Color COLOR_CAN_CONNECT = Color.white;//new Color(0.60f, 0.60f, 1.0f, 1.0f);
        public Color COLOR_CAN_NOT_CONNECT = new Color(0.33f, 0.33f, 0.33f, 1.0f);

        public Texture2D GetNodeGraphicsByName(string name)
        {
            foreach (var graphNode in NodesGraphics)
            {
                if (graphNode.node.name == name)
                {
                    return graphNode.node;
                }
                
                if (graphNode.node2x.name == name)
                {
                    return graphNode.node2x;
                }
                if (graphNode.OnNode.name == name)
                {
                    return graphNode.OnNode;
                }
                if (graphNode.OnNode2x.name == name)
                {
                    return graphNode.OnNode2x;
                }
            }

            return null;
        }
        
        [Serializable]
        public class GraphNode
        {
            public Texture2D node;
            public Texture2D node2x;
            public Texture2D OnNode;
            public Texture2D OnNode2x;
        }
    }
}
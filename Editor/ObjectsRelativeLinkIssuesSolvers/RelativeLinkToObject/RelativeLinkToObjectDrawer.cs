using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Experimental.RelativeLinkToObject.Editor
{
    [CustomPropertyDrawer(typeof(RelativeLinkToObject))]
    public class RelativeLinkToObjectDrawer : PropertyDrawer
    {
        #region Public Members

        private const string PROPERTY_ASSET_PATH_NAME = "_assetPath";
        private const string PROPERTY_ASSET_TYPE = "_assetType";
        private const string PROPERTY_ASSET_INTO_RESOURCES_NAME = "_assetIntoResourcesFolder";

        private Dictionary<string, Object> _loadedObjectsCache = new Dictionary<string, Object>();

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var propertyPath = property.propertyPath;

            var assetPath = property.FindPropertyRelative(PROPERTY_ASSET_PATH_NAME).stringValue;
            var isAssetIntoResourcesFolder =
                property.FindPropertyRelative(PROPERTY_ASSET_INTO_RESOURCES_NAME).boolValue;
            var assetType = (AssetType) property.FindPropertyRelative(PROPERTY_ASSET_TYPE).enumValueIndex;

            Object cachedObject;
            _loadedObjectsCache.TryGetValue(property.propertyPath, out cachedObject);

            if (!string.IsNullOrEmpty(assetPath) && cachedObject == null)
            {
                if (isAssetIntoResourcesFolder)
                {
                    switch (assetType)
                    {
                        case AssetType.Object:
                            cachedObject = Resources.Load(assetPath, typeof(Object));
                            break;
                        case AssetType.GameObject:

                            cachedObject = Resources.Load(assetPath, typeof(GameObject));
                            break;
                        case AssetType.Texture:

                            cachedObject = Resources.Load(assetPath, typeof(Texture));
                            break;
                        case AssetType.Material:

                            cachedObject = Resources.Load(assetPath, typeof(Material));
                            break;
                    }
                }
                else
                {
                    switch (assetType)
                    {
                        case AssetType.Object:
                            cachedObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
                            break;
                        case AssetType.GameObject:
                            cachedObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject));
                            break;
                        case AssetType.Texture:
                            cachedObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture));
                            break;
                        case AssetType.Material:
                            cachedObject = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Material));
                            break;
                    }
                }

                _loadedObjectsCache[propertyPath] = cachedObject;
            }

            if (!string.IsNullOrEmpty(assetPath))
            {
                GUI.color = new Color(0.7f, 1, 0.7f);
            }
            else
            {
                GUI.color = Color.gray;
            }

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var newObject = cachedObject;

            switch (assetType)
            {
                case AssetType.Object:
                    newObject = EditorGUI.ObjectField(
                        position,
                        string.Empty,
                        cachedObject,
                        typeof(Object),
                        false);
                    break;
                case AssetType.GameObject:
                    newObject = EditorGUI.ObjectField(
                        position,
                        string.Empty,
                        cachedObject,
                        typeof(GameObject),
                        false);
                    break;
                case AssetType.Texture:
                    newObject = EditorGUI.ObjectField(
                        position,
                        string.Empty,
                        cachedObject,
                        typeof(Texture),
                        false);
                    break;
                case AssetType.Material:
                    newObject = EditorGUI.ObjectField(
                        position,
                        string.Empty,
                        cachedObject,
                        typeof(Material),
                        false);
                    break;
            }

            if (cachedObject != newObject)
            {
                if (newObject == null)
                {
                    _loadedObjectsCache.Remove(propertyPath);
                    property.FindPropertyRelative(PROPERTY_ASSET_PATH_NAME).stringValue = null;
                    property.FindPropertyRelative(PROPERTY_ASSET_TYPE).enumValueIndex = (int) AssetType.Object;
                }
                else
                {
                    _loadedObjectsCache[propertyPath] = newObject;
                    SerializePath(newObject, property);
                }
            }

            GUI.color = Color.white;

            EditorGUI.EndProperty();
        }

        #endregion

        #region Private Members

        private void SerializePath(Object prefabGameObject, SerializedProperty prefabProperty)
        {
            var path = AssetDatabase.GetAssetPath(prefabGameObject);
            bool isAssetIntoResourcesFolder = false;
            var assetType = AssetType.Object;

            var search = "/Resources/";
            var index = path.IndexOf(search, StringComparison.OrdinalIgnoreCase);

            var ext = Path.GetExtension(path);
            switch (ext)
            {
                case ".prefab":
                    assetType = AssetType.GameObject;
                    break;
                case ".mat":
                    assetType = AssetType.Material;
                    break;
                case ".png":
                case ".jpg":
                case ".jpeg":
                    assetType = AssetType.Texture;
                    break;
                default:
                    assetType = AssetType.Object;
                    break;
            }

            if (index > 0)
            {
                path = path.Substring(index + search.Length);
                path = Path.ChangeExtension(path, "");
                path = path.Substring(0, path.Length - 1);
                isAssetIntoResourcesFolder = true;
            }

            prefabProperty.FindPropertyRelative(PROPERTY_ASSET_PATH_NAME).stringValue = path;
            prefabProperty.FindPropertyRelative(PROPERTY_ASSET_INTO_RESOURCES_NAME).boolValue = isAssetIntoResourcesFolder;
            prefabProperty.FindPropertyRelative(PROPERTY_ASSET_TYPE).enumValueIndex = (int) assetType;
        }

        #endregion
    }
}
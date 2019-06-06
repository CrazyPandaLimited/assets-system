using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.AssetGraph
{
    [CreateAssetMenu(fileName = "AssetGraphUserPreferencesConfig", menuName = "Resource system/AssetGraphUserPreferencesConfig", order = 1)]
    public class UserPreference : ScriptableObject
    {
//        static readonly string kKEY_USERPREF_GRID = "UnityEngine.AssetGraph.UserPref.GridSize";
//        static readonly string kKEY_USERPREF_DEFAULTVERBOSELOG = "UnityEngine.AssetGraph.UserPref.DefaultVerboseLog";
//        static readonly string kKEY_USERPREF_DEFAULTASSETLOG = "UnityEngine.AssetGraph.UserPref.DefaultAssetLog";
//        static readonly string kKEY_USERPREF_DEFAULTCACHESROOTPATH = "UnityEngine.AssetGraph.UserPref.DefaultCachesPath";

        #region Load config

        private const string GRAPHICS_SETTINGS_FILE_NAME = "t:UserPreference AssetGraphUserPreferencesConfig";

        public static UserPreference LoadConfig()
        {
#if UNITY_EDITOR
            string path = null;
            foreach (var findAsset in AssetDatabase.FindAssets(GRAPHICS_SETTINGS_FILE_NAME))
            {
                if (AssetDatabase.GUIDToAssetPath(findAsset).Contains(".asset"))
                {
                    path = AssetDatabase.GUIDToAssetPath(findAsset);
                    break;
                }
            }

            return AssetDatabase.LoadAssetAtPath<UserPreference>(path);
#else
            throw new NullReferenceException();

#endif
        }

        #endregion

        public float _editorWindowGridSize;
        public bool _defaultVerboseLog;
        public bool _clearAssetLogOnBuild;
        public string _cachesRootPath;

        private static UserPreference _userPreferences;

        public static UserPreference Preferences
        {
            get
            {
                if (_userPreferences == null)
                {
                    _userPreferences = LoadConfig();
                }

                return _userPreferences;
            }
        }

#if UNITY_EDITOR
        [PreferenceItem("AssetGraph")]
        public static void PreferencesGUI()
        {
            Preferences._editorWindowGridSize = EditorGUILayout.FloatField("Graph editor grid size", Preferences._editorWindowGridSize);
            Preferences._defaultVerboseLog = EditorGUILayout.ToggleLeft("Default show verbose log", Preferences._defaultVerboseLog);

            using (new EditorGUILayout.HorizontalScope())
            {
                Preferences._cachesRootPath = EditorGUILayout.TextField("Caches Root folder", Preferences._cachesRootPath);

                if (GUILayout.Button("Select", GUILayout.Width(50f)))
                {
                    var folderSelected = EditorUtility.OpenFolderPanel("Select root folder for caches", string.Empty, "");
                    folderSelected = FilterPath(folderSelected);
                    if (!string.IsNullOrEmpty(folderSelected) && Preferences._cachesRootPath != folderSelected)
                    {
                        if (EditorUtility.DisplayDialog("Warning!", "Delete old cache folder?", "Yes", "No"))
                        {
                            Directory.Delete(Preferences._cachesRootPath, true);
                        }

                        Preferences._cachesRootPath = folderSelected;
                    }
                }
            }

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_userPreferences);
//                AssetDatabase.SaveAssets();
            }
        }

        private static string FilterPath(string folderSelected)
        {
            var projectPath = Directory.GetParent(Application.dataPath).ToString().Replace('\\', '/');
            var projectAssetsPath = Application.dataPath.Replace('\\', '/');

            var folderSelectedUnistyle = folderSelected.Replace('\\', '/');

            if (folderSelectedUnistyle.StartsWith(projectAssetsPath))
            {
                folderSelectedUnistyle = folderSelectedUnistyle.Substring(projectPath.Length);//it must starts with Assets
                if (folderSelectedUnistyle.StartsWith("/"))
                {
                    folderSelectedUnistyle = folderSelectedUnistyle.Substring(1);
                }
                return folderSelectedUnistyle;
            }
            else
            {
                EditorUtility.DisplayDialog("Wrong directory", "You must select directory inside project Assets folder", "OK");
                return null;
            }          
        }
#endif
    }
}
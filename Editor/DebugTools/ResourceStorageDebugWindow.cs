using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
    public class ResourceStorageDebugWindow : EditorWindow
    {
        [MenuItem("UnityCore/Resource system/Debug window")]
        static void Init()
        {
            ResourceStorageDebugWindow window = GetWindow<ResourceStorageDebugWindow>("Resource system debug");
            window.Show();
        }

        private const string GRAPHICS_SETTINGS_FILE_NAME = "RSDebugToolResources";

        private int resStorageIdToDraw;
        private Action _drawFunction;
        private DrawResourceSystemDebugGui systemDrawer;
        private ResourceSystemDebugToolResources _settings;

        private EditorSpriteAnimator _hipnotoad;

        private void Awake()
        {
            LoadGraphicsSettings();
            systemDrawer = new DrawResourceSystemDebugGui(_settings);
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            OnPlayModeStateChanged(Application.isPlaying ? PlayModeStateChange.EnteredPlayMode : PlayModeStateChange.EnteredEditMode);
            _hipnotoad = new EditorSpriteAnimator(_settings.EditorModeAnimationSprites);
        }

        private void LoadGraphicsSettings()
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

            _settings = AssetDatabase.LoadAssetAtPath<ResourceSystemDebugToolResources>(path);
        }

        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            _hipnotoad = null;
            Clear();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange newState)
        {
            #if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS_TEST_MODE
            _drawFunction = DrawPlayMode;
            return;
            #endif
            
            switch (newState)
            {
                case PlayModeStateChange.EnteredPlayMode:
                {
                    _drawFunction = DrawPlayMode;
                    break;
                }
                case PlayModeStateChange.ExitingPlayMode:
                {
                    Clear();
                    _drawFunction = DrawEditMode;
                    break;
                }
                case PlayModeStateChange.EnteredEditMode:
                {
                    _drawFunction = DrawEditMode;
                    break;
                }
            }
        }

        private void Update()
        {
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS_TEST_MODE
            Repaint();
            return;
#endif      
            if (_drawFunction != null)
            {
                Repaint();
            }
        }

        private void Clear()
        {
            resStorageIdToDraw = 0;
            systemDrawer = null;
        }

        private void OnGUI()
        {
            if (_drawFunction != null)
            {
                _drawFunction();
                return;
            }

            OnDestroy();
            Awake();
        }

        private void DrawPlayMode()
        {
            if (ResourceSystemLocator.ResourceStorageInstances == null || ResourceSystemLocator.ResourceStorageInstances.Count == 0)
            {
                return;
            }

            if (ResourceSystemLocator.ResourceStorageInstances.Count > 1)
            {
                GUILayout.Label(_settings.PlayModeSelectRSTitle, _settings.PlayModeSelectRSTitleStyle);
                string[] resNames = new string[ResourceSystemLocator.ResourceStorageInstances.Count];

                for (int i = 0; i < resNames.Length; i++)
                {
                    resNames[i] = String.Format(_settings.PlayModeSelectRSButtonTemplate,i);
                }

                var tmpRect = GUILayoutUtility.GetRect(position.width, _settings.PlayModeSelectContentButtonsHeight);
                GUI.Label(tmpRect, string.Empty, _settings.PlayModeSelectRSButtomStyle);
                tmpRect.x += _settings.PlayModeSelectRSButtomStyle.contentOffset.x;
                tmpRect.width -= _settings.PlayModeSelectRSButtomStyle.contentOffset.x;
                
                tmpRect.y += _settings.PlayModeSelectRSButtomStyle.contentOffset.y;
                tmpRect.height -= _settings.PlayModeSelectRSButtomStyle.contentOffset.y * 2 + _settings.PlayModeSelectRSButtomStyle.border.bottom;
                
                resStorageIdToDraw = GUI.SelectionGrid(tmpRect, resStorageIdToDraw, resNames, 3, _settings.PlayModeSelectRSButtonTemplateButtonStyle);
            }
            
            systemDrawer.DrawSystemGui(ResourceSystemLocator.ResourceStorageInstances[resStorageIdToDraw], position);
        }

        private void DrawEditMode()
        {
            GUI.Label(
                new Rect(position.width / 2f + _settings.EditorModeTextPositionOffset.x,
                    position.height / 2f + _settings.EditorModeTextPositionOffset.y,
                    _settings.EditorModeTextPositionOffset.width,
                    _settings.EditorModeTextPositionOffset.height),
                _settings.EditorModeRunOnlyRuntimeString, _settings.EditorModeRunOnlyRuntimeStringStyle);

            _hipnotoad.Draw(new Rect(position.width / 2f + _settings.EditorModeImagePositionOffset.x,
                position.height / 2f + _settings.EditorModeImagePositionOffset.y,
                +_settings.EditorModeImagePositionOffset.width,
                +_settings.EditorModeImagePositionOffset.height));
        }
    }
#endif
}
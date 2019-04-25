#if CRAZYPANDA_UNITYCORE_COROUTINE
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
    public class CoroutineManagerWindow : EditorWindow
    {
        private static GUIStyle _style;
        #region Public Members
        [ MenuItem( "UnityCore/Coroutines system/Coroutine Manager View" ) ]
        public static void ShowWindow()
        {
            var w = GetWindow< CoroutineManagerWindow >();
            w.titleContent = new GUIContent( "Coroutine Manager" );
            _style = new GUIStyle { richText = true };
        }

        public void OnGUI()
        {
            if( !Application.isPlaying )
            {
                GUILayout.Label( "Coroutine Editor Works Only In Game Mode" );
                return;
            }

            var coroutineManager = CoroutineManager.Instance;
            _style = _style ?? new GUIStyle { richText = true };
            if( coroutineManager == null )
            {
                GUILayout.Label( "CoroutineManager wasn't initialized yet" );
                return;
            }

            GUILayout.Space( 1 );
            EditorGUILayout.Separator();

            var coroutines = coroutineManager.Coroutines;
            GUILayout.Label( string.Format( "Running coroutines: {0}", coroutines.Count ) );
            EditorGUILayout.Separator();

            foreach( var coroutine in coroutines )
            {
                if( !coroutine.IsAlive )
                {
                    continue;
                }

                //var owner = coroutine.Target.UnityName();
                var owner = coroutine.Target.ToString();
                var state = coroutine.CoroutineProcessor.State;
                GUILayout.Label( string.Format( "{0}: <b><size=14>{1}</size></b>", owner, state ), _style );
            }

            EditorGUILayout.Separator();

            Repaint();
        }
        #endregion
    }
}

#endif

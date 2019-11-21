using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class DebugUIPromises
	{
		private bool _foldout;
		public void Draw( RequestToPromiseMap promiseMap )
		{
			if( promiseMap == null )
			{
				return;
			}
			_foldout = EditorGUILayout.Foldout( _foldout, $"Active promises" );

			if( !_foldout )
			{
				return;
			}

			foreach( var entry in promiseMap.AllPromises() )
			{
				EditorGUILayout.LabelField( $"      Id:{entry.Key} Status:{entry.Value.Status}" );
			}	
		}
	}
}

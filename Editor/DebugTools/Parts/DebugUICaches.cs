using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class DebugUICaches
	{
		private bool[ ] cachesFoldouts = new bool[ 1 ];
		private bool[ ] refcountCachesFoldouts = new bool[ 1 ];

		public void Draw( Dictionary< string, ICache > caches )
		{
			if( caches == null )
			{
				return;
			}

			if( cachesFoldouts.Length != caches.Count )
			{
				cachesFoldouts = new bool[ caches.Count ];
			}


			int i = 0;
			foreach( var cache in caches )
			{
				var col = GUI.color;
				GUI.color = Color.blue;
				cachesFoldouts[ i ] = EditorGUILayout.Foldout( cachesFoldouts[ i ], $"{cache.Key}");
				GUI.color = col;

				if( cachesFoldouts[ i ] )
				{
					DrawICacheUI( cache.Value );
				}

				i++;
			}
		}

		private void DrawICacheUI( ICache cache )
		{
			EditorGUILayout.LabelField( $"    Cached assets:{cache.GetAllAssetsNames().Count}" );
			foreach( var assetName in cache.GetAllAssetsNames() )
			{
				EditorGUILayout.LabelField( $"      |{assetName}" );
			}
		}

		public void Draw( Dictionary< string, ICacheControllerWithAssetReferences > refcountCacheControllers )
		{
			if( refcountCacheControllers == null )
			{
				return;
			}

			if( refcountCachesFoldouts.Length != refcountCacheControllers.Count )
			{
				refcountCachesFoldouts = new bool[ refcountCacheControllers.Count ];
			}


			int i = 0;
			foreach( var cache in refcountCacheControllers )
			{
				refcountCachesFoldouts[ i ] = EditorGUILayout.Foldout( refcountCachesFoldouts[ i ], $"{cache.Key}" );

				if( refcountCachesFoldouts[ i ] )
				{
					DrawRefcountCacheUI( cache.Value );
				}

				i++;
			}
		}

		private void DrawRefcountCacheUI( ICacheControllerWithAssetReferences cache )
		{
			EditorGUILayout.LabelField( $"    Cached assets:{cache.GetAllAssetsNames().Count}" );
			foreach( var assetName in cache.GetAllAssetsNames() )
			{
				var AllReferences = cache.GetReferencesByAssetName( assetName );

				EditorGUILayout.LabelField( $"      |{assetName}" );

				if( AllReferences.Count > 0 )
				{
					for( int i = 1; i < AllReferences.Count; i++ )
					{
						EditorGUILayout.LabelField( $"      |                                Reference: {AllReferences[ i ].ToString()}  hashCode:{AllReferences[ i ]?.GetHashCode()}" );
					}
				}
			}
		}
	}
}

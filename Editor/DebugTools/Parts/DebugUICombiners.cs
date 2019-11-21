using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using UnityEditor;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class DebugUICombiners
	{
		private bool[ ] combinersFoldouts = new bool[1];


		public void Draw(Dictionary< string, Dictionary< string, CombinedRequest > > combinersQueue)
		{
			if( combinersQueue == null )
			{
				return;
			}
            
			if( combinersFoldouts.Length != combinersQueue.Count )
			{
				combinersFoldouts = new bool[combinersQueue.Count];
			}


			int i = 0;
			foreach( var combiner in combinersQueue )
			{
				combinersFoldouts[ i ] = EditorGUILayout.Foldout( combinersFoldouts[ i ], combiner.Key );

				if( combinersFoldouts[ i ] )
				{
					DrawCombinerUI( combiner.Value );
				}
				i++;
			}
		}

		private void DrawCombinerUI( Dictionary< string, CombinedRequest > requests)
		{
			EditorGUILayout.LabelField( "    Waiting" );
			foreach( var waitRequest in requests)
			{
				EditorGUILayout.LabelField( $"      Id:{waitRequest.Key}" );
				EditorGUILayout.LabelField( $"          CombinedHeader:{waitRequest.Value.CombinedHeader.ToString()}" );
				EditorGUILayout.LabelField( $"          CombinedBody:{waitRequest.Value.CombinedBody.ToString()}" );
				EditorGUILayout.LabelField( $"          Cancel requested:{waitRequest.Value.CancellationToken.IsCancellationRequested}" );
				EditorGUILayout.LabelField( $"          Combined messages:{waitRequest.Value.SourceRequests.Count}" );
				foreach( var valueSourceRequest in waitRequest.Value.SourceRequests )
				{ 
				EditorGUILayout.LabelField( $"              Id:{valueSourceRequest.Key.Id}" );
				EditorGUILayout.LabelField( $"                  Header:{valueSourceRequest.Key.ToString()}" );
				EditorGUILayout.LabelField( $"                  Body:{valueSourceRequest.Value.ToString()}" );
				}
			}
		}
	}
}

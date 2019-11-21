using System.Collections.Generic;
using UnityEditor;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class DebugUIRequestsHistory
	{
		private bool _foldout;
		private Dictionary<string,bool> _requestsFoldouts = new Dictionary< string, bool >();
		public void Draw( RequestsHistoryInfo requestsHistoryInfo )
		{
			if( requestsHistoryInfo == null )
			{
				return;
			}
			_foldout = EditorGUILayout.Foldout( _foldout, $"Requests history" );

			if( !_foldout )
			{
				return;
			}

			foreach( var entry in requestsHistoryInfo.RequestsHistory )
			{
				if( !_requestsFoldouts.ContainsKey( entry.Key ) )
				{
					_requestsFoldouts.Add( entry.Key, false );
				}
				
				_requestsFoldouts[entry.Key] = EditorGUILayout.Foldout( _requestsFoldouts[entry.Key], $"      Id:{entry.Key} Name:{entry.Value.AssetName} Duration(s):{entry.Value.RequestDuration}" );

				if( _requestsFoldouts[ entry.Key ] )
				{
					foreach( var historyEntry in entry.Value.History )
					{
						EditorGUILayout.LabelField( $"          Time:{historyEntry.Timestamp.ToLongTimeString()} Out:{historyEntry.OutNodeName}  In:{historyEntry.InputNodeName}" );
					}
				}
			}	
		}
	}
}

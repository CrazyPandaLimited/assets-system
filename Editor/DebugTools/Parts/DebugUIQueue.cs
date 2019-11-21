using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using UnityEditor;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
    public class DebugUIQueue
    {

        private bool[ ] queuesFoldouts = new bool[1];


        public void Draw(Dictionary< string, RequestsQueue > requestsQueue)
        {
            if( requestsQueue == null )
            {
                return;
            }
            
            if( queuesFoldouts.Length != requestsQueue.Count )
            {
                queuesFoldouts = new bool[requestsQueue.Count];
            }


            int i = 0;
            foreach( var queue in requestsQueue )
            {
                queuesFoldouts[ i ] = EditorGUILayout.Foldout( queuesFoldouts[ i ], queue.Key );

                if( queuesFoldouts[ i ] )
                {
                    DrawQueueUI( queue.Value );
                }
                i++;
            }
        }

        private void DrawQueueUI( RequestsQueue requestsQueue )
        {
            EditorGUILayout.LabelField( "    Waiting" );
            foreach( var waitRequest in requestsQueue._waitingRequests)
            {
                EditorGUILayout.LabelField( $"      Id:{waitRequest.Header.Id}" );
                EditorGUILayout.LabelField( $"          Header:{waitRequest.Header.ToString()}" );
                EditorGUILayout.LabelField( $"          Body:{waitRequest.Body.ToString()}" );
            }
            EditorGUILayout.LabelField( "    Worked" );
            foreach( var workingRequest in requestsQueue._workingRequests)
            {
                EditorGUILayout.LabelField( $"      Id:{workingRequest.Header.Id}" );
                EditorGUILayout.LabelField( $"          Header:{workingRequest.Header.ToString()}" );
                EditorGUILayout.LabelField( $"          Body:{workingRequest.Body.ToString()}" );
            }
        }
    }
}

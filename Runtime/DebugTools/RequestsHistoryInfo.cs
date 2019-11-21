using System;
using System.Collections.Generic;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
	public class RequestsHistoryInfo
	{
		public Dictionary<string, RequestHistory> RequestsHistory = new Dictionary< string, RequestHistory >();
		
		private List< IFlowNode > AllNodes;
		private AssetsStorage storageNode;

		public RequestsHistoryInfo(AssetsStorage storageNode, List< IFlowNode > nodes)
		{
			AllNodes = new List< IFlowNode >();
			
			storageNode.OnMessageSended += StorageNodeOnMessageSended;
			
			foreach( var flowNode in nodes )
			{
				if( flowNode is AssetsStorage )
				{
					continue;
				}
                
				AllNodes.Add( flowNode );

				if( flowNode is IInputNode )
				{
					((IInputNode)flowNode).OnMessageConsumed += OnMessageConsumed;
				}
                
				if( flowNode is IOutputNode< IMessageBody > )
				{
					((IOutputNode< IMessageBody >)flowNode).OnMessageSended += OnMessageSended;
				}
			}
		}

		private void OnMessageConsumed( object sender, MessageConsumedEventArgs e )
		{
			if( !RequestsHistory.ContainsKey( e.Header.Id ) )
			{
				//Debug.LogError( $"Request not registered {e.Header.Id}" );
				return;
			}
			
			RequestsHistory[e.Header.Id].RegisterMessageReachInputNode(sender.ToString());
		}

		private void OnMessageSended( object sender, MessageSendedOutEventArgs e )
		{
			if( !RequestsHistory.ContainsKey( e.Header.Id ) )
			{
				//Debug.LogError( $"Request not registered {e.Header.Id}" );
				return;
			}
			
			RequestsHistory[e.Header.Id].RegisterMessageReachNodeOut(sender.ToString(), e.Header,e.Body);
		}

		private void StorageNodeOnMessageSended( object sender, MessageSendedOutEventArgs e )
		{
			if( RequestsHistory.ContainsKey( e.Header.Id ) )
			{
				Debug.LogError( $"Dupliceted header id:{e.Header.Id}" );
				return;
			}
			RequestsHistory.Add( e.Header.Id, new RequestHistory(e.Header, e.Body) );
		}
	}
	
	public class RequestHistory
	{
		public string id { get; private set; }
		public string AssetName{ get; private set; }
		public float RequestDuration{ get; private set; }
		public List< RequestHistoryEntry > History = new List< RequestHistoryEntry >();

		public RequestHistory( MessageHeader header, object body )
		{
			id = header.Id;
			AssetName = body is UrlLoadingRequest ? ( ( UrlLoadingRequest ) body ).Url : string.Empty;
			History.Add( new RequestHistoryEntry()
			{
				Timestamp = DateTime.Now,
				OutNodeName = "AssetsStorage",
				IsCanceled = header.CancellationToken.IsCancellationRequested,
				MetaDataInfo = header.MetaData.ToString(),
				BodyInfo = body.ToString(),
				Exceptions = header.Exceptions?.Flatten().ToString()
			} );
		}


		public void RegisterMessageReachInputNode(string nodeType)
		{
			if(!string.IsNullOrEmpty(   History[History.Count-1].InputNodeName)  )
			{
				Debug.LogError( $"Multiple input for message prevInput:{History[History.Count-1].InputNodeName} newInput:{nodeType}" );
				return;
			}
			History[ History.Count - 1 ].InputNodeName = nodeType;
			RequestDuration = ( History[ History.Count-1 ].Timestamp - History[ 0 ].Timestamp ).Seconds;
		}

		public void RegisterMessageReachNodeOut(string outNodeName, MessageHeader header, object body)
		{
			History.Add( new RequestHistoryEntry()
			{
				Timestamp = DateTime.Now,
				OutNodeName = outNodeName,
				IsCanceled = header.CancellationToken.IsCancellationRequested,
				MetaDataInfo = header.MetaData.ToString(),
				BodyInfo = body.ToString(),
				Exceptions = header.Exceptions?.Flatten().ToString()
			} );
		}
	}

	public class RequestHistoryEntry
	{
		public DateTime Timestamp;
		public string OutNodeName;
		public string InputNodeName;
		public bool IsCanceled;
		public string MetaDataInfo;
		public string BodyInfo;
		public string Exceptions;
	}
}

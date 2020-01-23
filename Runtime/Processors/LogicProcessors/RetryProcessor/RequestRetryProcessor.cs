using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestRetryProcessor< T > : AbstractRequestInputOutputProcessor< AssetLoadingRequest< T >, UrlLoadingRequest >
    {
        #region Constants
        public const string RETRY_METADATA_KEY = "RetryProcessorMetaData";
        #endregion

        #region Protected Fields
        /// <summary>
        /// key: retry idx
        /// value: seconds before next try
        /// </summary>
        protected Dictionary< int, float > _retryMap;
        protected Func< MessageHeader, AssetLoadingRequest< T >, bool > _needRetryChecker;
        protected NodeOutputConnection< UrlLoadingRequest > _retryConnection;
        protected NodeOutputConnection< AssetLoadingRequest< T > > _outputConnection;
        protected NodeOutputConnection< UrlLoadingRequest > _allRetrysFailedConnection;
        #endregion

        #region Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryMap"></param>
        /// <param name="coroutineManager"></param>
        /// <param name="needRetryChecker">return true if need retry</param>
        public RequestRetryProcessor( Dictionary< int, float > retryMap, Func< MessageHeader, AssetLoadingRequest< T >, bool > needRetryChecker )
        {
            _retryMap = retryMap ?? throw new ArgumentNullException( nameof(retryMap) );
            _needRetryChecker = needRetryChecker ?? throw new ArgumentNullException( nameof(needRetryChecker) );
        }
        #endregion

        #region Public Members
        public void RegisterRetryConnection( IInputNode< UrlLoadingRequest > connectedNode )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( connectedNode );
            _retryConnection = connection;
            RegisterConnection( connection );
        }

        public void RegisterAllRetrysFailedConnection( IInputNode< UrlLoadingRequest > connectedNode )
        {
            var connection = new NodeOutputConnection< UrlLoadingRequest >( connectedNode );
            _allRetrysFailedConnection = connection;
            RegisterConnection( connection );
        }

        public void RegisterOutputConnection( IInputNode< AssetLoadingRequest< T > > connectedNode )
        {
            var connection = new NodeOutputConnection< AssetLoadingRequest< T > >( connectedNode );
            _outputConnection = connection;
            RegisterConnection( connection );
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( !_needRetryChecker( header, body ) )
            {
                _outputConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            int nextRetryIdx = 0;
            if( header.MetaData.IsMetaExist( RETRY_METADATA_KEY ) )
            {
                nextRetryIdx = header.MetaData.GetMeta< int >( RETRY_METADATA_KEY );
            }

            nextRetryIdx++;

            if( !_retryMap.ContainsKey( nextRetryIdx ) )
            {
                header.AddException( new AllRequestRetrysFallException() );
                _allRetrysFailedConnection.ProcessMessage( header, body );
                return FlowMessageStatus.Accepted;
            }

            NextTryWait( header, body, nextRetryIdx );
            return FlowMessageStatus.Accepted;
        }
        #endregion

        #region Private Members
        private async IPandaTask NextTryWait( MessageHeader header, AssetLoadingRequest< T > body, int nextRetryIdx )
        {
            await PandaTasksUtilitys.Delay( TimeSpan.FromSeconds( _retryMap[ nextRetryIdx ] ) );
            
            if( header.CancellationToken.IsCancellationRequested )
            {
                return;
            }

            header.MetaData.SetMeta( RETRY_METADATA_KEY, nextRetryIdx, true );
            _retryConnection.ProcessMessage( header, new UrlLoadingRequest( body ) );
        }
        #endregion
    }
}

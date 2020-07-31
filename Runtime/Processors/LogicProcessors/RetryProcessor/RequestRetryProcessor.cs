using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestRetryProcessor< T > : AbstractRequestInputOutputProcessor< AssetLoadingRequest< T >, AssetLoadingRequest< T > >
    {
        public const string RETRY_METADATA_KEY = "RetryProcessorMetaData";

        /// <summary>
        /// key: retry idx
        /// value: seconds before next try
        /// </summary>
        protected Dictionary< int, float > _retryMap;
        protected Func< MessageHeader, AssetLoadingRequest< T >, bool > _needRetryChecker;

        private BaseOutput< UrlLoadingRequest > _retryConnection = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );
        private BaseOutput< UrlLoadingRequest > _allRetrysFailedConnection = new BaseOutput< UrlLoadingRequest >( OutputHandlingType.Optional );

        public IOutputNode< UrlLoadingRequest > RetryOutput => _retryConnection;
        public IOutputNode< UrlLoadingRequest > AllRetrysFailedOutput => _allRetrysFailedConnection;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="retryMap"></param>
        /// <param name="coroutineManager"></param>
        /// <param name="needRetryChecker">return true if need retry</param>
        public RequestRetryProcessor( Dictionary< int, float > retryMap, Func< MessageHeader, AssetLoadingRequest< T >, bool > needRetryChecker )
        {
            _retryMap = retryMap ?? throw new ArgumentNullException( nameof( retryMap ) );
            _needRetryChecker = needRetryChecker ?? throw new ArgumentNullException( nameof( needRetryChecker ) );
        }

        protected override void InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( !_needRetryChecker( header, body ) )
            {
                SendOutput( header, body );
                return;
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
                return;
            }

            NextTryWait( header, body, nextRetryIdx );
        }

        private void NextTryWait( MessageHeader header, AssetLoadingRequest< T > body, int nextRetryIdx )
        {
            PandaTasksUtilitys.Delay( TimeSpan.FromSeconds( _retryMap[ nextRetryIdx ] ) )
                .Done( () =>
                {
                    if( header.CancellationToken.IsCancellationRequested )
                    {
                        return;
                    }

                    header.MetaData.SetMeta( RETRY_METADATA_KEY, nextRetryIdx, true );
                    _retryConnection.ProcessMessage( header, new UrlLoadingRequest( body ) );
                } );
        }
    }
}

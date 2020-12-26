﻿using System;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class BaseAssetsStorage : IAssetsStorage
    {
        private bool _isDisposed;
        private IInputNode<UrlLoadingRequest> _linkedNode;

        protected readonly RequestToPromiseMap _requestToPromiseMap;
        
        public event EventHandler< MessageSentOutEventArgs > OnMessageSended;
        public event Action< FlowNodeStatusChangedEventArgs > OnNodeStatusChanged;

        protected BaseAssetsStorage()
        {
            _requestToPromiseMap = new RequestToPromiseMap();
        }

        protected BaseAssetsStorage( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
        }

        /// <summary>
        /// Sync load Asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loaded asset</returns>
        public AssetType LoadAssetSync< AssetType >( string url )
        {
            return LoadAssetSync< AssetType >( url, new MetaData() );
        }

        /// <summary>
        /// Sync load Asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <param name="metaData">Any additional data</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loaded asset</returns>
        /// <exception cref="SyncLoadException">Thrown when graph needs async execution</exception>
        public AssetType LoadAssetSync< AssetType >( string url, MetaData metaData )
        {
            metaData.SetFlag( MetaDataReservedKeys.SYNC_REQUEST_FLAG );

            var promice = LoadAssetAsync< AssetType >( url, metaData );

            if( promice.Status == PandaTaskStatus.Resolved )
            {
                return promice.Result;
            }

            if( promice.Status == PandaTaskStatus.Rejected )
            {
                throw new SyncLoadException( url, metaData, promice.Error );
            }

            throw new SyncLoadException( url, metaData );
        }

        /// <summary>
        /// Async load asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loading task </returns>
        public IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url )
        {
            return LoadAssetAsync< AssetType >( url, new MetaData(), CancellationToken.None, null );
        }

        /// <summary>
        /// Async load asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <param name="metaData">Any additional data</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loading task </returns>
        public IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url, MetaData metaData )
        {
            return LoadAssetAsync< AssetType >( url, metaData, CancellationToken.None, null );
        }

        /// <summary>
        /// Async load asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <param name="metaData">Any additional data</param>
        /// <param name="tracker">Loading progress tracker</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loading task </returns>
        public IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url, MetaData metaData, IProgressTracker< float > tracker )
        {
            return LoadAssetAsync< AssetType >( url, metaData, CancellationToken.None, tracker );
        }

        /// <summary>
        /// Async load asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <param name="metaData">Any additional data</param>
        /// <param name="tocken">Tocken to cancel request possibility</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loading task </returns>
        public IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url, MetaData metaData, CancellationToken tocken )
        {
            return LoadAssetAsync< AssetType >( url, metaData, tocken, null );
        }

        /// <summary>
        /// Async load asset
        /// </summary>
        /// <param name="url">Asset URL</param>
        /// <param name="metaData">Any additional data</param>
        /// <param name="tocken">Tocken to cancel request possibility</param>
        /// <param name="tracker">Loading progress tracker</param>
        /// <typeparam name="AssetType"></typeparam>
        /// <returns>Loading task </returns>
        public IPandaTask< AssetType > LoadAssetAsync< AssetType >( string url, MetaData metaData, CancellationToken tocken, IProgressTracker< float > tracker )
        {
            CheckDisposed();
            if( string.IsNullOrEmpty( url ) )
            {
                throw new AssetUrlEmptyException( "Asset url can not be null or empty" );
            }

            if( _linkedNode == null )
            {
                throw new InvalidOperationException( $"{nameof( BaseAssetsStorage )} is not fully built" );
            }

            if( tocken.IsCancellationRequested )
            {
                return PandaTasksUtilitys.GetCanceledTask< AssetType >();
            }
            
            var header = new MessageHeader( metaData, tocken );
            var body = new UrlLoadingRequest( url, typeof( AssetType ), tracker == null ? new ProgressTracker< float >() : tracker );

            var resultTask = new PandaTaskCompletionSource< AssetType >();
            var internalTask = new PandaTaskCompletionSource< object >();

            internalTask.ResultTask
                .Done( o => { resultTask.SetValue( ( AssetType ) o ); } )
                .Fail( exception => { resultTask.SetError( exception ); } );

            tocken.RegisterIfCanBeCanceled( () =>
            {
                if( !_requestToPromiseMap.Has( header.Id ) )
                {
                    return;
                }

                _requestToPromiseMap.Get( header.Id ).SetError( new OperationCanceledException() );
            } );

            _requestToPromiseMap.Add( header.Id, internalTask );

            try
            {
                OnMessageSended?.Invoke( this, new MessageSentOutEventArgs( header, body ) );
                _linkedNode.ProcessMessage( header, body );
            }
            catch( Exception exception )
            {
                if( resultTask.ResultTask.Status == PandaTaskStatus.Pending )
                {
                    resultTask.SetError( exception );
                }
            }

            return resultTask.ResultTask;
        }

        /// <summary>
        /// Dispose system
        /// </summary>
        public void Dispose()
        {
            _isDisposed = true;
        }

        protected void SubscribeToNodeStatusChanged( IFlowNode node )
        {
            node.OnStatusChanged += NodeStatusChanged;
        }

        protected void LinkTo( IInputNode< UrlLoadingRequest > input )
        {
            _linkedNode = input;
        }
        
        private void NodeStatusChanged(object sender, FlowNodeStatusChangedEventArgs eventArgs )
        {
            OnNodeStatusChanged?.Invoke( eventArgs );
        }
        
        private void CheckDisposed()
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( "AssetsSystem is disposed" );
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetsStorage : IOutputNode< UrlLoadingRequest >, IAssetsStorage
    {
        #region Protected Fields
        protected NodeOutputConnection< UrlLoadingRequest > _outputConnection;
        #endregion

        #region Private Fields
        private bool _isDisposed;
        private RequestToPromiseMap _requestToPromiseMap;
        #endregion

        #region Events
        public event EventHandler< MessageSendedOutEventArgs > OnMessageSended;
        #endregion

        #region Constructors
        public AssetsStorage( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
            _isDisposed = false;
        }
        #endregion

        #region Public Members
        public void RegisterOutConnection( IInputNode< UrlLoadingRequest > outputConnection )
        {
            _outputConnection = new NodeOutputConnection< UrlLoadingRequest >( outputConnection );
        }

        public IEnumerable< IBaseOutputConnection > GetOutputs()
        {
            yield return _outputConnection;
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
                throw new SyncLoadException( $"Something happen, see inner exception InputInfo: url:{url} {metaData.ToString()}", promice.Error );
            }

            throw new SyncLoadException( $"Something in graph has async operation!!! InputInfo: url:{url} {metaData.ToString()}" );
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
            var resultTask = new PandaTaskCompletionSource< AssetType >();
            try
            {
                CheckDisposed();
                if( string.IsNullOrEmpty( url ) )
                {
                    throw new AssetUrlException( "Asset url can not be null or empty" );
                }

                var internalMetaData = metaData;
                if( metaData == null )
                {
                    internalMetaData = new MetaData();
                }

                var header = new MessageHeader( metaData, tocken );
                var body = new UrlLoadingRequest( url, typeof( AssetType ), tracker == null ? new ProgressTracker< float >() : tracker );

                var internalTask = new PandaTaskCompletionSource< object >();

                internalTask.ResultTask.Done( o => { resultTask.SetValue( ( AssetType ) o ); } ).Fail( exception => { resultTask.SetError( exception ); } );

                tocken.Register( () =>
                {
                    if( !_requestToPromiseMap.Has( header.Id ) )
                    {
                        return;
                    }
                    _requestToPromiseMap.Get( header.Id ).SetError( new OperationCanceledException() );
                } );

                _requestToPromiseMap.Add( header.Id, internalTask );
                OnMessageSended?.Invoke( this, new MessageSendedOutEventArgs( header, body ) );
                _outputConnection.ProcessMessage( header, body );
            }
            catch( Exception exception )
            {
                if( resultTask.ResultTask.Status == PandaTaskStatus.Pending )
                {
                    resultTask.SetError( exception );
                }
                else if( resultTask.ResultTask.Status == PandaTaskStatus.Resolved )
                {
                    throw new Exception( $"Task completed, but catch other exception InputInfo: url:{url} {metaData.ToString()}", exception );
                }
                else
                {
                    throw new Exception( $"Task rejected with {resultTask.ResultTask.Error} and catched other exception InputInfo: url:{url} {metaData.ToString()}", exception );
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
        #endregion

        #region Private Members
        private void CheckDisposed()
        {
            if( _isDisposed )
            {
                throw new ObjectDisposedException( "AssetsSystem is disposed" );
            }
        }
        #endregion
    }
}

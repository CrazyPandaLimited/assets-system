using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.CoroutineSystem;
using UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class DefaultBuilder
    {
        #region Protected Fields
        protected ICoroutineManager _coroutineManager;
        protected RequestsQueue _requestsQueue;
        protected RequestToPromiseMap _promiseMap;
        #endregion

        #region Private Fields
        private List< IFlowNode > _allProcessors;
        #endregion

        #region Properties
        //Caches
        public AssetsWithRefcountCacheController OtherAssetsCache { get; private set; }
        public AssetsFromBundlesWithRefcountCacheController AssetsFromBundlesCache { get; private set; }
        public BundlesCacheWithRefcountCacheController BundlesCache { get; private set; }

        //Manifests
        public AssetsManifest< AssetInfo > AssetsManifestFromResourcesFolder { get; private set; }
        public AssetBundleManifest AssetBundleManifest { get; private set; }

        public AssetsStorage AssetsStorage { get; private set; }
        #endregion

        #region Constructors
        public DefaultBuilder( ICoroutineManager coroutineManager, int maxWorkingRequests )
        {
            _coroutineManager = coroutineManager ?? throw new ArgumentNullException( $"{nameof(coroutineManager)} can not be null!" );

            _requestsQueue = new RequestsQueue( maxWorkingRequests );

            AssetsManifestFromResourcesFolder = new AssetsManifest< AssetInfo >();

            OtherAssetsCache = new AssetsWithRefcountCacheController();
            _promiseMap = new RequestToPromiseMap();

            AssetBundleManifest = new AssetBundleManifest();
            BundlesCache = new BundlesCacheWithRefcountCacheController();
            AssetsFromBundlesCache = new AssetsFromBundlesWithRefcountCacheController( AssetBundleManifest, BundlesCache );

            AssetsStorage = new AssetsStorage( _promiseMap );
            _allProcessors = new List< IFlowNode >();
        }
        #endregion

        #region Public Members
        /// <summary>
        ///Always should be last tree part because we don't have instruments to check contains asset in Resources folder or not
        /// </summary>
        /// <returns></returns>
        public CheckAssetInCacheWithRefcountProcessor BuildLoadFromResourceFolderTree()
        {
            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( OtherAssetsCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( getFromCache );

            var loader = new ResorcesFolderLoadProcessor( _coroutineManager );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            cacheChecker.RegisterExistCacheOutConnection( getFromCache );
            getFromCache.RegisterDefaultConnection( finishNode );
            getFromCache.RegisterExceptionConnection( finishNodeException );

            cacheChecker.RegisterNotExistCacheOutConnection( loader );
            loader.RegisterDefaultConnection( addToCache );
            loader.RegisterExceptionConnection( finishNodeException );
            addToCache.RegisterDefaultConnection( finishNode );
            addToCache.RegisterExceptionConnection( finishNodeException );

            return cacheChecker;
        }


        public ManifestCheckerProcessor BuildLoadFromResourceFolderWithManifestTree()
        {
            var manifestChecker = new ManifestCheckerProcessor( AssetsManifestFromResourcesFolder );
            _allProcessors.Add( manifestChecker );

            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( OtherAssetsCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( getFromCache );

            var loader = new ResorcesFolderLoadProcessor( _coroutineManager );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            manifestChecker.RegisterExistOutConnection( cacheChecker );
            cacheChecker.RegisterExistCacheOutConnection( getFromCache );
            getFromCache.RegisterDefaultConnection( finishNode );
            getFromCache.RegisterExceptionConnection( finishNodeException );


            cacheChecker.RegisterNotExistCacheOutConnection( loader );
            loader.RegisterDefaultConnection( addToCache );
            loader.RegisterExceptionConnection( finishNodeException );
            addToCache.RegisterDefaultConnection( finishNode );
            addToCache.RegisterExceptionConnection( finishNodeException );

            return manifestChecker;
        }

        public ConditionBasedProcessor< TConditionNodeBodyType > BuildLoadFromWebRequestTree< TConditionNodeBodyType, UAssetType >() where TConditionNodeBodyType : UrlLoadingRequest
        {
            var isWebAssetChecker = new ConditionBasedProcessor< TConditionNodeBodyType >( ( url,exception, data ) => { return data.Url.Contains( "http://" ) || data.Url.Contains( "https://" ); } );
            _allProcessors.Add( isWebAssetChecker );

            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( OtherAssetsCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< UAssetType >( OtherAssetsCache );
            _allProcessors.Add( getFromCache );


            var dataCreators = new List< IAssetDataCreator > { new StringDataCreator(), new TextureDataCreator() };

            var loader = new WebRequestLoadProcessor< UAssetType >( _coroutineManager, dataCreators );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< UAssetType >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var queue = new RequestsQueueProcessor< UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queue );

            var queueEndPoint = new RequestsQueueEndPoint< AssetLoadingRequest< UAssetType > >( _requestsQueue );
            _allProcessors.Add( queueEndPoint );

            var conbinedREquestDict = new Dictionary< string, CombinedRequest >();
            var combiner = new RequestsCombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( combiner );

            var unCombiner = new RequestsUncombinerProcessor< UAssetType >( conbinedREquestDict );
            _allProcessors.Add( unCombiner );
            
            var unCombinerForExceptions = new RequestsUncombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( unCombinerForExceptions );
            
            var queueEndPointForExceptions = new RequestsQueueEndPoint<UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queueEndPointForExceptions );

            var finishNode = new AssetLoadingRequestEndPointProcessor< UAssetType >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            isWebAssetChecker.RegisterConditionPassedOutConnection( cacheChecker );

            cacheChecker.RegisterExistCacheOutConnection( getFromCache );
            getFromCache.RegisterDefaultConnection( finishNode );
            getFromCache.RegisterExceptionConnection( finishNodeException );

            cacheChecker.RegisterNotExistCacheOutConnection( combiner );
            combiner.RegisterDefaultConnection( queue );
            queue.RegisterDefaultConnection( loader );
            loader.RegisterDefaultConnection( queueEndPoint );
            queueEndPoint.RegisterDefaultConnection( unCombiner );
            unCombiner.RegisterDefaultConnection( addToCache );
            
            loader.RegisterExceptionConnection( queueEndPointForExceptions  );
            queueEndPointForExceptions.RegisterDefaultConnection( unCombinerForExceptions );
            unCombinerForExceptions.RegisterDefaultConnection( finishNodeException );
            
            addToCache.RegisterDefaultConnection( finishNode );
            addToCache.RegisterExceptionConnection( finishNodeException );

            return isWebAssetChecker;
        }

        public AssetBundleManifestCheckerProcessor BuildLoadBundlesFromWebRequestTree( string serverUrl )
        {
            var manifestChecker = new AssetBundleManifestCheckerProcessor( AssetBundleManifest, true, false );
            _allProcessors.Add( manifestChecker );

            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( BundlesCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< AssetBundle >( BundlesCache );
            _allProcessors.Add( getFromCache );

            var loader = new BundlesFromWebRequestLoadProcessor( serverUrl, AssetBundleManifest, _coroutineManager );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< AssetBundle >( BundlesCache );
            _allProcessors.Add( addToCache );

            var queue = new RequestsQueueProcessor< UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queue );

            var queueEndPoint = new RequestsQueueEndPoint< AssetLoadingRequest< AssetBundle > >( _requestsQueue );
            _allProcessors.Add( queueEndPoint );

            var conbinedREquestDict = new Dictionary< string, CombinedRequest >();

            var combiner = new RequestsCombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( combiner );

            var unCombiner = new RequestsUncombinerProcessor< AssetBundle >( conbinedREquestDict );
            _allProcessors.Add( unCombiner );

            var finishNode = new AssetLoadingRequestEndPointProcessor< AssetBundle >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var unCombinerForExceptions = new RequestsUncombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( unCombinerForExceptions );
            
            var queueEndPointForExceptions = new RequestsQueueEndPoint<UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queueEndPointForExceptions );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );


            manifestChecker.RegisterExistOutConnection( cacheChecker );

            cacheChecker.RegisterExistCacheOutConnection( getFromCache );
            getFromCache.RegisterDefaultConnection( finishNode );
            getFromCache.RegisterExceptionConnection( finishNodeException );

            cacheChecker.RegisterNotExistCacheOutConnection( combiner );

            combiner.RegisterDefaultConnection( queue );
            queue.RegisterDefaultConnection( loader );
            loader.RegisterDefaultConnection( queueEndPoint );
            queueEndPoint.RegisterDefaultConnection( unCombiner );
            unCombiner.RegisterDefaultConnection( addToCache );
            
            loader.RegisterExceptionConnection( queueEndPointForExceptions );
            queueEndPointForExceptions.RegisterDefaultConnection( unCombinerForExceptions );
            unCombinerForExceptions.RegisterDefaultConnection( finishNodeException );
            
            addToCache.RegisterDefaultConnection( finishNode );
            addToCache.RegisterExceptionConnection( finishNodeException );

            return manifestChecker;
        }

        public AssetBundleManifestCheckerProcessor BuildLoadAssetFromBundleTree()
        {
            var manifestChecker = new AssetBundleManifestCheckerProcessor( AssetBundleManifest, false, true );
            _allProcessors.Add( manifestChecker );

            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( AssetsFromBundlesCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< Object >( AssetsFromBundlesCache );
            _allProcessors.Add( getFromCache );

            var loader = new AssetFromBundleLoadProcessor(AssetsStorage, AssetBundleManifest, BundlesCache, _coroutineManager );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( AssetsFromBundlesCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            manifestChecker.RegisterExistOutConnection( cacheChecker );
            cacheChecker.RegisterExistCacheOutConnection( getFromCache );
            getFromCache.RegisterDefaultConnection( finishNode );
            getFromCache.RegisterExceptionConnection( finishNodeException );

            cacheChecker.RegisterNotExistCacheOutConnection( loader );
            loader.RegisterDefaultConnection( addToCache );
            loader.RegisterExceptionConnection( finishNodeException );
            addToCache.RegisterDefaultConnection( finishNode );
            addToCache.RegisterExceptionConnection( finishNodeException );
            
            return manifestChecker;
        }

        public void AddExceptionsHandlingForAllNodes( params IFlowNode[ ] additionalNodes )
        {
            foreach( var flowNode in _allProcessors )
            {
                flowNode.OnStatusChanged += ( sender, args ) =>
                {
                    if( args.NewStatus == FlowNodeStatus.Failed )
                    {
                        var exception = new AggregateException( $"Exception in node {args.Node.GetType()}", args.Node.Exception );
                        Debug.LogException( exception );
                        _promiseMap.Get( args.Header.Id ).SetError( exception );
                    }
                };
            }

            AssetsStorage.OnStatusChanged += ( sender, args ) =>
            {
                if( args.NewStatus == FlowNodeStatus.Failed )
                {
                    var exception = new AggregateException( $"Exception in node {args.Node.GetType()} headerId:{args.Header?.Id} headerMeta:{args.Header?.MetaData.ToString()} isCanceled:{args.Header?.CancellationToken.IsCancellationRequested}", args.Node.Exception );
                    Debug.LogException( exception );
                    _promiseMap.Get( args.Header.Id ).SetError( exception );
                }
            };

            if( additionalNodes == null )
            {
                return;
            }

            foreach( var additionalNode in additionalNodes )
            {
                additionalNode.OnStatusChanged += ( sender, args ) =>
                {
                    if( args.NewStatus == FlowNodeStatus.Failed )
                    {
                        var exception = new AggregateException( $"Exception in node {args.Node.GetType()} headerId:{args.Header?.Id} headerMeta:{args.Header?.MetaData.ToString()} isCanceled:{args.Header?.CancellationToken.IsCancellationRequested}", args.Node.Exception );
                        Debug.LogException( exception );
                        _promiseMap.Get( args.Header.Id ).SetError( exception );
                    }
                };
            }
        }

        public AssetLoadingRequestEndPointProcessor< BodyType > GetNewAssetLoadingRequestEndpoint< BodyType >()
        {
            var processor = new AssetLoadingRequestEndPointProcessor< BodyType >( _promiseMap );
            _allProcessors.Add( processor );
            return processor;
        }

        public UrlRequestEndPointProcessor GetNewUrlRequestEndpoint()
        {
            var processor = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( processor );
            return processor;
        }
        #endregion
    }
}

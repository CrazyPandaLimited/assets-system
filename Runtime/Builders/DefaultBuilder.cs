using System;
using System.Collections.Generic;
using System.Linq;
using CrazyPanda.UnityCore.AssetsSystem.Caching;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class DefaultBuilder
    {
        #region Protected Fields
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
        public DefaultBuilder( int maxWorkingRequests )
        {
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

            var loader = new ResorcesFolderLoadProcessor( );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
            getFromCache.DefaultOutput.LinkTo( finishNode );
            getFromCache.ExceptionOutput.LinkTo( finishNodeException );

            cacheChecker.NotExistInCacheOutput.LinkTo( loader );
            loader.DefaultOutput.LinkTo( addToCache );
            loader.ExceptionOutput.LinkTo( finishNodeException );
            addToCache.DefaultOutput.LinkTo( finishNode );
            addToCache.ExceptionOutput.LinkTo( finishNodeException );

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

            var loader = new ResorcesFolderLoadProcessor();
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            manifestChecker.ExistOutput.LinkTo( cacheChecker );
            cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
            getFromCache.DefaultOutput.LinkTo( finishNode );
            getFromCache.ExceptionOutput.LinkTo( finishNodeException );

            cacheChecker.ExistInCacheOutput.LinkTo( loader );
            loader.DefaultOutput.LinkTo( addToCache );
            loader.ExceptionOutput.LinkTo( finishNodeException );
            addToCache.DefaultOutput.LinkTo( finishNode );
            addToCache.ExceptionOutput.LinkTo( finishNodeException );

            return manifestChecker;
        }

        public ConditionBasedProcessor< TConditionNodeBodyType > BuildLoadFromWebRequestTree< TConditionNodeBodyType, TAssetType >() where TConditionNodeBodyType : UrlLoadingRequest
        {
            var isWebAssetChecker = new ConditionBasedProcessor< TConditionNodeBodyType >( ( url,exception, data ) => { return data.Url.Contains( "http://" ) || data.Url.Contains( "https://" ); } );
            _allProcessors.Add( isWebAssetChecker );

            var cacheChecker = new CheckAssetInCacheWithRefcountProcessor( OtherAssetsCache );
            _allProcessors.Add( cacheChecker );

            var getFromCache = new GetAssetFromCacheWithRefcountProcessor< TAssetType >( OtherAssetsCache );
            _allProcessors.Add( getFromCache );


            var dataCreators = new List< IAssetDataCreator > { new StringDataCreator(), new TextureDataCreator() };

            var loader = new WebRequestLoadProcessor< TAssetType >( dataCreators );
            _allProcessors.Add( loader );

            var addToCache = new AddAssetToCacheWithRefcountProcessor< TAssetType >( OtherAssetsCache );
            _allProcessors.Add( addToCache );

            var queue = new RequestsQueueProcessor< UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queue );

            var queueEndPoint = new RequestsQueueEndPoint< AssetLoadingRequest< TAssetType > >( _requestsQueue );
            _allProcessors.Add( queueEndPoint );

            var conbinedREquestDict = new Dictionary< string, CombinedRequest >();
            var combiner = new RequestsCombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( combiner );

            var unCombiner = new RequestsUncombinerProcessor< TAssetType >( conbinedREquestDict );
            _allProcessors.Add( unCombiner );
            
            var unCombinerForExceptions = new RequestsUncombinerProcessor( conbinedREquestDict );
            _allProcessors.Add( unCombinerForExceptions );
            
            var queueEndPointForExceptions = new RequestsQueueEndPoint<UrlLoadingRequest >( _requestsQueue );
            _allProcessors.Add( queueEndPointForExceptions );

            var finishNode = new AssetLoadingRequestEndPointProcessor< TAssetType >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            isWebAssetChecker.PassedOutput.LinkTo( cacheChecker );

            cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
            getFromCache.DefaultOutput.LinkTo( finishNode );
            getFromCache.ExceptionOutput.LinkTo( finishNodeException );

            cacheChecker.NotExistInCacheOutput.LinkTo( combiner );
            combiner.DefaultOutput.LinkTo( queue );
            queue.DefaultOutput.LinkTo( loader );
            loader.DefaultOutput.LinkTo( queueEndPoint );
            queueEndPoint.DefaultOutput.LinkTo( unCombiner );
            unCombiner.DefaultOutput.LinkTo( addToCache );
            
            loader.ExceptionOutput.LinkTo( queueEndPointForExceptions  );
            queueEndPointForExceptions.DefaultOutput.LinkTo( unCombinerForExceptions );
            unCombinerForExceptions.DefaultOutput.LinkTo( finishNodeException );
            
            addToCache.DefaultOutput.LinkTo( finishNode );
            addToCache.ExceptionOutput.LinkTo( finishNodeException );

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

            var loader = new BundlesFromWebRequestLoadProcessor( serverUrl, AssetBundleManifest);
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


            manifestChecker.ExistOutput.LinkTo( cacheChecker );

            cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
            getFromCache.DefaultOutput.LinkTo( finishNode );
            getFromCache.ExceptionOutput.LinkTo( finishNodeException );

            cacheChecker.NotExistInCacheOutput.LinkTo( combiner );

            combiner.DefaultOutput.LinkTo( queue );
            queue.DefaultOutput.LinkTo( loader );
            loader.DefaultOutput.LinkTo( queueEndPoint );
            queueEndPoint.DefaultOutput.LinkTo( unCombiner );
            unCombiner.DefaultOutput.LinkTo( addToCache );
            
            loader.ExceptionOutput.LinkTo( queueEndPointForExceptions );
            queueEndPointForExceptions.DefaultOutput.LinkTo( unCombinerForExceptions );
            unCombinerForExceptions.DefaultOutput.LinkTo( finishNodeException );
            
            addToCache.DefaultOutput.LinkTo( finishNode );
            addToCache.ExceptionOutput.LinkTo( finishNodeException );

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

            var assetFromBundleLoader = new AssetFromBundleLoadProcessor( AssetBundleManifest, AssetsStorage );
            _allProcessors.Add( assetFromBundleLoader );

            var bundlesWithDepsLoader = new BundleDepsLoadingProcessor( AssetBundleManifest, AssetsStorage );
            _allProcessors.Add( bundlesWithDepsLoader );
            
            var addToCache = new AddAssetToCacheWithRefcountProcessor< Object >( AssetsFromBundlesCache );
            _allProcessors.Add( addToCache );

            var finishNode = new AssetLoadingRequestEndPointProcessor< Object >( _promiseMap );
            _allProcessors.Add( finishNode );
            
            var finishNodeException = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( finishNodeException );

            manifestChecker.ExistOutput.LinkTo( cacheChecker );
            cacheChecker.ExistInCacheOutput.LinkTo( getFromCache );
            getFromCache.DefaultOutput.LinkTo( finishNode );
            getFromCache.ExceptionOutput.LinkTo( finishNodeException );

            cacheChecker.NotExistInCacheOutput.LinkTo( bundlesWithDepsLoader );

            bundlesWithDepsLoader.DefaultOutput.LinkTo( assetFromBundleLoader );
            bundlesWithDepsLoader.ExceptionOutput.LinkTo( finishNodeException );
            
            assetFromBundleLoader.DefaultOutput.LinkTo( addToCache );
            assetFromBundleLoader.ExceptionOutput.LinkTo( finishNodeException );
            
            addToCache.DefaultOutput.LinkTo( finishNode );
            addToCache.ExceptionOutput.LinkTo( finishNodeException );
            
            return manifestChecker;
        }

        public void AddExceptionsHandlingForAllNodes( params IFlowNode[ ] additionalNodes )
        {
            PrintExceptionForNodes( _allProcessors );
            PrintExceptionForNodes( additionalNodes );
        }

        public AssetLoadingRequestEndPointProcessor< TBodyType > GetNewAssetLoadingRequestEndpoint< TBodyType >()
        {
            var processor = new AssetLoadingRequestEndPointProcessor< TBodyType >( _promiseMap );
            _allProcessors.Add( processor );
            return processor;
        }

        public IEnumerable< T > GetExistingNodes< T >() where T : IFlowNode
        {
            var nodeType = typeof( T ); 
            return _allProcessors.Where( node => node.GetType() == nodeType ).Cast< T >();
        }

        public UrlRequestEndPointProcessor GetNewUrlRequestEndpoint()
        {
            var processor = new UrlRequestEndPointProcessor( _promiseMap );
            _allProcessors.Add( processor );
            return processor;
        }
        #endregion

        private void PrintExceptionForNodes( IEnumerable< IFlowNode > nodes )
        {
            if( nodes == null )
            {
                return;
            }
            
            foreach( var node in nodes )
            {
                node.OnStatusChanged += PrintExceptionForNode;
            }
        }
        
        private void PrintExceptionForNode(object sender, FlowNodeStatusChangedEventArgs args)
        {
            if( args.NewStatus == FlowNodeStatus.Failed )
            {
                var exception = args.Node.Exception;
                Debug.LogException( exception );

                if( _promiseMap.Has( args.Header.Id ) )
                {
                    _promiseMap.Get( args.Header.Id ).SetError( exception );
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.AssetsSystem.Processors;
using UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.DebugTools
{
    public class AssetsSystemDebugInterface
    {
        private static AssetsSystemDebugInterface instance;
        private AssetsSystemDebugInfo _debugInfo;

        /// <summary>
        /// Call me first
        /// </summary>
        /// <param name="promiseMap"></param>
        /// <param name="allNodes"></param>
        /// <param name="requestsQueue"></param>
        public static void Setup(AssetsStorage assetsStorage, RequestToPromiseMap promiseMap, List< IFlowNode > allNodes )
        {
            if( instance == null )
            {
                instance = new AssetsSystemDebugInterface();
            }

            instance._debugInfo = new AssetsSystemDebugInfo(assetsStorage, promiseMap, allNodes );
        }

        public static AssetsSystemDebugInfo GetInfoForViewer()
        {
            return instance?._debugInfo;
        }

        public static void AddRequestQueue( string name, RequestsQueue requestsQueue )
        {
            instance._debugInfo.RequestsQueue.Add( name, requestsQueue );
        }

        public static void AddCache( string name, ICache cache )
        {
            instance._debugInfo.Caches.Add( name, cache );
        }
        
        public static void AddRefcountCacheController( string name, ICacheControllerWithAssetReferences cache )
        {
            instance._debugInfo.RefcountCacheControllers.Add( name, cache );
        }

        public static void AddCombinedNodeDict( string name, Dictionary< string, CombinedRequest > dict )
        {
            instance._debugInfo.MessageCombineNodesInfos.Add( name, dict );
        }

        public static void ForceReleaseAll()
        {
            if( instance == null )
            {
                return;
            }

            instance._debugInfo = null;
            instance = null;
        }
    }

    public class AssetsSystemDebugInfo
    {
        public RequestsHistoryInfo RequestsHistoryInfo;
        public RequestToPromiseMap PromiseMap;
        
    
        public Dictionary< string, ICache > Caches = new Dictionary< string, ICache >();
        public Dictionary< string, ICacheControllerWithAssetReferences > RefcountCacheControllers = new Dictionary< string, ICacheControllerWithAssetReferences >();
        public Dictionary< string, RequestsQueue > RequestsQueue = new Dictionary< string, RequestsQueue >();
        public Dictionary< string, Dictionary< string, CombinedRequest > > MessageCombineNodesInfos = new Dictionary< string, Dictionary< string, CombinedRequest > >();
    
        
        public AssetsSystemDebugInfo(AssetsStorage assetsStorage, RequestToPromiseMap promiseMap, List< IFlowNode > allNodes )
        {
            PromiseMap = promiseMap;
            RequestsHistoryInfo = new RequestsHistoryInfo(assetsStorage, allNodes );
        }
    }

    
}

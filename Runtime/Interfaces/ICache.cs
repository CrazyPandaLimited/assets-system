#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections;
using System.Collections.Generic;


namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public interface ICache<TStoredResourcesType>
    {
        #region Public Members

        /// <summary>
        /// Determines whether [contains] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool Contains(string key);


        /// <summary>
        /// Return all assetsNames, stored in cache
        /// </summary>
        /// <returns></returns>
        List<string> GetAllResorceNames();

        /// <summary>
        /// Adds resource to cache
        /// </summary>
        /// <param name="owner">The resource owner</param>
        /// <param name="key">The key</param>
        /// <param name="data">The resource</param>
        void Add(object owner, string key, TStoredResourcesType data);
        
        /// <summary>
        /// Gets the data, stored in cache
        /// </summary>
        /// <param name="owner">New owner for resource</param>
        /// <param name="key">The key</param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Cached resource</returns>
        TStoredResourcesType Get(object owner, string key);

        /// <summary>
        /// Async adds resource to cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="resource">The resource to keep</param>
        /// <returns></returns>
        IEnumerator AddAsync(object owner,string key, TStoredResourcesType resource);
        
        /// <summary>
        /// Async gets the data, stored in cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns></returns>
        ICacheGettingOperation<TStoredResourcesType> GetAsync(object owner,string key);


        /// <summary>
        /// Get list with resources without owners
        /// </summary>
        /// <param name="keys"></param>
        List<string> GetUnusedResourceNames();

        /// <summary>
        /// Will get all resources from cache, holds by this owner.
        /// </summary>
        /// <param name="owner">The resource owner</param>
        List<string> GetOwnerResourcesNames(object owner);        

        Dictionary<string, object> ReleaseAllResources();

        Dictionary<string, object> ReleaseResource(object owner, string uri);

        Dictionary<string, object> ReleaseResources(object owner, List<string> uri);

        Dictionary<string, object> ForceReleaseResource(string uri);

        Dictionary<string, object> ForceReleaseResources(List<string> uri);

        Dictionary<string, object> ReleaseAllOwnerResources(object owner);

        Dictionary<string, object> ReleaseUnusedResources();

        #endregion
    }
}
#endif
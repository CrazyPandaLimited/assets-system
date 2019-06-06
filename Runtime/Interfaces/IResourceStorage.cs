#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public interface IResourceStorage
    {
        /// <summary>
        /// Add new loader
        /// </summary>
        /// <param name="_loader"></param>
        void RegisterResourceLoader(IResourceLoader _loader);

        /// <summary>
        /// Returns true if this loader type already registered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool IsLoaderRegistered<T>() where T : IResourceLoader;

        /// <summary>
        /// Returns first finded loader of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        T GetResourceLoader<T>() where T : IResourceLoader;

        /// <summary>
        /// Returns all registered loaders of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        List<T> GetResourceLoaders<T>() where T : IResourceLoader;

        /// <summary>
        /// Returns true if resource already cached by any of loader
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        bool IsResourceCached(string url);

        /// <summary>
        /// Returns true if resource can be loaded synchronously
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        bool CanLoadResourceImmediately<T>(object owner, string url) where T : UnityEngine.Object;

        /// <summary>
        /// Load resource synchronously
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <typeparam name="T">Resource type</typeparam>
        /// <returns></returns>
        T LoadResourceImmediately<T>(object owner, string url) where T : UnityEngine.Object;

        /// <summary>
        /// Load resource async
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <typeparam name="T">Resource type</typeparam>
        /// <returns>Loading operation to get loaded resource</returns>
        ILoadingOperation<T> LoadResource<T>(object owner, string url) where T : class;

        /// <summary>
        /// Load resource async
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <param name="actionOnLoad">Handler to handle load complete</param>
        /// <param name="actionOnStep">Handler to handle loading progress</param>
        /// <typeparam name="T"></typeparam>
        void LoadResource<T>(object owner, string url, Action<ResourceLoadedEventArgs<T>> actionOnLoad,
            Action<ResourceLoadingProgressEventArgs> actionOnStep = null) where T : class;

        /// <summary>
        /// Close all loadings by resource owner
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        void CancelAllRequests(object owner);

        /// <summary>
        /// Release resource, stored in cache without owners check
        /// </summary>
        /// <param name="url">Resource URL</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        void ForceReleaseFromCache(string url, bool destroy = true);

        /// <summary>
        /// Release resource, stored in cache. Resource can't be released if it has other owner!
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        /// <param name="url">Resource URL</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        void ReleaseFromCache(object owner, string url, bool destroy = true);

        /// <summary>
        /// Release all resources, stored in cache. Resource can't be released if it has other owner!
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        void ReleaseAllOwnerResourcesFromCache(object owner, bool destroy = true);

        /// <summary>
        /// Release resources without owners
        /// </summary>
        /// <param name="forceCollect">force call GC.Collect()</param>
        /// <param name="destroyResources">Is force remove resource from memory</param>
        void UnloadUnusedResources(bool forceCollect = false, bool destroyResources = true);

        /// <summary>
        /// Release all resources from all loaders and caches
        /// </summary>
        void UnloadAll();
    }
}
#endif
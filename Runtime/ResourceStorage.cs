#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.ResourcesSystem.LoaderChoiceResolvers;
using UnityEngine;
using Object = UnityEngine.Object;
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
using CrazyPanda.UnityCore.ResourcesSystem.DebugTools;

#endif

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public partial class ResourceStorage : IDisposable, IResourceStorage
    {
        private readonly List<IResourceLoader> _loaders;
        private bool _isDisposed;
        private readonly ILoaderChoiceResolver _loaderChoiceResolver;

        private WorkersQueue _workersQueue;
        private event Action<Exception> _onInternalError;
        private event Action<Exception> _onResourceLoadError;


        #region Constructors

        /// <summary>
        ///     Constructor to customize Loaders resolver
        /// </summary>
        /// <param name="maxSimultaneousResouceLoadings">Max parallel loadings</param>
        /// <param name="loaderChoiceResolverOverride">Custom loader resolver</param>
        /// <param name="onInternalError">Handler to catch internal exceptions</param>
        /// <param name="onResourceLoadError">Handler to catch all loader loading exceptions</param>
        public ResourceStorage(int maxSimultaneousResouceLoadings, ILoaderChoiceResolver loaderChoiceResolverOverride, Action<Exception> onInternalError = null,
            Action<Exception> onResourceLoadError = null)
        {
            _loaders = new List<IResourceLoader>();
            _workersQueue = new WorkersQueue(maxSimultaneousResouceLoadings);
            _loaderChoiceResolver = loaderChoiceResolverOverride;
            _loaderChoiceResolver.Init(_loaders);
            _onInternalError = onInternalError;
            _onResourceLoadError = onResourceLoadError;
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
            ResourceSystemLocator.RegisterInstance(this);
#endif
            _isDisposed = false;
        }

        /// <summary>
        ///     Default constructor
        /// </summary>
        /// <param name="maxSimultaneousResouceLoadings">Max parallel loadings</param>
        /// <param name="onInternalError">Handler to catch internal exceptions</param>
        /// <param name="onResourceLoadError">Handler to catch all loader loading exceptions</param>
        public ResourceStorage(int maxSimultaneousResouceLoadings, Action<Exception> onInternalError = null, Action<Exception> onResourceLoadError = null) :
            this(maxSimultaneousResouceLoadings, new SimpleListLoaderChoiceResolver(), onInternalError, onResourceLoadError)
        {
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Add new loader
        /// </summary>
        /// <param name="_loader"></param>
        public void RegisterResourceLoader(IResourceLoader _loader)
        {
            try
            {
                _loader.OnRegisteredToResourceStorage(this, _workersQueue, _onResourceLoadError);
                _loaders.Add(_loader);
                _loaderChoiceResolver.NewLoaderRegistered(_loader);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Returns true if this loader type already registered
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsLoaderRegistered<T>() where T : IResourceLoader
        {
            try
            {
                foreach (var loader in _loaders)
                {
                    if (loader is T)
                    {
                        return true;
                    }
                    
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            return false;
        }

        /// <summary>
        /// Returns first finded loader of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T GetResourceLoader<T>() where T : IResourceLoader
        {
            try
            {
                foreach (var loader in _loaders)
                {
                    if (loader is T)
                    {
                        return (T) loader;
                    }
                    
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            CallInternalException(new LoaderNotRegisteredException("Loader type not found type:" + typeof(T)));
            return default(T);
        }

        /// <summary>
        /// Returns all registered loaders of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public List<T> GetResourceLoaders<T>() where T : IResourceLoader
        {
            var results = new List<T>();
            try
            {
                foreach (var loader in _loaders)
                {
                    if (loader is T)
                    {
                        results.Add((T) loader);
                    }
                    
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            if (results.Count == 0)
            {
                CallInternalException(new LoaderNotRegisteredException("Loader type not found type:" + typeof(T)));
            }

            return results;
        }

        /// <summary>
        /// Returns true if resource already cached by any of loader
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public bool IsResourceCached(string url)
        {
            ValidateUri(url);
            string resolvedUri;
            var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
            return resolvedLoader.IsCached(resolvedUri);
        }

        /// <summary>
        /// Returns true if resource can be loaded synchronously
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="url"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool CanLoadResourceImmediately<T>(object owner, string url) where T : Object
        {
            try
            {
                ValidateOwner(owner);
                ValidateUri(url);
                string resolvedUri;
                var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
                return resolvedLoader.CanLoadResourceImmediately<T>(owner, resolvedUri);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            return false;
        }

        /// <summary>
        /// Load resource synchronously
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <typeparam name="T">Resource type</typeparam>
        /// <returns></returns>
        public T LoadResourceImmediately<T>(object owner, string url) where T : Object
        {
            try
            {
                ValidateOwner(owner);
                ValidateUri(url);
                string resolvedUri;
                var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
                return resolvedLoader.LoadResourceImmediately<T>(owner, resolvedUri);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            return default(T);
        }

        /// <summary>
        /// Load resource async
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <typeparam name="T">Resource type</typeparam>
        /// <returns>Loading operation to get loaded resource</returns>
        public ILoadingOperation<T> LoadResource<T>(object owner, string url) where T : class
        {
            return InternalLoadResource<T>(owner, url);
        }

        /// <summary>
        /// Load resource async
        /// </summary>
        /// <param name="owner">Who request resource</param>
        /// <param name="url">Resource URL</param>
        /// <param name="actionOnLoad">Handler to handle load complete</param>
        /// <param name="actionOnStep">Handler to handle loading progress</param>
        /// <typeparam name="T"></typeparam>
        public void LoadResource<T>(object owner, string url, Action<ResourceLoadedEventArgs<T>> actionOnLoad,
            Action<ResourceLoadingProgressEventArgs> actionOnStep = null) where T : class
        {
            var loadingOperation = InternalLoadResource<T>(owner, url);

            if (loadingOperation == null)
            {
                if (actionOnLoad != null)
                {
                    actionOnLoad(ResourceLoadedEventArgs<T>.CompletedError(url,
                        new ImpossibleToLoadResourceException(string.Format("ResourceType:{0} Owner:{1} Url:{2}", typeof(T), owner, url))));
                    
                }
                if (_onResourceLoadError != null)
                {
                    _onResourceLoadError(new ImpossibleToLoadResourceException(string.Format("ResourceType:{0} Owner:{1} Url:{2}", typeof(T), owner, url)));
                }

                return;
            }

            if (actionOnLoad != null)
            {
                if (loadingOperation.Error != null)
                {
                    actionOnLoad(ResourceLoadedEventArgs<T>.CompletedError(url, loadingOperation.Error));
                    return;
                }

                if (loadingOperation.IsCompleted)
                {
                    actionOnLoad(ResourceLoadedEventArgs<T>.CompletedSuccess(url, loadingOperation.Resource));
                    return;
                }

                loadingOperation.OnResourceLoaded += actionOnLoad;
            }

            if (actionOnStep != null)
            {
                loadingOperation.OnLoadStep += actionOnStep;
            }
        }


        private ILoadingOperation<T> InternalLoadResource<T>(object owner, string url) where T : class
        {
            try
            {
                ValidateOwner(owner);
                ValidateUri(url);
                string resolvedUri;
                var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
                return resolvedLoader.CreateLoadingWorkerAsync<T>(owner, resolvedUri);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }

            return null;
        }

        /// <summary>
        /// Close all loadings by resource owner
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        public void CancelAllRequests(object owner)
        {
            try
            {
                ValidateOwner(owner);
                _workersQueue.CancelAllRequests(owner);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Release resource, stored in cache without owners check
        /// </summary>
        /// <param name="url">Resource URL</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        public void ForceReleaseFromCache(string url, bool destroy = true)
        {
            try
            {
                ValidateUri(url);
                string resolvedUri;
                var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
                resolvedLoader.ForceReleaseFromCache(resolvedUri, destroy);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Release resource, stored in cache. Resource can't be released if it has other owner!
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        /// <param name="url">Resource URL</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        public void ReleaseFromCache(object owner, string url, bool destroy = true)
        {
            try
            {
                ValidateOwner(owner);
                ValidateUri(url);
                string resolvedUri;
                var resolvedLoader = _loaderChoiceResolver.Resolve(url, out resolvedUri);
                resolvedLoader.ReleaseFromCache(owner, resolvedUri, destroy);
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Release all resources, stored in cache. Resource can't be released if it has other owner!
        /// </summary>
        /// <param name="owner">Who requested resource</param>
        /// <param name="destroy">Is force remove resource from memory</param>
        public void ReleaseAllOwnerResourcesFromCache(object owner, bool destroy = true)
        {
            try
            {
                ValidateOwner(owner);
                //_workersQueue.CancelAllRequests(owner);
                for (var i = 0; i < _loaders.Count; i++)
                {
                    _loaders[i].ReleaseAllOwnerResourcesFromCache(owner, destroy);
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Release resources without owners
        /// </summary>
        /// <param name="forceCollect">force call GC.Collect()</param>
        /// <param name="destroyResources">Is force remove resource from memory</param>
        public void UnloadUnusedResources(bool forceCollect = false, bool destroyResources = true)
        {
            try
            {
                foreach (var loader in _loaders)
                {
                    loader.ReleaseUnusedFromCache(destroyResources);
                }

                if (forceCollect)
                {
                    GC.Collect();
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Release all resources from all loaders and caches
        /// </summary>
        public void UnloadAll()
        {
            try
            {
                _workersQueue.CancelAllRequests();
                for (var i = 0; i < _loaders.Count; i++)
                {
                    _loaders[i].ReleaseAllFromCache();
                }
            }
            catch (Exception e)
            {
                CallInternalException(e);
            }
        }

        /// <summary>
        /// Dispose system
        /// </summary>
        public void Dispose()
        {
            UnloadAll();
            _workersQueue = null;
            _onInternalError = null;
            _loaders.Clear();
            _isDisposed = true;
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
            ResourceSystemLocator.ReleaseInstance(this);
#endif
        }

        ~ResourceStorage()
        {
            if (_isDisposed)
            {
                return;
            }

            Dispose();
        }

        #endregion

        #region Private Members

        private void ValidateOwner(object owner)
        {
            if (owner == null)
            {
                throw new ResourceOwnerException("Owner can not be null");
            }
        }

        private void ValidateUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
            {
                throw new ResourceNameException("Resource name can not be null or empty");
            }
        }

        private void CallInternalException(Exception ex)
        {
            if (_onInternalError != null)
            {
                _onInternalError(ex);
            }
#if !CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_OFF_LOG_EXCEPTION_TO_CONSOLE
            else
            {
                Debug.LogException(ex);
            }
#endif
        }

        #endregion
    }
}
#endif
#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.LoaderChoiceResolvers
{
    public class BundleManifestBasedLoaderChoiceResolver : ILoaderChoiceResolver
    {
        private List<IResourceLoader> _loadersList;

        public void Init(List<IResourceLoader> loadersList)
        {
            _loadersList = loadersList;
        }

        public void NewLoaderRegistered(IResourceLoader newLoader)
        {
            
        }

        public IResourceLoader Resolve(string uri, out string resolvedUri)
        {
            foreach (var resourceLoader in _loadersList)
            {
                if (resourceLoader is IBundlesLoader && ((IBundlesLoader) resourceLoader).Manifest.IsResourceExistInBundles(uri))
                {
                    resolvedUri = UrlHelper.GetUriWithPrefix(resourceLoader.SupportsMask, uri);
                    return resourceLoader;
                }
            }

            foreach (var resourceLoader in _loadersList)
            {
                if (!(resourceLoader is IBundlesLoader) && uri.StartsWith(resourceLoader.SupportsMask))
                {
                    resolvedUri = UrlHelper.GetUriWithPrefix(resourceLoader.SupportsMask, uri);
                    return resourceLoader;
                }
            }

            throw new LoaderNotRegisteredException("Loader not resolved for uri:" + uri);
        }
    }
}
#endif
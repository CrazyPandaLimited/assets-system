#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.ResourcesSystem.LoaderChoiceResolvers
{
    public interface ILoaderChoiceResolver
    {
        void Init(List<IResourceLoader> loadersList);

        void NewLoaderRegistered(IResourceLoader newLoader);
        IResourceLoader Resolve(string uri, out string resolvedUri);
    }
}
#endif
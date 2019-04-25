#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public interface IBundlesLoader:IResourceLoader
    {
        AssetBundleManifest Manifest { get; }
        
    }
}
#endif
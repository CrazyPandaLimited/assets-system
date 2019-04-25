#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public interface IAntiCacheUrlResolver
    {
        string ResolveURL(string uri, string anticacheResourceKey = "");
    }
}
#endif

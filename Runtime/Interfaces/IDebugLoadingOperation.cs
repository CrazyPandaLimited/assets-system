#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM && CRAZYPANDA_UNITYCORE_RESOURCESYSTEM_DEBUG_TOOLS
namespace CrazyPanda.UnityCore.ResourcesSystem.DebugTools
{
    public interface IDebugLoadingOperation
    {
        string Uri { get; }
        float Progress { get; }
        bool IsCompleted { get; }
        object Owner { get; }
    }
}
#endif
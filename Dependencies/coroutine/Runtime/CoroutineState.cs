#if CRAZYPANDA_UNITYCORE_COROUTINE
namespace CrazyPanda.UnityCore.CoroutineSystem
{
    public enum CoroutineState
    {
        NotStarted,
        InProgress,
        Paused,
		Stopped,
        Completed
    }
}
#endif
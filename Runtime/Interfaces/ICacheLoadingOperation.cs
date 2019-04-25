#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public interface ICacheGettingOperation<TCacheStoredResourcesType>
	{
		TCacheStoredResourcesType Result { get; }
		bool IsCompleted { get; }
		IEnumerator StartProcess();
	}
}
#endif
#if CRAZYPANDA_UNITYCORE_COROUTINE
namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public class CoroutineSystemWaitForSeconds
	{
		#region Properties
		public double Seconds { get; private set; }
		#endregion

		#region Constructors
		public CoroutineSystemWaitForSeconds( double seconds )
		{
			Seconds = seconds;
		}
		#endregion
	}
}
#endif
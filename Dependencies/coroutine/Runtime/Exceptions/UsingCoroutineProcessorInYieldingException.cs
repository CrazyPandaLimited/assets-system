#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
	public class UsingCoroutineProcessorInYieldingException : Exception
	{
		#region Constants
		private const string ErrorMessage = "You are not allow use IProcessor in yieldinig. Looks like you don't understand what you doing";
		#endregion

		#region Constructors
		public UsingCoroutineProcessorInYieldingException() : base( ErrorMessage )
		{
		}

		public UsingCoroutineProcessorInYieldingException( Exception innerException ) : base( ErrorMessage, innerException )
		{
		}
		#endregion
	}
}
#endif
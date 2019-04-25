#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class RemoteLoadingEventArgs
	{
		#region Properties
		/// <summary>
		/// Gets a value indicating whether this <see cref="RemoteLoadingEventArgs"/> is fails with error.
		/// </summary>
		/// <value>
		///   <c>null</c> if success; otherwise, <c>any Exception</c>.
		/// </value>
		public Exception Error { get; private set; }
		#endregion

		public byte[] Data { get; private set; }

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="RemoteLoadingEventArgs"/> class.
		/// </summary>
		/// <param name="localPath">The local path.</param>
		/// <param name="err">Exception object if <see cref="success"/> is false.</param>
		public RemoteLoadingEventArgs( byte[] data, Exception err = null )
		{
			Data = data;
			Error = err;
		}
		#endregion
	}
}
#endif

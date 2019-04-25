#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public class ResourceReferenceInfo
	{
		#region Properties
		public object Resource { get; private set; }
		public HashSet< WeakReference > Owners { get; private set; }
		#endregion

		#region Constructors
		public ResourceReferenceInfo()
		{
			Owners = new HashSet< WeakReference >();
		}

		public ResourceReferenceInfo( object data ) : this()
		{
			Resource = data;
		}
		#endregion

		#region Public Members
		public bool ContainsOwner( object owner )
		{
			return Owners.Any( weakReference => weakReference.Target == owner );
		}

		public void AddOwner( object owner )
		{
			Owners.Add( new WeakReference( owner ) );
		}

		public void RemoveOwner( object owner )
		{
			var wr = Owners.FirstOrDefault( weakReference => weakReference.Target == owner );
			Owners.Remove( wr );
		}
		#endregion
	}
}
#endif
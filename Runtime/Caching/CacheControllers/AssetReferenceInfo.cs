using System;
using System.Collections.Generic;
using System.Linq;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetReferenceInfo
    {
        #region Properties
        public HashSet< WeakReference > References { get; private set; }
        #endregion

        #region Constructors
        public AssetReferenceInfo()
        {
            References = new HashSet< WeakReference >();
        }
        #endregion

        #region Public Members
        public bool ContainsReference( object reference )
        {
            return References.Any( weakReference => weakReference.Target == reference );
        }

        public void AddReference( object reference )
        {
            if (!ContainsReference(reference))
            {
                References.Add(new WeakReference(reference));
            }
            ClearNullReferences();
        }

        public void RemoveReference( object reference )
        {            
            var wr = References.FirstOrDefault( weakReference => weakReference.Target == reference );
            References.Remove( wr );
            ClearNullReferences();
        }

        public bool HasReferences()
        {
            ClearNullReferences();
            return References.Count > 0;
        }
        #endregion

        #region Protected Members
        protected void ClearNullReferences()
        {
            var removedOwners = new List< WeakReference >();
            foreach( var weakReference in References )
            {
                if( weakReference.Target == null || weakReference.Target.ToString() == "null" )
                {
                    removedOwners.Add( weakReference );
                }
            }

            foreach( var weakReference in removedOwners )
            {
                References.Remove( weakReference );
            }
        }
        #endregion
    }
}

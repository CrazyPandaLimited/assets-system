using System;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class TryOfOverridingCachedObjectException : Exception
    {
        #region Constructors
        public TryOfOverridingCachedObjectException(string message) : base(message)
        {
        }
        #endregion
    }
}
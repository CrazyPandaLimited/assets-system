using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AnticacheNotFoundException : Exception
    {
        #region Constructors
        public AnticacheNotFoundException( string message ) : base( message )
        {
        }
        #endregion
    }
}

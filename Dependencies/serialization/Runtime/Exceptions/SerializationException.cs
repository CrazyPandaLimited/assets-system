#if CRAZYPANDA_UNITYCORE_SERIALIZATION_JSON
using System;

namespace CrazyPanda.UnityCore.Serialization
{
    public class SerializationException : Exception
    {
        #region Constructors
        public SerializationException( string message ) : base( message )
        {
        }

        public SerializationException( string message, params object[ ] values ) : base( string.Format( message, values ) )
        {
        }


        public SerializationException( Exception innerException, string message ) : base( message, innerException )
        {
        }

        public SerializationException( Exception innerException, string message, params object[ ] values ) : base( string.Format( message, values ), innerException )
        {
        }
        #endregion
    }
}
#endif
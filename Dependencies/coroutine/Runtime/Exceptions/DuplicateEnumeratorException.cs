#if CRAZYPANDA_UNITYCORE_COROUTINE
using System;
using System.Collections;

namespace CrazyPanda.UnityCore.CoroutineSystem
{
    public class DuplicateEnumeratorException : Exception
    {
        #region Properties
        public IEnumerator Enumerator { get; private set; }
        #endregion

        #region Constructors
        public DuplicateEnumeratorException( IEnumerator enumerator ) : base( "Can't add same enumerator twice" )
        {
            Enumerator = enumerator;
        }

        public DuplicateEnumeratorException( IEnumerator enumerator, Exception innerException ) : base( "Can't add same enumerator twice", innerException )
        {
            Enumerator = enumerator;
        }
        #endregion
    }
}
#endif
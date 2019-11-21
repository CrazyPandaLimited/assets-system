namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class InvalidHashException : FileCachingException
    {
        #region Constructors
        public InvalidHashException( string message ) : base( message )
        {
        }
        #endregion
    }
}

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class CachedFileNotFoundException : FileCachingException
    {
        #region Constructors
        public CachedFileNotFoundException( string message ) : base( message )
        {
        }
        #endregion
    }
}

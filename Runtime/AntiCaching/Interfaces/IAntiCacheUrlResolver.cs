namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface IAntiCacheUrlResolver
    {
        #region Public Members
        string ResolveURL( string uri, string anticacheAssetKey = "" );
        #endregion
    }
}

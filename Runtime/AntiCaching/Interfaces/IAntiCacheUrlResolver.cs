namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface IAntiCacheUrlResolver
    {
        string ResolveURL( string uri, string anticacheAssetKey = "" );
    }
}

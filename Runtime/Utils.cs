namespace CrazyPanda.UnityCore.AssetsSystem
{
    public static class Utils
    {
        public static string ConstructAssetWithSubassetName( string assetName, string subAssetName )
        {
            return $"{assetName}_SubAsset:{subAssetName}";
        }
    }
}

namespace CrazyPanda.UnityCore.PandaTasks.Tests
{
    public class AssetBundlesToStreamingAssetsProvider : CrazyPanda.UnityCore.DeliverySystem.AssetBundlesToStreamingAssetsProvider
    {
        protected override CrazyPanda.UnityCore.DeliverySystem.AssetBundlesProvider AssetBundlesProvider { get; } = new AssetBundlesProvider();
    }
}

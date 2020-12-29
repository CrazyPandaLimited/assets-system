using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class AssetsMemoryCacheTests : ICacheTests< AssetsMemoryCache >
    {
        [Test]
        public override void GetNotExistedElementTest()
        {
            Assert.Throws< AssetNotFoundInCacheException >( () => { _assetsMemoryCache.Get( "any" ); } );
        }
    }
}
using NUnit.Framework;

namespace CrazyPanda.UnityCore.AssetsSystem.ModuleTests
{
    public sealed class AssetsMemoryCacheTests : BaseCacheTests< AssetsMemoryCache >
    {
        [Test]
        public override void GetNotExistedElementTest()
        {
            Assert.Throws< AssetNotFoundInCacheException >( () => { _memoryCache.Get( "any" ); } );
        }
    }
}
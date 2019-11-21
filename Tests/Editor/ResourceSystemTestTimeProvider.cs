using CrazyPanda.UnityCore.CoroutineSystem;
using NSubstitute;

namespace CrazyPanda.UnityCore.AssetsSystem
{
	public static class ResourceSystemTestTimeProvider
    {
        #region Public Members
        public static ITimeProvider TestTimeProvider()
        {
            var timeProvider = Substitute.For< ITimeProvider >();
            timeProvider.deltaTime.Returns( 1f / 60f );
            return timeProvider;
        }
        #endregion
    }
}

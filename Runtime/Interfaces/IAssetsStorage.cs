using System;
using System.Threading;
using CrazyPanda.UnityCore.PandaTasks;
using CrazyPanda.UnityCore.PandaTasks.Progress;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface IAssetsStorage : IDisposable
    {
        #region Public Members
        T LoadAssetSync< T >( string url );
        T LoadAssetSync< T >( string url, MetaData metaData );

        IPandaTask< T > LoadAssetAsync< T >( string url );
        IPandaTask< T > LoadAssetAsync< T >( string url, MetaData metaData );
        IPandaTask< T > LoadAssetAsync< T >( string url, MetaData metaData, IProgressTracker< float > tracker );
        IPandaTask< T > LoadAssetAsync< T >( string url, MetaData metaData, CancellationToken tocken );
        IPandaTask< T > LoadAssetAsync< T >( string url, MetaData metaData, CancellationToken tocken, IProgressTracker< float > tracker );
        #endregion
    }
}

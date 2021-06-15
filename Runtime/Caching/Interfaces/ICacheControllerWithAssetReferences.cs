using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface ICacheControllerWithAssetReferences
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="reference"></param>
        /// <exception cref="NullReferenceException"></exception>
        void Add< T >( T asset, string assetName, object reference );

        T Get< T >( string assetName, object reference );

        Object Get( string assetName, object reference, Type assetType );

        void ReleaseReference( string assetName, object reference );

        /// <summary>
        /// Determines whether [contains] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool Contains( string key );

        List< string > GetAllAssetsNames();
        List< object > GetReferencesByAssetName( string assetName );
    }
}

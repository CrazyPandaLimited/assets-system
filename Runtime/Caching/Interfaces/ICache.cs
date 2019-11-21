using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface ICache
    {
        #region Public Members
        /// <summary>
        /// Determines whether [contains] [the specified key].
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        bool Contains( string key );


        /// <summary>
        /// Return all assetsNames, stored in cache
        /// </summary>
        /// <returns></returns>
        List< string > GetAllAssetsNames();

        /// <summary>
        /// Adds asset to cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="asset">The asset</param>
        void Add( string key, object asset );

        /// <summary>
        /// Gets the asset, stored in cache
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>Cached asset</returns>
        object Get( string key );

        /// <summary>
        /// Remove asset from cache with key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        void Remove( string key );

        /// <summary>
        /// Clear all assets from cache
        /// </summary>
        void ClearCache();
        #endregion
    }
}

using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetsMemoryCache : ICache
    {
        #region Private Fields
        private readonly Dictionary< string, object > _assets;
        #endregion

        #region Constructors
        public AssetsMemoryCache() : this( 200 )
        {
            
        }

        public AssetsMemoryCache( int startCapacity )
        {
            _assets = new Dictionary< string, object >( startCapacity );
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Check if cache contains element
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Contains( string key )
        {
            if( string.IsNullOrEmpty( key ) )
            {
                throw new ArgumentNullException( "empty or null key" );
            }

            return _assets.ContainsKey( key );
        }

        public void Add( string key, object asset )
        {
            if( string.IsNullOrEmpty( key ) )
            {
                throw new ArgumentNullException( "empty or null key" );
            }

            if( asset == null )
            {
                throw new ArgumentNullException( "asset is null" );
            }

            if( _assets.ContainsKey( key ) )
            {
                throw new InvalidOperationException($"Cache already contains element with key={key}");
            }

            _assets[ key ] = asset;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AssetMemoryCacheException"></exception>
        public object Get( string key )
        {
            if( string.IsNullOrEmpty( key ) )
            {
                throw new ArgumentNullException( "empty or null key" );
            }

            if( Contains( key ) )
            {
                return _assets[ key ];
            }

            throw new AssetNotFoundInCacheException( key );
        }

        /// <summary>
        /// Removes element from cache
        /// </summary>
        /// <param name="key"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public void Remove( string key )
        {
            if( string.IsNullOrEmpty( key ) )
            {
                throw new ArgumentNullException( "empty or null key" );
            }

            if( !Contains( key ) )
            {
                throw new KeyNotFoundException( key );
            }

            _assets.Remove( key );
        }

        public List< string > GetAllAssetsNames()
        {
            return new List< string >( _assets.Keys );
        }

        public void ClearCache()
        {
            _assets.Clear();
        }
        #endregion
    }
}

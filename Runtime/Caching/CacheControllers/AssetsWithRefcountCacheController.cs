using System;
using System.Collections.Generic;

namespace CrazyPanda.UnityCore.AssetsSystem.Caching
{
    /// <summary>
    /// Cache controller keeps assets and counts references on it.
    /// </summary>
    public class AssetsWithRefcountCacheController : ICacheControllerWithAssetReferences
    {
        private readonly AssetsMemoryCache _memCache;
        private readonly Dictionary< string, AssetReferenceInfo > _references;

        public AssetsWithRefcountCacheController( int startCapacity = 200 )
        {
            _references = new Dictionary< string, AssetReferenceInfo >( startCapacity );
            _memCache = new AssetsMemoryCache( startCapacity );
        }

        public bool Contains( string assetName )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentNullException( $"{nameof(assetName)} is null" );
            }

            return _memCache.Contains( assetName );
        }

        public List< string > GetAllAssetsNames()
        {
            return _memCache.GetAllAssetsNames();
        }

        public List< object > GetReferencesByAssetName( string assetName )
        {
            var result = new List< object >();
            if( !_references.ContainsKey( assetName ) )
            {
                return result;
            }

            foreach( var reference in _references[assetName].References )
            {
                result.Add( reference.Target );
            }

            return result;
        }

        /// <summary>
        /// Adds asset to cache
        /// </summary>
        /// <typeparam name="T">asset type</typeparam>
        /// <param name="asset">asset</param>
        /// <param name="assetName">asset name</param>
        /// <param name="reference">object that has reference on this asset. It used to count references on asset and clear assets without references on it for example.</param>
        public virtual void Add< T >( T asset, string assetName, object reference )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentNullException( $"{nameof(assetName)} is null" );
            }

            if( asset == null )
            {
                throw new ArgumentNullException( $"{nameof(asset)} is null" );
            }

            if( !_memCache.Contains( assetName ) )
            {
                _memCache.Add( assetName, asset );
                _references.Add( assetName, new AssetReferenceInfo() );
            }

            _references[ assetName ].AddReference( reference );
        }

        /// <summary>
        /// Gets asset from cache
        /// </summary>
        /// <typeparam name="T">type of asset</typeparam>
        /// <param name="assetName">asset name</param>
        /// <param name="reference">object that will keep reference on returned asset. 
        /// controller will add this reference to all references that referenced to this asset.</param>
        /// <returns>Asset with T type</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AssetTypeMismatchException"></exception>
        public virtual T Get< T >( string assetName, object reference )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentNullException( $"{nameof(assetName)} is null" );
            }

            if( reference == null )
            {
                throw new ArgumentNullException( $"{nameof(reference)} is null" );
            }

            var res = _memCache.Get(assetName);
            if (!(res is T))
            {
                throw new AssetTypeMismatchException( assetName, typeof( T ), res.GetType() );
            }

            _references[ assetName ].AddReference( reference );
            return ( T ) res;
        }

        /// <summary>
        /// Gets asset from cache
        /// </summary>
        /// <param name="assetName">asset name</param>
        /// <param name="assetType">type of asset</typeparam>
        /// <param name="reference">object that will keep reference on returned asset. 
        /// controller will add this reference to all references that referenced to this asset.</param>
        /// <returns>Asset</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AssetTypeMismatchException"></exception>
        public virtual Object Get( string assetName, object reference, Type assetType )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentNullException( $"{nameof(assetName)} is null" );
            }

            if( reference == null )
            {
                throw new ArgumentNullException( $"{nameof(reference)} is null" );
            }

            var res = _memCache.Get(assetName);
            if (!(res.GetType().IsSubclassOf(assetType) || res.GetType() == assetType))
            {
                throw new AssetTypeMismatchException( assetName, assetType, res.GetType() );
            }

            _references[ assetName ].AddReference( reference );
            return res;
        }

        public virtual void Remove( string assetName, bool onlyIfUnused = true, bool destroy = true )
        {
            bool isAssetUsed = _references.ContainsKey( assetName ) && _references[ assetName ].HasReferences();

            if( onlyIfUnused && isAssetUsed )
            {
                return;
            }

            var asset = _memCache.Get( assetName );
            _memCache.Remove( assetName );

            if( _references.ContainsKey( assetName ) )
            {
                _references.Remove( assetName );
            }

            if( destroy )
            {
                DestroyFromMemory( assetName, asset );
            }
        }

        /// <summary>
        /// Controller removes "reference" asset's references.
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="reference"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="AssetNotFoundInCacheException"></exception>
        public void ReleaseReference( string assetName, object reference )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentNullException( $"{nameof(assetName)} is null" );
            }

            if( reference == null )
            {
                throw new ArgumentNullException( $"{nameof(reference)} is null" );
            }

            if( !_memCache.Contains( assetName ) )
            {
                throw new AssetNotFoundInCacheException( assetName );
            }

            if( _references[ assetName ].ContainsReference( reference ) )
            {
                _references[ assetName ].RemoveReference( reference );
            }
        }

        public List< string > GetUnusedAssetNames()
        {
            var unusedAssetsNames = new List< string >();
            foreach( var assetReferenceInfo in _references )
            {
                if( !assetReferenceInfo.Value.HasReferences() )
                {
                    unusedAssetsNames.Add( assetReferenceInfo.Key );
                }
            }

            return unusedAssetsNames;
        }

        public List< string > GetAssetsNamesWithReference( object reference )
        {
            List< string > result = new List< string >( 0 );
            foreach( var assetReferenceInfo in _references )
            {
                if( assetReferenceInfo.Value.ContainsReference( reference ) )
                {
                    result.Add( assetReferenceInfo.Key );
                }
            }

            return result;
        }

        /// <summary>
        /// Removes "reference" from all assets
        /// </summary>
        /// <param name="reference"></param>
        public void ReleaseAllAssetReferences( object reference )
        {
            foreach( var assetName in GetAssetsNamesWithReference( reference ) )
            {
                ReleaseReference( assetName, reference );
            }
        }

        public void RemoveUnusedAssets( bool destroy = true )
        {
            foreach( var unusedAssetName in GetUnusedAssetNames() )
            {
                Remove( unusedAssetName, true, destroy );
            }
        }

        public void RemoveAllAssets( bool destroy = true )
        {
            foreach( var assetsName in GetAllAssetsNames() )
            {
                Remove( assetsName, false, destroy );
            }
        }

        protected virtual void DestroyFromMemory( string assetName, object asset )
        {
            if( asset is UnityEngine.Object )
            {
#if !UNITY_EDITOR
                UnityEngine.Object.Destroy( (UnityEngine.Object) asset);
#endif
            }
        }
    }
}

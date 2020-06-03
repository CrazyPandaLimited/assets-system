using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetBundleManifest
    {
        #region Public Fields
        public readonly Dictionary< string, BundleInfo > BundleInfos = new Dictionary< string, BundleInfo >();
        public readonly Dictionary< string, AssetInBundleInfo > AssetInfos = new Dictionary< string, AssetInBundleInfo >();
        #endregion

        #region Private Fields
        private Dictionary< string, BundleInfo > _bundleInfoByAssetNameCache = new Dictionary< string, BundleInfo >();
        #endregion

        #region Public Members
        /// <exception cref="BundleInfoDuplicateException"></exception>
        /// <exception cref="AssetInBundleInfoDuplicateException"></exception>
        public void AddManifestPart( AssetBundleManifest manifestPart, bool allowOverrides = false )
        {
            AddManifestPart( manifestPart.BundleInfos, manifestPart.AssetInfos, allowOverrides );
        }

        /// <exception cref="BundleInfoDuplicateException"></exception>
        /// <exception cref="AssetInBundleInfoDuplicateException"></exception>
        public void AddManifestPart( Dictionary< string, BundleInfo > bundleInfos, Dictionary< string, AssetInBundleInfo > assetInfos, bool allowOverrides = false )
        {
            foreach( var bundleInfo in bundleInfos )
            {
                if( !BundleInfos.ContainsKey( bundleInfo.Key ) )
                {
                    BundleInfos.Add( bundleInfo.Key, bundleInfo.Value );
                    continue;
                }

                if( allowOverrides )
                {
                    BundleInfos[ bundleInfo.Key ] = bundleInfo.Value;
                    continue;
                }

                throw new BundleInfoDuplicateException( bundleInfo.Key );
            }

            foreach( var assetInfo in assetInfos )
            {
                if( !AssetInfos.ContainsKey( assetInfo.Key ) )
                {
                    AssetInfos.Add( assetInfo.Key, assetInfo.Value );
                    continue;
                }

                if( allowOverrides )
                {
                    AssetInfos[ assetInfo.Key ] = assetInfo.Value;
                    continue;
                }

                throw new AssetInBundleInfoDuplicateException( assetInfo.Key );
            }

            RecalculateCache();
        }

        public bool ContainsAsset( string name )
        {
            return AssetInfos.ContainsKey( name );
        }

        public bool ContainsBundle( string name )
        {
            return BundleInfos.ContainsKey( name );
        }

        /// <summary>
        /// Gets the name of the bundle by asset.
        /// </summary>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public BundleInfo GetBundleByAssetName( string assetName )
        {
            if( string.IsNullOrEmpty( assetName ) )
            {
                throw new ArgumentException( "Asset name is empty" );
            }

            BundleInfo result = null;

            if( !_bundleInfoByAssetNameCache.TryGetValue( assetName, out result ) )
            {
                //                throw new ArgumentException( string.Format( "Asset {0} not't found in Manifest", assetName ) );
            }

            return result;
        }

        /// <summary>
        /// Gets the asset with dependencies.
        /// </summary>
        /// <param name="assetInBundleInfo">The asset information.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">AssetInBundleInfo is NULL</exception>
        public List< AssetInBundleInfo > GetAssetWithDependencies( AssetInBundleInfo assetInBundleInfo )
        {
            var result = new List< AssetInBundleInfo >();

            if( assetInBundleInfo == null )
            {
                throw new ArgumentException( "AssetInBundleInfo is NULL" );
            }

            result.Add( assetInBundleInfo );
            foreach( var dependencyName in assetInBundleInfo.Dependencies )
            {
                result.AddRange( GetAssetWithDependencies( AssetInfos[ dependencyName ] ) );
            }

            return result.Distinct().ToList();
        }

        /// <exception cref="ArgumentException"></exception>
        public List< AssetInBundleInfo > GetAssetInfosByTag( string tag )
        {
            List< AssetInBundleInfo > result = null;

            if( string.IsNullOrEmpty( tag ) )
            {
                throw new ArgumentException( "Tag name is empty" );
            }

            result = AssetInfos.Values.Where( assetInfo => assetInfo.GameAssetTypeTag == tag ).ToList();

            if( result == null )
            {
                throw new ArgumentException( string.Format( "Manifest don't contains any bindles for '{0}' GameAssetType.Tag ", tag ) );
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Restoreds this instance.
        /// </summary>
        public void RecalculateCache()
        {
            _bundleInfoByAssetNameCache.Clear();
            foreach( var bundleInfo in BundleInfos.Values )
            {
                foreach( var assetName in bundleInfo.AssetInfos )
                {
                    _bundleInfoByAssetNameCache.Add( assetName, bundleInfo );
                }
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine( "AssetBundleMainfest:" );

            foreach( var bundleInfo in BundleInfos )
            {
                stringBuilder.AppendLine( bundleInfo.ToString() );
            }

            return stringBuilder.ToString();
        }

        public bool IsAssetExistInBundles( string assetName )
        {
            if( _bundleInfoByAssetNameCache == null || _bundleInfoByAssetNameCache.Count == 0 )
            {
                RecalculateCache();
            }

            return GetBundleByAssetName( assetName ) != null;
        }
        #endregion
    }
}

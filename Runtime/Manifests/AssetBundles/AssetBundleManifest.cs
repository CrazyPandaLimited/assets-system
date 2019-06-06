#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class AssetBundleManifest
    {
        #region Public Fields

        public readonly Dictionary<string, BundleInfo> BundleInfos = new Dictionary<string, BundleInfo>();
        public readonly Dictionary<string, AssetInBundleInfo> AssetInfos = new Dictionary<string, AssetInBundleInfo>();

        #endregion

        #region Private Fields

        private Dictionary<string, BundleInfo> _infoIndex = new Dictionary<string, BundleInfo>();

        #endregion

        #region Public Members

        public void AddManifestPart(AssetBundleManifest manifestPart, bool allowOverrides = false)
        {
            AddManifestPart(manifestPart.BundleInfos, manifestPart.AssetInfos, allowOverrides);
        }

        public void AddManifestPart(Dictionary<string, BundleInfo> bundleInfos,
            Dictionary<string, AssetInBundleInfo> assetInfos, bool allowOverrides = false)
        {
            foreach (var bundleInfo in bundleInfos)
            {
                if (!BundleInfos.ContainsKey(bundleInfo.Key))
                {
                    BundleInfos.Add(bundleInfo.Key, bundleInfo.Value);
                    continue;
                }

                if (allowOverrides)
                {
                    BundleInfos[bundleInfo.Key] = bundleInfo.Value;
                    continue;
                }

                throw new BundleManifestInfoDuplicationException("Try to add double Bundle Info for " + bundleInfo.Key);
            }

            foreach (var assetInfo in assetInfos)
            {
                if (!AssetInfos.ContainsKey(assetInfo.Key))
                {
                    AssetInfos.Add(assetInfo.Key, assetInfo.Value);
                    continue;
                }

                if (allowOverrides)
                {
                    AssetInfos[assetInfo.Key] = assetInfo.Value;
                    continue;
                }

                throw new BundleManifestInfoDuplicationException("Try to add double Asset Info for " + assetInfo.Key);
            }

            Reindex();
        }


        /// <summary>
        /// Gets the name of the bundle by asset.
        /// </summary>
        /// <param name="assetName">Name of the asset.</param>
        /// <returns></returns>
        public BundleInfo GetBundleByAssetName(string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                throw new ArgumentException("Asset name is empty");
            }

            BundleInfo result = null;
            
            if (!_infoIndex.TryGetValue(assetName, out result))
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
        /// <exception cref="System.ArgumentException">AssetInBundleInfo is NULL</exception>
        public List<AssetInBundleInfo> GetAssetWithDependencies(AssetInBundleInfo assetInBundleInfo)
        {
            var result = new List<AssetInBundleInfo>();

            if (assetInBundleInfo == null)
            {
                throw new ArgumentException("AssetInBundleInfo is NULL");
            }

            result.Add(assetInBundleInfo);
            foreach (var dependencyName in assetInBundleInfo.Dependencies)
            {
                result.AddRange(GetAssetWithDependencies(AssetInfos[dependencyName]));
            }

            return result.Distinct().ToList();
        }

        public List<AssetInBundleInfo> GetAssetInfosByTag(string tag)
        {
            List<AssetInBundleInfo> result = null;

            if (string.IsNullOrEmpty(tag))
            {
                throw new ArgumentException("Tag name is empty");
            }

            result = AssetInfos.Values.Where(assetInfo => assetInfo.GameAssetTypeTag == tag).ToList();

            if (result == null)
            {
                throw new ArgumentException(string.Format("Manifest don't contains any bindles for '{0}' GameAssetType.Tag ",
                    tag));
            }

            return result.Distinct().ToList();
        }

        /// <summary>
        /// Restoreds this instance.
        /// </summary>
        public void Reindex()
        {
            _infoIndex.Clear();
            foreach (var bundleInfo in BundleInfos.Values)
            {
                foreach (var assetName in bundleInfo.AssetInfos)
                {
                    _infoIndex.Add(assetName, bundleInfo);
                }
            }
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("AssetBundleMainfest:");

            foreach (var bundleInfo in BundleInfos)
            {
                stringBuilder.AppendLine(bundleInfo.ToString());
            }

            return stringBuilder.ToString();
        }

        public bool IsResourceExistInBundles(string assetName)
        {
            if (_infoIndex == null || _infoIndex.Count == 0)
            {
                Reindex();
            }
            return GetBundleByAssetName(assetName) != null;
        }

        public List<BundleInfo> GetBundlesByAssetName(string resourceName)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
#endif
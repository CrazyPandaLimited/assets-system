#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System.Collections.Generic;
using System.Text;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
    public class AssetInBundleInfo
    {
        #region Properties
        /// <summary>
        /// Gets or sets the type of the game asset.
        /// </summary>
        /// <value>
        /// The type of the game asset.
        /// </value>
        public string GameAssetTypeTag { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the dependencies.
        /// </summary>
        /// <value>
        /// The dependencies on bundles.
        /// </value>
        public List< string > Dependencies { get; set; }

        /// <summary>
        /// Gets or sets the custom infos.
        /// </summary>
        /// <value>
        /// The custom infos.
        /// </value>
        public Dictionary< string, string > CustomInfo { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInBundleInfo"/> class.
        /// </summary>
        public AssetInBundleInfo()
        {
            Dependencies = new List< string >();
            CustomInfo = new Dictionary< string, string >();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AssetInBundleInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public AssetInBundleInfo( string name ) : this()
        {
            Name = name;
        }
        #endregion

        #region Public Members
        public string ToString( string prefix )
        {
            var sb = new StringBuilder();
            sb.AppendLine( string.Format( "{0}AssetInBundleInfo.Name = {1}", prefix, Name ) );
            sb.AppendLine( string.Format( "{0}AssetInBundleInfo.Dependencies.Count = {1}", prefix, Dependencies.Count ) );
            foreach( var assetInfo in Dependencies )
            {
                sb.AppendLine( assetInfo );
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return ToString( "" );
        }
        #endregion
    }
}
#endif
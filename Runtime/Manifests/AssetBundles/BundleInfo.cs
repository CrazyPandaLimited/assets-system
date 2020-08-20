using System.Collections.Generic;
using System.Text;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class BundleInfo
    {
        #region Properties
        /// <summary>
        /// Gets or sets the URI.
        /// </summary>
        /// <value>
        /// The URI.
        /// </value>
        public string Uri { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the ux.
        /// </summary>
        /// <value>
        /// The ux.
        /// </value>
        public uint Ux { get; set; }

        /// <summary>
        /// Get or sets CRC32 Hash
        /// </summary>
        /// <value>
        /// CRC32 Hash
        /// </value>
        public string CRC { get; set; }

        /// <summary>
        /// Get or sets Hash128 bundle hash
        /// A version hash. If this hash does not match the hash for the cached version of this asset bundle, the asset bundle will be redownloaded.
        /// </summary>
        /// <value>
        /// Hash128 Hash
        /// </value>
        public string Hash { get; set; }

        /// <summary>
        /// Gets or sets the asset infos.
        /// </summary>
        /// <value>
        /// The asset infos.
        /// </value>
        public List< string > AssetInfos { get; set; }

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
        /// Initializes a new instance of the <see cref="BundleInfo"/> class.
        /// </summary>
        public BundleInfo()
        {
            AssetInfos = new List< string >();
            CustomInfo = new Dictionary< string, string >();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleInfo"/> class.
        /// </summary>
        /// <param name="type">The type.</param>
        public BundleInfo( GameAssetType type, string name ) : this()
        {
            Name = name;
        }
        #endregion

        #region Public Members
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine( string.Format( "BundleInfo.Name = {0}", Name ) );
            sb.AppendLine( string.Format( "BundleInfo.Uri = {0}", Uri ) );
            sb.AppendLine( string.Format( "BundleInfo.Ux = {0}", Ux ) );
            sb.AppendLine( string.Format( "BundleInfo.AssetInfos.Count = {0}", AssetInfos.Count ) );

            var prefix = "    ";

            foreach( var assetInfo in AssetInfos )
            {
                sb.AppendLine( string.Format( "{0}{1}", prefix, assetInfo ) );
            }

            return sb.ToString();
        }
        #endregion
    }
}

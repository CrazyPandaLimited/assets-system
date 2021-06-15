namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetFileInfo
    {
        /// <summary>
        /// File name without extension
        /// </summary>
        public string name;

        /// <summary>
        /// File version
        /// </summary>
        public int version;

        /// <summary>
        /// File extension like .png
        /// </summary>
        public string ext;

        /// <summary>
        /// Get or sets CRC32 Hash
        /// </summary>
        public string CRC { get; set; }

        /// <summary>
        /// Return file name with extension and actual version
        /// </summary>
        /// <returns></returns>
        public string GetVersionedAssetName()
        {
            return name + "." + version + ext;
        }

        /// <summary>
        /// Return file name with extension and old version for cache clear purpose
        /// </summary>
        public string[ ] GetOldVersionedAssetName()
        {
            if( version == 0 )
            {
                return new string[ 0 ];
            }

            string[ ] result = new string[ version ];
            for( int i = 0; i < version; i++ )
            {
                result[ i ] = name + "." + i + ext;
            }

            return result;
        }
    }
}

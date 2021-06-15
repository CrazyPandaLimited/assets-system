namespace CrazyPanda.UnityCore.AssetsSystem
{
    /// <summary>
    /// Game asset type discryptor
    /// </summary>
    public class GameAssetType
    {
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAssetType"/> class.
        /// </summary>
        /// <param name="tag">The tag.</param>
        public GameAssetType( string tag )
        {
            Tag = tag;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameAssetType"/> class.
        /// </summary>
        public GameAssetType() : this( string.Empty )
        {
        }
    }
}

#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
namespace CrazyPanda.UnityCore.ResourcesSystem
{
    /// <summary>
    /// Game asset type discryptor
    /// </summary>
    public class GameAssetType
    {
        #region Properties
        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        /// <value>
        /// The tag.
        /// </value>
        public string Tag { get; set; }
        #endregion

        #region Constructors
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
        #endregion
    }
}
#endif
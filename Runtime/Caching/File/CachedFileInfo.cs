using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    [ Serializable ]
    public struct CachedFileInfo
    {
        #region Public Fields
        public string Key;
        public string Hash;
        #endregion

        #region Constructors
        public CachedFileInfo( string key, string hash )
        {
            Key = key;
            Hash = hash;
        }
        #endregion
    }
}

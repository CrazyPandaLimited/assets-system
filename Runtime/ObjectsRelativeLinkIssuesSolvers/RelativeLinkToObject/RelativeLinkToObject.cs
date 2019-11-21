using System;
using UnityEngine;

namespace Experimental.RelativeLinkToObject
{
    [ Serializable ]
    public class RelativeLinkToObject
    {
        #region Private Fields
        /// <summary>
        ///     Don't rename, is used in editor.
        /// </summary>
        [ SerializeField ] private string _assetPath;
        [ SerializeField ] private AssetType _assetType;
        [ SerializeField ] private bool _assetIntoResourcesFolder;
        #endregion

        #region Properties
#if UNITY_EDITOR
        /// <summary>
        /// Only for editor functionality
        /// </summary>
        public string AssetPath { get { return _assetPath; } set { _assetPath = value; } }

        public bool AssetIntoResourcesFolder { get { return _assetIntoResourcesFolder; } set { _assetIntoResourcesFolder = value; } }
        public AssetType AssetType { get { return _assetType; } set { _assetType = value; } }

#else
        public string AssetPath =>  _assetPath;
#endif
        #endregion
    }

    public enum AssetType
    {
        Object = 0,
        GameObject,
        Texture,
        Material
    }
}

using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetTypeMismatchException : AssetsSystemException
    {
        #region Properties
        public string AssetKey { get; }
        public Type RequestedType { get; }
        public Type ActualType { get; }
        #endregion

        #region Constructors
        public AssetTypeMismatchException( string assetKey, Type requestedType, Type actualType )
            : base( $"Requested asset {assetKey} as type {requestedType} but actual type is {actualType}" )
        {
            AssetKey = assetKey;
            RequestedType = requestedType;
            ActualType = actualType;
        }
        #endregion
    }
}

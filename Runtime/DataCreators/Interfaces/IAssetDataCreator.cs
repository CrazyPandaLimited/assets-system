using System;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public interface IAssetDataCreator
    {
        #region Public Members
        bool Supports( Type requestedAssetType );

        //object Create( byte[ ] data );
        TAssetType Create< TAssetType >( byte[ ] data ) where TAssetType : class;
        Object Create( byte[ ] data, Type type );
        #endregion
    }
}

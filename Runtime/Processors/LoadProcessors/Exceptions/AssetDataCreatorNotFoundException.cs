using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetDataCreatorNotFoundException : AbstractProcessorException
    {
        #region Properties
        public Type AssetType { get; }
        #endregion

        #region Constructors
        public AssetDataCreatorNotFoundException( Type assetType, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"IAssetDataCreator not found for type {assetType}", flowNode, messageHeader, messageBody )
        {
            AssetType = assetType;
        }
        #endregion
    }
}

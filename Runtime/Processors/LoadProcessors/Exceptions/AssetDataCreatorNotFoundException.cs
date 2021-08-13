using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetDataCreatorNotFoundException : AbstractProcessorException
    {
        public Type AssetType { get; }

        public AssetDataCreatorNotFoundException( Type assetType, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"IAssetDataCreator not found for type {assetType}", flowNode, messageBody )
        {
            AssetType = assetType;
        }
    }
}

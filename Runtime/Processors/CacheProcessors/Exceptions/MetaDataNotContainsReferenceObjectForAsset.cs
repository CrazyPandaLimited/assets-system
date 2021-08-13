using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class MetaDataNotContainsReferenceObjectForAsset : AbstractProcessorException
    {
        public MetaDataNotContainsReferenceObjectForAsset( IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"Metadata does not contain key {MetaDataReservedKeys.OWNER_REFERENCE_RESERVED_KEY}", flowNode, messageBody )
        {
        }
    }
}

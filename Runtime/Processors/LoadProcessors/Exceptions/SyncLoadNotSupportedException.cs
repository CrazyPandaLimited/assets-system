using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class SyncLoadNotSupportedException : AbstractProcessorException
    {
        public SyncLoadNotSupportedException( IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( "Sync load not supported", flowNode, messageBody )
        {
        }
    }
}

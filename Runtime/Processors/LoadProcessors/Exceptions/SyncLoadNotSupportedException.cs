using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class SyncLoadNotSupportedException : AbstractProcessorException
    {
        #region Constructors
        public SyncLoadNotSupportedException( IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( "Sync load not supported", flowNode, messageHeader, messageBody )
        {
        }
        #endregion
    }
}

using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{

    public class CachedObjectOverrideException : AbstractProcessorException
    {
        public CachedObjectOverrideException( IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( "Can't override cached object", flowNode, messageHeader, messageBody )
        {
        }
    }
}

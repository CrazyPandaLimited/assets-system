using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class UnhandledProcessorException : AbstractProcessorException
    {
        public UnhandledProcessorException( IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody, Exception innerException )
            : base( "Unhandled excepton in Processor. See InnerException for details", flowNode, messageHeader, messageBody, innerException )
        {
        }
    }
}

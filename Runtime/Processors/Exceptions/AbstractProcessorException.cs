using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class AbstractProcessorException : AssetsSystemException
    {
        public IFlowNode FlowNode { get; }
        public IMessageBody MessageBody { get; }

        protected AbstractProcessorException( string message, IFlowNode flowNode, IMessageBody messageBody )
            : this( message, flowNode, messageBody, null )
        {
        }

        protected AbstractProcessorException( string message, IFlowNode flowNode, IMessageBody messageBody, Exception innerException )
            : base( message + $" Processor: {flowNode}. Body: {messageBody}", innerException )
        {
            FlowNode = flowNode;
            MessageBody = messageBody;
        }
    }
}

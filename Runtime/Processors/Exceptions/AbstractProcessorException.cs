using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public abstract class AbstractProcessorException : AssetsSystemException
    {
        #region Properties
        public IFlowNode FlowNode { get; }
        public MessageHeader MessageHeader { get; }
        public IMessageBody MessageBody { get; }
        #endregion

        #region Constructors
        protected AbstractProcessorException( string message, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : this( message, flowNode, messageHeader, messageBody, null )
        {
        }

        protected AbstractProcessorException( string message, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody, Exception innerException )
            : base( message + $" Processor: {flowNode}. Header: {messageHeader}. Body: {messageBody}", innerException )
        {
            FlowNode = flowNode;
            MessageHeader = messageHeader;
            MessageBody = messageBody;
        }
        #endregion
    }
}

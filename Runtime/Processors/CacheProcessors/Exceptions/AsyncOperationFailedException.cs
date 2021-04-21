using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AsyncOperationFailedException : AbstractProcessorException
    {
        public AsyncOperationFailedException( AsyncOperation asyncOperation, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"Async operation failed - {asyncOperation.ToString()}", flowNode, messageHeader, messageBody )
        {
        }
    }
}
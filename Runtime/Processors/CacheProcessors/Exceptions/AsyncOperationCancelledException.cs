using CrazyPanda.UnityCore.MessagesFlow;
using UnityEngine;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AsyncOperationCancelledException : AbstractProcessorException
    {
        public AsyncOperationCancelledException( AsyncOperation asyncOperation, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody )
            : base( $"Async operation was cancelled - {asyncOperation.ToString()}", flowNode, messageHeader, messageBody )
        {
        }
    }
}
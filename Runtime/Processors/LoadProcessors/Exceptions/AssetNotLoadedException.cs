using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class AssetNotLoadedException : AbstractProcessorException
    {
        public AssetNotLoadedException( string message, IFlowNode flowNode, MessageHeader messageHeader, IMessageBody messageBody ) 
            : base( message, flowNode, messageBody )
        {
        }
    }
}

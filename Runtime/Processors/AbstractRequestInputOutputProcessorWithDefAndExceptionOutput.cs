using System.Collections.Generic;
using System.Runtime.CompilerServices;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputOutputProcessorWithDefAndExceptionOutput< TIn, TOut, TError > : AbstractRequestInputOutputProcessor< TIn, TOut >
        where TIn : IMessageBody
        where TOut : IMessageBody
        where TError : IMessageBody
    {
        private BaseOutput< TError > _excHandler = new BaseOutput< TError >( OutputHandlingType.Optional );

        public IOutputNode< TError > ExceptionOutput => _excHandler;

        protected void SendException( MessageHeader header, TError body )
        {
            _excHandler.ProcessMessage( header, body );
        }
    }
}

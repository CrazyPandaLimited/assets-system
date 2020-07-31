using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputProcessor< T > : AbstractRequestProcessor
        where T : IMessageBody
    {
        private BaseInput< T > _inHandler;

        public IInputNode< T > DefaultInput => _inHandler;

        public AbstractRequestInputProcessor()
        {
            _inHandler = new BaseInput< T >( OnMessageReceived );
        }

        protected abstract void InternalProcessMessage( MessageHeader header, T body );

        private void OnMessageReceived( MessageHeader header, T body )
        {
            if( Status == FlowNodeStatus.Failed || header.CancellationToken.IsCancellationRequested  )
            {
                return;
            }

            try
            {
                InternalProcessMessage( header, body );
            }
            catch( Exception e )
            {
                ProcessException( header, body, e );
            }
        }
    }
}

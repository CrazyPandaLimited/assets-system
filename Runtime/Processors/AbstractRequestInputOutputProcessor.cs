using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public abstract class AbstractRequestInputOutputProcessor< TIn, TOut > : AbstractRequestInputProcessor< TIn >
        where TIn : IMessageBody
        where TOut : IMessageBody
    {
        private BaseOutput< TOut > _outHandler = new BaseOutput< TOut >();

        public IOutputNode< TOut > DefaultOutput => _outHandler;

        protected void SendOutput( MessageHeader header, TOut body )
        {
            _outHandler.ProcessMessage( header, body );
        }
    }
}

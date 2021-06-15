using System;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestQueueEntry
    {
        public MessageHeader Header { get; protected set; }
        public Object Body { get; protected set; }
        public Action ContinueProcessingHandler { get; protected set; }

        public int Priority { get; protected set; }

        public RequestQueueEntry( MessageHeader header, Object body, int priority, Action continueProcessingHandler )
        {
            Header = header;
            Body = body;
            Priority = priority;
            ContinueProcessingHandler = continueProcessingHandler;
        }
    }
}

using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueueEndPoint< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType > where TBodyType : IMessageBody
    {
        protected IRequestsQueue _requestsQueue;

        public RequestsQueueEndPoint( IRequestsQueue requestsQueue )
        {
            _requestsQueue = requestsQueue;
        }

        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            _requestsQueue.RequestReachedQueuedEndPoint( header );
            SendOutput( header, body );
        }
    }
}

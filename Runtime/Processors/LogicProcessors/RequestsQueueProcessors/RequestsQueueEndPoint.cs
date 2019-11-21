using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueueEndPoint< TBodyType > : AbstractRequestInputOutputProcessorWithDefaultOutput< TBodyType, TBodyType > where TBodyType : IMessageBody
    {
        #region Protected Fields
        protected IRequestsQueue _requestsQueue;
        #endregion

        #region Constructors
        public RequestsQueueEndPoint( IRequestsQueue requestsQueue )
        {
            _requestsQueue = requestsQueue;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            _requestsQueue.RequestReachedQueuedEndPoint( header );
            _defaultConnection.ProcessMessage( header, body );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}

using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueueEndPoint< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType > where TBodyType : IMessageBody
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
        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            _requestsQueue.RequestReachedQueuedEndPoint( header );
            SendOutput( header, body );
        }
        #endregion
    }
}

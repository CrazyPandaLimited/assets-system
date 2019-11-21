using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueueProcessor< TBodyType > : AbstractRequestInputOutputProcessorWithDefaultOutput< TBodyType, TBodyType > where TBodyType : IMessageBody
    {
        #region Protected Fields
        protected IRequestsQueue _requestsQueue;
        #endregion

        #region Constructors
        public RequestsQueueProcessor( IRequestsQueue requestsQueue )
        {
            _requestsQueue = requestsQueue;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            int priority = 0;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.QUEUE_PRIORITY ) )
            {
                priority = header.MetaData.GetMeta< int >( MetaDataReservedKeys.QUEUE_PRIORITY );
            }
            _requestsQueue.Add( new RequestQueueEntry( header, body,priority, () => { _defaultConnection.ProcessMessage( header, body ); } ) );

            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}

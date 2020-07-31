using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class RequestsQueueProcessor< TBodyType > : AbstractRequestInputOutputProcessor< TBodyType, TBodyType >
        where TBodyType : IMessageBody
    {
        protected IRequestsQueue _requestsQueue;

        public RequestsQueueProcessor( IRequestsQueue requestsQueue )
        {
            _requestsQueue = requestsQueue;
        }

        protected override void InternalProcessMessage( MessageHeader header, TBodyType body )
        {
            int priority = 0;
            if( header.MetaData.IsMetaExist( MetaDataReservedKeys.QUEUE_PRIORITY ) )
            {
                priority = header.MetaData.GetMeta< int >( MetaDataReservedKeys.QUEUE_PRIORITY );
            }

            _requestsQueue.Add( new RequestQueueEntry( header, body, priority, () => { SendOutput( header, body ); } ) );
        }
    }
}

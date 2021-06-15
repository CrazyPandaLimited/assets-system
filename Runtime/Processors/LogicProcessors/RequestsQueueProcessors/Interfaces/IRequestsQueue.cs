using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public interface IRequestsQueue
    {
        void Add( RequestQueueEntry entry );
        void RequestReachedQueuedEndPoint( MessageHeader header );
    }
}

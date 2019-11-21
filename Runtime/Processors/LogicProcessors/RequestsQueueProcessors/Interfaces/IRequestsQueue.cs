using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public interface IRequestsQueue
    {
        #region Public Members
        void Add( RequestQueueEntry entry );
        void RequestReachedQueuedEndPoint( MessageHeader header );
        #endregion
    }
}

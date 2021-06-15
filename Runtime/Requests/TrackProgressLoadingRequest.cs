using CrazyPanda.UnityCore.PandaTasks.Progress;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class TrackProgressLoadingRequest : IMessageBody
    {
        public IProgressTracker< float > ProgressTracker { get; protected set; }

        protected TrackProgressLoadingRequest( IProgressTracker< float > progressTracker )
        {
            ProgressTracker = progressTracker;
        }

        public override string ToString()
        {
            return $"TrackProgressLoadingRequest progress:{ProgressTracker.Progress} {base.ToString()}";
        }
    }
}

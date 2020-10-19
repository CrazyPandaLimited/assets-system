using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public sealed class AssetsStorage : BaseAssetsStorage
    {
        public AssetsStorage( RequestToPromiseMap requestToPromiseMap ) : base( requestToPromiseMap )
        {
        }

        public new void LinkTo( IInputNode< UrlLoadingRequest > input )
        {
            base.LinkTo( input );
        }
    }
}
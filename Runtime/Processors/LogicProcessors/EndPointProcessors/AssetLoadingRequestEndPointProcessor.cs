using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetLoadingRequestEndPointProcessor< T > : AbstractRequestInputProcessor< AssetLoadingRequest< T > >
    {
        #region Private Fields
        private RequestToPromiseMap _requestToPromiseMap;
        #endregion

        #region Constructors
        public AssetLoadingRequestEndPointProcessor( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( header.Exceptions != null && body.Asset == null )
            {
                _requestToPromiseMap.Get( header.Id ).SetError( header.Exceptions );
                return FlowMessageStatus.Accepted;
            }

            _requestToPromiseMap.Get( header.Id ).SetValue( body.Asset );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}

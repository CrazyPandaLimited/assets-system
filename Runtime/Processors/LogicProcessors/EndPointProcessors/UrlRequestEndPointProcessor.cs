using UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class UrlRequestEndPointProcessor : AbstractRequestInputProcessor< UrlLoadingRequest >
    {
        #region Private Fields
        private RequestToPromiseMap _requestToPromiseMap;
        #endregion

        #region Constructors
        public UrlRequestEndPointProcessor( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
        }
        #endregion

        #region Protected Members
        protected override FlowMessageStatus InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.Exceptions != null )
            {
                _requestToPromiseMap.Get( header.Id ).SetError( header.Exceptions );
                return FlowMessageStatus.Accepted;
            }

            _requestToPromiseMap.Get( header.Id ).SetValue( body.Url );
            return FlowMessageStatus.Accepted;
        }
        #endregion
    }
}

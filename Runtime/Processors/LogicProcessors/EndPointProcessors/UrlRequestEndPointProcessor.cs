using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class UrlRequestEndPointProcessor : AbstractRequestInputProcessor< UrlLoadingRequest >
    {
        private RequestToPromiseMap _requestToPromiseMap;

        public UrlRequestEndPointProcessor( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
        }

        protected override void InternalProcessMessage( MessageHeader header, UrlLoadingRequest body )
        {
            if( header.Exceptions != null )
            {
                _requestToPromiseMap.Get( header.Id ).SetError( header.Exceptions );
                return;
            }

            _requestToPromiseMap.Get( header.Id ).SetValue( body.Url );
        }
    }
}

using System;
using System.Collections.Generic;
using CrazyPanda.UnityCore.MessagesFlow;

namespace CrazyPanda.UnityCore.AssetsSystem.Processors
{
    public class AssetLoadingRequestEndPointProcessor< T > : AbstractRequestInputProcessor< AssetLoadingRequest< T > >
    {
        private RequestToPromiseMap _requestToPromiseMap;

        public AssetLoadingRequestEndPointProcessor( RequestToPromiseMap requestToPromiseMap )
        {
            _requestToPromiseMap = requestToPromiseMap;
        }

        protected override void InternalProcessMessage( MessageHeader header, AssetLoadingRequest< T > body )
        {
            if( header.Exceptions != null && body.Asset == null )
            {
                _requestToPromiseMap.Get( header.Id ).SetError( header.Exceptions );
                return;
            }

            _requestToPromiseMap.Get( header.Id ).SetValue( body.Asset );
        }
    }
}

using System.Collections.Generic;
using CrazyPanda.UnityCore.PandaTasks;

namespace CrazyPanda.UnityCore.AssetsSystem
{
    public class RequestToPromiseMap
    {
        protected Dictionary< string, UnsafeCompletionSource< object > > _map = new Dictionary< string, UnsafeCompletionSource< object > >();

        public void Add( string id, UnsafeCompletionSource< object > promise )
        {
            _map.Add( id, promise );
        }

        public bool Has( string id ) => _map.ContainsKey( id );

        public UnsafeCompletionSource < object > Get( string id )
        {
            var task = _map[ id ];

            _map.Remove( id );

            return task;
        }


        public List< KeyValuePair< string, IPandaTask< object > > > AllPromises()
        {
            var result = new List< KeyValuePair< string, IPandaTask< object > > >();
            foreach( var completionSource in _map )
            {
                result.Add( new KeyValuePair< string, IPandaTask< object > >(completionSource.Key, completionSource.Value.ResultTask) );
            }

            return result;
        }
    }
}

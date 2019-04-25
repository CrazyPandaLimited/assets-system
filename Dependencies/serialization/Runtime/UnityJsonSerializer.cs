#if CRAZYPANDA_UNITYCORE_SERIALIZATION_JSON
using System;
using System.Text;
using CrazyPanda.UnityCore.Utils;
using UnityEngine;

namespace CrazyPanda.UnityCore.Serialization
{
    public class UnityJsonSerializer : ISerializer
    {
        #region Private Fields
        private Settings _settings;
        private bool _prettyPrint;
        #endregion

        #region Constructors
        public UnityJsonSerializer( Settings settings = null, bool prettyPrint = false )
        {
            _settings = settings ?? new Settings();
            _prettyPrint = prettyPrint;
        }
        #endregion

        #region Public Members
        public byte[ ] Serialize( object obj )
        {
            if( obj == null )
            {
                throw new SerializationException( "Object to serialize is NULL" );
            }

            var jsonString = string.Empty;
            try
            {
                jsonString = JsonUtility.ToJson( obj, _prettyPrint );
            }
            catch( Exception exception )
            {
                throw new SerializationException( exception, "Failed to Serialize {0}", obj );
            }

            return _settings.Encoding.GetBytes( jsonString );
        }

        public T Deserialize< T >( byte[ ] data ) where T : class
        {
            var jsonString = _settings.Encoding.GetString( data );
            T obj = null;
            try
            {
                obj = JsonUtility.FromJson< T >( jsonString );
            }
            catch( Exception exception )
            {
                throw new SerializationException( exception, "Failed to deserialize {0} to object of type {1}", jsonString, typeof( T ).CSharpName() );
            }
            return obj;
        }
        #endregion

        #region Nested Types
        public class Settings
        {
            #region Public Fields
            public Encoding Encoding;
            #endregion

            #region Constructors
            public Settings( Encoding encoding = null )
            {
                Encoding = encoding ?? Encoding.UTF8;
            }
            #endregion
        }
        #endregion
    }
}
#endif
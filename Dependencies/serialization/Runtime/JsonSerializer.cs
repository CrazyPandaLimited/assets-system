#if CRAZYPANDA_UNITYCORE_SERIALIZATION_JSON && CRAZYPANDA_UNITYCORE_SERIALIZATION_NEWTONSOFT_JSON
using CrazyPanda.UnityCore.Utils;
using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Newtonsoft.Json;

namespace CrazyPanda.UnityCore.Serialization
{
	public class JsonSerializer : ISerializer
	{
		#region Private Fields
		private readonly Encoding _encoding;
		private readonly JsonSerializerSettings _settings;
	    private Newtonsoft.Json.JsonSerializer _jsonSerializer;
		#endregion

		#region Constructors
		public JsonSerializer( JsonSerializerSettings settings, Encoding encoding = null )
		{
			_encoding = encoding ?? Encoding.UTF8;
			_settings = settings;
		}

		public JsonSerializer( Encoding encoding = null )
		{
			_encoding = encoding ?? Encoding.UTF8;
			_settings = new JsonSerializerSettings();
			_settings.Formatting = Formatting.Indented;
		}
		#endregion

		#region Public Members
		public byte[ ] Serialize( object obj )
		{
			if( obj == null )
				throw new SerializationException( "Object to serialize is NULL" );

			var jsonString = string.Empty;
			try
			{
				jsonString = SerializeString( obj );
			}
			catch( Exception exception )
			{
				throw new SerializationException( exception, "Failed to serialize {0}", obj );
			}

			return _encoding.GetBytes( jsonString );
		}

		public string SerializeString( object obj )
		{
			var serialized = JsonConvert.SerializeObject( obj, _settings );
			return serialized;
		}

		public object Deserialize( byte[ ] data )
		{
			return JsonConvert.DeserializeObject( _encoding.GetString( data ) );
		}

		public T Deserialize< T >( byte[ ] data ) where T : class
		{
			var jsonString = _encoding.GetString( data );
			T obj = null;
			try
			{
				obj = DeserializeString< T >( jsonString );
			}
			catch( Exception exception )
			{
				throw new SerializationException( exception, "Failed to deserialize {0} to object of type {1}", jsonString, typeof( T ).CSharpName() );
			}

			return obj;
		}

		public T DeserializeString< T >( string data ) where T : class
		{
			return JsonConvert.DeserializeObject< T >( data, _settings );
		}

	    public T Deserialize < T >( JToken data ) where T : class
	    {
	        if( _jsonSerializer == null )
	        {
	            _jsonSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault( _settings );
	        }
	        return data.ToObject< T >( _jsonSerializer );
	    }

		public void Populate( string data, object obj )
		{
			JsonConvert.PopulateObject( data, obj, _settings );
		}

		public void Populate( JToken data, object obj )
		{
		    if( _jsonSerializer == null )
		    {
		        _jsonSerializer = Newtonsoft.Json.JsonSerializer.CreateDefault( _settings );
		    }
            using ( var sr = data.CreateReader() )
			{
			    _jsonSerializer.Populate( sr, obj );
			}
		}
		#endregion
	}
}

#endif

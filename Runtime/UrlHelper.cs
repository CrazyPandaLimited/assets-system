#if CRAZYPANDA_UNITYCORE_RESOURCESYSTEM
using System;

namespace CrazyPanda.UnityCore.ResourcesSystem
{
	public static class UrlHelper
	{
		public const string URI_DELIMETER = "://";
		public static string GetResourceName( string url )
		{	
			int index = url.IndexOf( URI_DELIMETER, StringComparison.InvariantCulture );
			if( index <= 0 )
			{
				return url;
			}
			return url.Substring(index + URI_DELIMETER.Length);
		}

		public static string GetUriWithPrefix(string prefix, string uri)
		{
			return prefix + URI_DELIMETER + uri;
		}
	}
}
#endif
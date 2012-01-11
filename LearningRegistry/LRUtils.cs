using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LearningRegistry
{
	internal static class LRUtils
	{
		private static JavaScriptSerializer _serializer = new JavaScriptSerializer();
		public static JavaScriptSerializer GetSerializer() { return _serializer; }
		
		public const string ISO_8061_FORMAT = "yyyy-MM-ddThh:mm:ssZ";
		
		public static HttpWebRequest CreateHttpRequest(Uri baseUri, string action)
		{
			return (HttpWebRequest)WebRequest.Create(new Uri(baseUri, action));
		}
		
		public static string BuildQueryString(Dictionary<string,string> args)
		{
			string qs = "";
			if(args != null && args.Count > 0)
			{
				qs += "?";
				foreach (var entry in args)
					qs += String.Format("{0}={1}&", entry.Key, entry.Value);
				qs = qs.TrimEnd('&');
			}
			return qs;
		}
		
		
	}
	
	
}


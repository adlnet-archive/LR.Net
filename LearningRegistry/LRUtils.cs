using System;
using System.Net;
using System.Reflection;
using System.Collections.Generic;

namespace LearningRegistry
{
	internal static class LRUtils
	{
		public const string ISO_8061_FORMAT = "yyyy-MM-ddThh:mm:ssZ";
		
		public static HttpWebRequest CreateHttpRequest(string baseUri, string action)
		{
			return (HttpWebRequest)WebRequest.Create(System.IO.Path.Combine(baseUri, action));
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
		
		public static void AssertVocabTerm(string s, Type t)
		{
			FieldInfo[] infos = t.GetFields(BindingFlags.Public | BindingFlags.Static);
			foreach(FieldInfo info in infos)
			{
				try 
				{
					string vocabVal = (string)info.GetRawConstantValue();
					if(!String.IsNullOrEmpty(vocabVal) && vocabVal.Equals(s))
						return;
				}
				catch
				{
					continue;
				}
			}
			throw new Exception(s+" is not in the vocabulary '"+t.Name+"'");
		}
	}
	
	
}


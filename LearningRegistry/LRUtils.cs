using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LearningRegistry
{
	
	internal static class LRUtils
	{
		public static class Routes
		{
			public readonly static string Publish = "publish";
			public readonly static string Obtain = "obtain";
			public readonly static string Harvest = "harvest";
		}
		
		private static JavaScriptSerializer _serializer = new JavaScriptSerializer();
		public static JavaScriptSerializer GetSerializer() { return _serializer; }
		
		public const string ISO_8061_FORMAT = "yyyy-MM-ddThh:mm:ssZ";
		
		public static HttpWebRequest CreateHttpRequest(Uri baseUri, string action)
		{
			return (HttpWebRequest)WebRequest.Create(new Uri(baseUri, action));
		}
		
		internal static ObtainResult Obtain(Uri baseUri, string resourceId, Dictionary<string, string> args = null)
		{
			if(args == null)
				args = new Dictionary<string, string>();
			args["request_id"] = resourceId;
			
			string jsonData = getDataFromUri(baseUri, Routes.Obtain, args);
			ObtainResult result = _serializer.Deserialize<ObtainResult>(jsonData);
			result.BaseUri = baseUri;
			result.Args = args;
			result.ResourceId = resourceId;
			return result;
		}
		
		internal static HarvestResult Harvest(Uri baseUri, string action, Dictionary<string, string> args)
		{
			string path = String.Format("{0}/{1}", Routes.Harvest, action);
			string jsonData = getDataFromUri(baseUri, path, args);
			HarvestResult results = new HarvestResult();
			switch (action)
			{
				case Harvester.HarvesterActions.GetRecord:
					results = _serializer.Deserialize<GetRecordHarvestResult>(jsonData);
					break;
				case Harvester.HarvesterActions.ListRecords:
					results = _serializer.Deserialize<ListRecordsHarvestResult>(jsonData);
					break;
				case Harvester.HarvesterActions.ListIdentifiers:
					results = _serializer.Deserialize<ListIdentifiersHarvestResult>(jsonData);
					break;
				case Harvester.HarvesterActions.ListMetadataFormats:
					results = _serializer.Deserialize<ListMetadataFormatsHarvestResult>(jsonData);
					break;
				default:
					throw new System.ArgumentException("Invalid harvester action: " + action);
			}
			
			if(!results.OK)
				throw new WebException("Error while calling getRecord: "+results.error, WebExceptionStatus.UnknownError);
			
			results.Action = action;
			results.Args = args;
			results.BaseUri = baseUri;
			return results;
		}
		
		internal static string BuildQueryString(Dictionary<string,string> args)
		{
			string qs = "";
			if(args != null && args.Count > 0)
			{
				foreach (var entry in args)
					qs += String.Format("{0}={1}&", entry.Key, entry.Value);
				qs = qs.TrimEnd('&');
			}
			return qs;
		}
		
		private static string getDataFromUri(Uri baseUri, string action, Dictionary<string,string> args)
		{
			UriBuilder urib = new UriBuilder(baseUri);
			urib.Path = action;
			urib.Query = LRUtils.BuildQueryString(args);
			
			Console.WriteLine(urib.Uri.AbsoluteUri);
			WebClient wc = new WebClient();
			return wc.DownloadString(urib.Uri.AbsoluteUri);
		}
	}
	
	
}


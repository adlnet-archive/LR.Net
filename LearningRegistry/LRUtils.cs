using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Text;
using System.IO;

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

        private static UTF8Encoding _encoder = new UTF8Encoding();
        public static UTF8Encoding GetEncoder() { return _encoder; }

		public const string ISO_8061_FORMAT = "yyyy-MM-ddThh:mm:ssZ";
		
		internal static string HttpPostRequest(Uri baseUri, string action, string data, string contentType = "application/json", string username = "", string password = "")
		{
            UriBuilder urib = new UriBuilder(baseUri);
            urib.Path = action;

            byte[] postBody = _encoder.GetBytes(data);
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(urib.Uri);
            req.Method = "POST";
            req.ContentType = contentType;
            req.ContentLength = postBody.Length;
            if (!String.IsNullOrEmpty(username))
            {
                req.UseDefaultCredentials = false;
                NetworkCredential creds = new NetworkCredential();
                creds.UserName = username;
                creds.Password = password;
                req.Credentials = creds;
            }

            using (Stream dataStream = req.GetRequestStream())
                dataStream.Write(postBody, 0, postBody.Length);

            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            string responseData;
            using (Stream dataStream = response.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(dataStream, _encoder))
                {
                    responseData = reader.ReadToEnd();   
                }
            }

            return responseData;
		}

        internal static ObtainResult Obtain(Uri baseUri, string resourceId, Dictionary<string, object> args = null, string username = "", string password = "")
        {
            List<string> ids = new List<string>();
            ids.Add(resourceId);
            return Obtain(baseUri, ids, args, username, password);
        }

		internal static ObtainResult Obtain(Uri baseUri, List<string> resourceIds, Dictionary<string, object> args = null, string username = "", string password = "")
		{
			if(args == null)
				args = new Dictionary<string, object>();
            if (resourceIds.Count > 0) //ResumableResult leaves this empty b/c of resumption token
                args["request_IDs"] = resourceIds;
			
			string jsonData = getDataFromUri(baseUri, Routes.Obtain, args, username, password);
			ObtainResult result = _serializer.Deserialize<ObtainResult>(jsonData);
			result.BaseUri = baseUri;
            result.HttpUsername = username;
            result.HttpPassword = password;
			return result;
		}

		internal static HarvestResult Harvest(Uri baseUri, string action, Dictionary<string, object> args, string username = "", string password = "")
		{
			string path = String.Format("{0}/{1}", Routes.Harvest, action);
			string jsonData = getDataFromUri(baseUri, path, args, username, password);
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
			results.BaseUri = baseUri;
            results.HttpUsername = username;
            results.HttpPassword = password;
			return results;
		}

        internal static SliceResult Slice(Uri baseUri, Dictionary<string, object> args)
        {
            UriBuilder urib = new UriBuilder(baseUri);
            urib.Path = "slice";
            urib.Query = BuildQueryString(args);

            string jsonData = new WebClient().DownloadString(urib.Uri);

            var result = _serializer.Deserialize<SliceResult>(jsonData);
            result.BaseUri = baseUri;
            return result;
        }
		
		internal static string BuildQueryString(Dictionary<string,object> args)
		{
			string qs = "";
			if(args != null && args.Count > 0)
			{
				foreach (var entry in args)
					qs += String.Format("{0}={1}&", entry.Key, entry.Value.ToString());
				qs = qs.TrimEnd('&');
			}
			return qs;
		}
		
		private static string getDataFromUri(Uri baseUri, string action, Dictionary<string,object> args, string username = "", string password = "")
		{
            string postContent = (args == null) ? "{}" : _serializer.Serialize(args);
            return HttpPostRequest(baseUri, action, postContent, "application/json", username, password);
		}
	}
	
	
}


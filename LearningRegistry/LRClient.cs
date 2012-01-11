using System;
using System.Collections.Generic;
using LearningRegistry.RDDD;
using System.Net;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;

namespace LearningRegistry
{
	
	public enum ErrorCode
	{
		OK = 0,
	}
	
	public class LRClient
	{
		
		private Uri _baseUri;
		public Uri BaseUri
		{ 
			get
			{
				return _baseUri;
			}
			set 
			{
				_baseUri = value;
			}
		}
		
		private JavaScriptSerializer _serializer;
		private UTF8Encoding _encoder;
		private Harvester _harvester;
		public Harvester Harvester 
		{
			get { return _harvester; }
		}
		
		public LRClient()
		{
			_serializer = LRUtils.GetSerializer();
			_encoder = new UTF8Encoding();
			_harvester = new Harvester();
		}
		
		public LRClient(string baseUri)
		{
			_baseUri = new Uri(baseUri);
			_serializer = new JavaScriptSerializer();
			_encoder = new UTF8Encoding();
			_harvester = new Harvester(_baseUri);
		}
		
		public PublishResponse Publish(lr_Envelope docs)
		{
			byte[] data = _encoder.GetBytes(docs.Serialize());
			HttpWebRequest request = LRUtils.CreateHttpRequest(BaseUri,LRUtils.Routes.Publish);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = data.GetLength(0);
			using (Stream dataStream = request.GetRequestStream())
				dataStream.Write(data, 0, data.Length);
			
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			byte[] responseData = new byte[response.ContentLength];
			using (Stream dataStream = response.GetResponseStream())
				dataStream.Read(responseData, 0, responseData.Length);
			var responseObject = _serializer.Deserialize<PublishResponse>(_encoder.GetString(responseData));
			return responseObject;
		}
		
		public lr_document ObtainDocByID(string docId)
		{
			Dictionary<string, string> args = new Dictionary<string, string>();
			args["by_doc_ID"] = "true";
			
			var result = LRUtils.Obtain(_baseUri, docId, args);
			
			if(result.documents.Count < 1)
				throw new Exception("Document with id "+docId+" does not exist.");
			
			return result.documents[0].document[0];
		}
		
		public ObtainResult ObtainDocsByResourceLocator(string locator, bool ids_only = false)
		{
			Dictionary<string, string> args = new Dictionary<string, string>();
			
			if(ids_only)
			{
				args["ids_only"] = "true";
				args["by_doc_ID"] = "true"; //we only want doc ids
			}
			
			return LRUtils.Obtain(_baseUri, locator, args);
		}

	}
}


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
		public static class Routes
		{
			public readonly static string Publish = "publish";
			public readonly static string Obtain = "obtain";
		}
		private string _baseUri;
		public string BaseUri
		{ 
			get
			{
				return _baseUri;
			}
			set 
			{
				_baseUri = value;
				_harvester.BaseUri = value;
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
			_baseUri = baseUri;
			_serializer = new JavaScriptSerializer();
			_encoder = new UTF8Encoding();
			_harvester = new Harvester(baseUri);
		}
		
		public PublishResponse Publish(lr_Envelope docs)
		{
			byte[] data = _encoder.GetBytes(docs.Serialize());
			HttpWebRequest request = LRUtils.CreateHttpRequest(BaseUri,Routes.Publish);
			request.Method = "POST";
			request.ContentType = "application/json";
			request.ContentLength = data.GetLength(0);
			using (Stream dataStream = request.GetRequestStream())
				dataStream.Write(data, 0, data.Length);
			
			HttpWebResponse response = (HttpWebResponse)request.GetResponse();
			byte[] responseData = new byte[response.ContentLength];
			using (Stream dataStream = response.GetResponseStream())
				dataStream.Read(responseData, 0, responseData.Length);
			Console.Write(_encoder.GetString(responseData));
			var responseObject = _serializer.Deserialize<PublishResponse>(_encoder.GetString(responseData));
			return responseObject;
		}
		
		/*public lr_document ObtainDocByDocID(string docId)
		{
			WebClient wc = new WebClient();
			Dictionary<string, string> args = new Dictionary<string, string>();
			
		}
		
		public List<lr_document> ObtainDocsByResourceLocator(string locator)
		{	
		}
		public List<string> ObtainIdsByResourceLocator(string locator)
		{
		}
		private lr_Envelope obtain(Dictionary<string,string> args)
		{
			
		}
		/*
		public List<ResourceDataDocument>  Harvest()
		{
		}
		
		public List<ResourceDataDocument> Slice()
		{
		}*/
	}
}


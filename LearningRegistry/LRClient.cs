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

        public string Username { get; set; }
        public string Password { get; set; }

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
            _encoder = LRUtils.GetEncoder();
			_harvester = new Harvester();
		}
		
		public LRClient(string baseUri)
		{
			_baseUri = new Uri(baseUri);
            _serializer = LRUtils.GetSerializer();
            _encoder = LRUtils.GetEncoder();
			_harvester = new Harvester(_baseUri);
            Username = "";
            Password = "";
		}

        public LRClient(string baseUri, string username, string password)
        {
            _baseUri = new Uri(baseUri);
            _serializer = LRUtils.GetSerializer();
            _encoder = LRUtils.GetEncoder();
            _harvester = new Harvester(_baseUri);
            Username = username;
            Password = password;
        }

        public PublishResponse Publish(lr_Envelope docs)
        {
            string rawData = LRUtils.HttpPostRequest(_baseUri, LRUtils.Routes.Publish,
                                                     docs.Serialize(), "application/json", 
                                                     this.Username, this.Password);
            return _serializer.Deserialize<PublishResponse>(rawData);
        }
		
		public lr_document ObtainDocByID(string docId)
		{
			Dictionary<string, object> args = new Dictionary<string, object>();
			args["by_doc_ID"] = true;
			
			var result = LRUtils.Obtain(_baseUri, docId, args, this.Username, this.Password);
			
			if(result.documents.Count < 1)
				throw new Exception("Document with id "+docId+" does not exist.");
			
			return result.documents[0].document[0];
		}
		
		public ObtainResult ObtainDocsByResourceLocator(string locator, bool ids_only = false)
		{
            Dictionary<string, object> args = new Dictionary<string, object>();
			
			if(ids_only)
			{
				args["ids_only"] = "true";
				args["by_doc_ID"] = "true"; //we only want doc ids
			}
			
			return LRUtils.Obtain(_baseUri, locator, args);
		}

	}
}


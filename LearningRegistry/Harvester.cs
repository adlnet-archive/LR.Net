using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using LearningRegistry.RDDD;
using System.Reflection;

namespace LearningRegistry
{
	public class Harvester
	{
		static class HarvesterActions
		{
			public const string GetRecord = "getrecord";
			public const string ListRecords = "listrecords";
			public const string ListIdentifiers = "listidentifiers";
			public const string ListMetadataFormats = "listmetadataformats";
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
				_baseUri = System.IO.Path.Combine(value, "harvest");
			}
		}
		
		private JavaScriptSerializer _serializer;
		
		public Harvester()
		{
			this._baseUri = "http://localhost";
			_serializer = new JavaScriptSerializer();
		}
		
		internal Harvester (string baseUri)
		{
			this.BaseUri = baseUri;
			_serializer = new JavaScriptSerializer();
		}
		
		public HarvestRecord GetRecordByDocId(string docId)
		{
			return getRecord(docId, true).getrecord["record"][0];
		}
		
		public GetRecordHarvestResult GetRecordsByResourceLocator(string locator)
		{
			return getRecord(locator);
		}
		
		public ListRecordsHarvestResult ListRecords()
		{
			return (ListRecordsHarvestResult) this.Harvest(HarvesterActions.ListRecords, null);
		}
		
		public ListRecordsHarvestResult ListRecordsFrom(DateTime date)
		{
			return (ListRecordsHarvestResult) harvestFromDateBoundary(HarvesterActions.ListRecords, "from", date);
		}
		
		public ListRecordsHarvestResult ListRecordsUntil(DateTime date)
		{
			return (ListRecordsHarvestResult) harvestFromDateBoundary(HarvesterActions.ListRecords, "until", date);
		}
		
		public ListRecordsHarvestResult ListRecords(DateTime fromDate, DateTime untilDate)
		{
			return (ListRecordsHarvestResult) harvestFromDateRange(HarvesterActions.ListRecords, fromDate, untilDate);
		}
		
		public ListIdentifiersHarvestResult ListIdentifiers()
		{
			return (ListIdentifiersHarvestResult) this.Harvest(HarvesterActions.ListIdentifiers, null);
		}
		
		public ListIdentifiersHarvestResult ListIdentifiersFrom(DateTime date)
		{
			return (ListIdentifiersHarvestResult) harvestFromDateBoundary(HarvesterActions.ListIdentifiers, "from", date);
		}
		
		public ListIdentifiersHarvestResult ListIdentifiersUntil(DateTime date)
		{
			return (ListIdentifiersHarvestResult) harvestFromDateBoundary(HarvesterActions.ListIdentifiers, "until", date);
		}
		
		public ListIdentifiersHarvestResult ListIdentifiers(DateTime fromDate, DateTime untilDate)
		{
			return (ListIdentifiersHarvestResult) harvestFromDateRange(HarvesterActions.ListIdentifiers, fromDate, untilDate);
		}
		
		public ListMetadataFormatsHarvestResult ListMetadataFormats()
		{
			return (ListMetadataFormatsHarvestResult) this.Harvest(HarvesterActions.ListMetadataFormats, null);
		}
		
		internal HarvestResult Harvest(string action, Dictionary<string, string> args)
		{
			string url = System.IO.Path.Combine(_baseUri, action);
			url += LRUtils.BuildQueryString(args);
			
			WebClient wc = new WebClient();
			string jsonData = wc.DownloadString(url);
			HarvestResult results = new HarvestResult();
			
			switch (action)
			{
				case HarvesterActions.GetRecord:
					results = _serializer.Deserialize<GetRecordHarvestResult>(jsonData);
					break;
				case HarvesterActions.ListRecords:
					results = _serializer.Deserialize<ListRecordsHarvestResult>(jsonData);
					break;
				case HarvesterActions.ListIdentifiers:
					results = _serializer.Deserialize<ListIdentifiersHarvestResult>(jsonData);
					break;
				case HarvesterActions.ListMetadataFormats:
					results = _serializer.Deserialize<ListMetadataFormatsHarvestResult>(jsonData);
					break;
				default:
					throw new System.ArgumentException("Invalid harvester action: " + action);
			}
			
			if(!results.OK)
				throw new WebException("Error while calling getRecord: "+results.error, WebExceptionStatus.UnknownError);
			
			results.Action = action;
			results.Args = args;
			results.Harvester = this;
			return results;
		}
		
		private GetRecordHarvestResult getRecord(string requestId, bool byDocId = false)
		{
			var args = new Dictionary<string,string>();
			args["request_ID"] = requestId;
			if(byDocId)
				args["by_doc_ID"] = true.ToString();
			
			var result = (GetRecordHarvestResult) Harvest(HarvesterActions.GetRecord, args);
			return result;
		}
		
		private HarvestResult harvestFromDateBoundary(string action, string boundaryType, DateTime date)
		{
			return this.Harvest(action,
			                 	new Dictionary<string, string>() {
									{ boundaryType, date.ToString(LRUtils.ISO_8061_FORMAT) }
							 	});
		}
		private HarvestResult harvestFromDateRange(string action, DateTime fromDate, DateTime untilDate)
		{
			var args = new Dictionary<string, string>();
			args["from"] = fromDate.ToString(LRUtils.ISO_8061_FORMAT);
			args["until"] = untilDate.ToString(LRUtils.ISO_8061_FORMAT);
			return this.Harvest(action, args);
		}
	}
}


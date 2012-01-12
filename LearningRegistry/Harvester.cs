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
		internal static class HarvesterActions
		{
			public const string GetRecord = "getrecord";
			public const string ListRecords = "listrecords";
			public const string ListIdentifiers = "listidentifiers";
			public const string ListMetadataFormats = "listmetadataformats";
		}
		
		private Uri _baseUri;
		
		public Harvester()
		{
			this._baseUri = new Uri("http://localhost");
		}
		
		internal Harvester (Uri baseUri)
		{
			this._baseUri = baseUri;
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
			return (ListRecordsHarvestResult) LRUtils.Harvest(_baseUri, HarvesterActions.ListRecords, null);
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
			return (ListIdentifiersHarvestResult) LRUtils.Harvest(_baseUri, HarvesterActions.ListIdentifiers, null);
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
			return (ListMetadataFormatsHarvestResult) LRUtils.Harvest(_baseUri, HarvesterActions.ListMetadataFormats, null);
		}
		
		
		
		private GetRecordHarvestResult getRecord(string requestId, bool byDocId = false)
		{
			Console.WriteLine(_baseUri.AbsoluteUri);
			var args = new Dictionary<string,object>();
			args["request_ID"] = requestId;
			if(byDocId)
				args["by_doc_ID"] = true.ToString();
			
			var result = (GetRecordHarvestResult) LRUtils.Harvest(_baseUri, HarvesterActions.GetRecord, args);
			return result;
		}
		
		private HarvestResult harvestFromDateBoundary(string action, string boundaryType, DateTime date)
		{
			return LRUtils.Harvest(_baseUri, action,
			                 	new Dictionary<string, object>() {
									{ boundaryType, date.ToString(LRUtils.ISO_8061_FORMAT) }
							 	});
		}
		private HarvestResult harvestFromDateRange(string action, DateTime fromDate, DateTime untilDate)
		{
			var args = new Dictionary<string, object>();
			args["from"] = fromDate.ToString(LRUtils.ISO_8061_FORMAT);
			args["until"] = untilDate.ToString(LRUtils.ISO_8061_FORMAT);
			return LRUtils.Harvest(_baseUri, action, args);
		}
	}
}


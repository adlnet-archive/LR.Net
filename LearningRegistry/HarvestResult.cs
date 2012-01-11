using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using LearningRegistry.RDDD;

namespace LearningRegistry
{
	public interface IRecordRetriever
	{
		List<HarvestRecord>GetRecords();
	}
	
	public interface IStringListRetriever
	{
		List<string> GetList();
	}
	
	// Keeping consistent with spec redundancy
	public class HarvestRecord
	{
		public HarvestRecordHeader header;
		public lr_document resource_data;
	}
	public class HarvestRecordHeader
	{
		public string identifier;
		public string datestamp;
		public string status;
	}
	public class HarvestResultRequestData
	{
		public string verb;
		public string identifier;
		public bool by_doc_ID;
		public bool by_resource_ID;
		public string HTTP_request;
	}
	public class HarvestResult
	{	
		public Boolean OK;
		public string error;
		public string responseDate;
		public HarvestResultRequestData request;
		public string resumption_token;
		
		[ScriptIgnore]
		internal string Action { get; set; }
		
		[ScriptIgnore]
		internal Dictionary<string, string> Args { get; set; }
		
		[ScriptIgnore]
		internal Uri BaseUri { get; set; }
		
		[ScriptIgnore]
		internal Harvester Harvester;
		
		[ScriptIgnore]
		public bool HasMoreRecords
		{
			get { return !String.IsNullOrEmpty(resumption_token); }
		}
		
		public HarvestResult GetNextPage()
		{
			if(!this.HasMoreRecords)
				throw new System.IndexOutOfRangeException("No resumption token present");
			
			Args["resumption_token"] = resumption_token;
			return LRUtils.Harvest(BaseUri, this.Action, this.Args);
		}
	}
	
	public class GetRecordHarvestResult : HarvestResult, IRecordRetriever
	{
		public Dictionary<string, List<HarvestRecord>> getrecord;
		
		public List<HarvestRecord> GetRecords ()
		{
			return getrecord["record"];
		}
	}
	public class ListRecordsHarvestResult : HarvestResult, IRecordRetriever
	{
		public List<Dictionary<string, HarvestRecord>> listrecords;
		public List<HarvestRecord> GetRecords ()
		{
			List<HarvestRecord> records = new List<HarvestRecord>();
			foreach(var dictRecord in listrecords)
				records.Add(dictRecord["record"]);
			return records;
		}
	}
	public class ListIdentifiersHarvestResult : HarvestResult, IStringListRetriever
	{
		public List<Dictionary<string, HarvestRecordHeader>> listidentifiers;
		public List<string> GetList()
		{
			List<string> ids = new List<string>();
			foreach(var header in listidentifiers)
				ids.Add(header["header"].identifier);
			return ids;
		}
	}
	public class ListMetadataFormatsHarvestResult : HarvestResult, IStringListRetriever
	{
		public List<Dictionary<string,Dictionary<string,string>>> listmetadataformats;
		public List<string> GetList()
		{
			List<string> fmts = new List<string>();
			foreach(var fmtObj in listmetadataformats)
				fmts.Add(fmtObj["metadataformat"]["metadataPrefix"]);
			
			return fmts;
		}
	}
}


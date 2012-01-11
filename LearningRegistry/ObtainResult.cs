using System;
using LearningRegistry.RDDD;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Linq;

namespace LearningRegistry
{
	public class ObtainResult : ResumableResult
	{
		public List<ObtainRecord> documents;
		
		[ScriptIgnore]
		internal string ResourceId { get; set; }
		
		public List<lr_document> GetDocuments()
		{
			return documents.Select( x => x.document[0] ).ToList<lr_document>();
		}
		
		public List<string> GetDocumentIds()
		{
			return documents.Select( x => x.doc_ID ).ToList<string>();
		}
		
		public override ResumableResult GetNextPage ()
		{
			if(!this.HasMoreRecords)
				throw new System.IndexOutOfRangeException("No resumption token present");
			
			Args["resumption_token"] = resumption_token;
			return LRUtils.Obtain(BaseUri, ResourceId, Args);
		}
	}
	public class ObtainRecord
	{
		public string doc_ID;
		public List<lr_document> document;
	}
}


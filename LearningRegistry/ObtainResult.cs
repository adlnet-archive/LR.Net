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
		
		public List<lr_document> GetDocuments()
		{
			return documents.Select( x => x.document[0] ).ToList<lr_document>();
		}
		
		public List<string> GetDocumentIds()
		{
			return documents.Select( x => x.doc_ID ).ToList<string>();
		}
		
		protected override ResumableResult getPage ()
		{
			return LRUtils.Obtain(BaseUri, new List<string>(), _Args, HttpUsername, HttpPassword);
		}
	}
	public class ObtainRecord
	{
		public string doc_ID;
		public List<lr_document> document;
	}
}


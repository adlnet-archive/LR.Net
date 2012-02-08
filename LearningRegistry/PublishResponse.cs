using System;
using LearningRegistry.RDDD;

namespace LearningRegistry
{
	public class DocPublishResult
	{
		public bool OK;
		public string error;
		public string doc_ID;
	}
	public class PublishResponse : Result
	{
		public bool OK;
		public string error;
		public System.Collections.Generic.List<DocPublishResult> document_results;
	}
}
using System;
using System.Web.Script.Serialization;

namespace LearningRegistry
{
	public abstract class ResumableResult
	{
		public string resumption_token;
		
		[ScriptIgnore]
		internal Uri BaseUri { get; set; }
		
		[ScriptIgnore]
		internal System.Collections.Generic.Dictionary<string, string> Args { get; set; }
		
		[ScriptIgnore]
		public bool HasMoreRecords
		{
			get { return !String.IsNullOrEmpty(resumption_token); }
		}
		
		public abstract ResumableResult GetNextPage();
	}
}


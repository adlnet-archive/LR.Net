using System;
using System.Web.Script.Serialization;

namespace LearningRegistry
{
	public abstract class ResumableResult
	{
		[ScriptIgnore]
		internal Uri BaseUri { get; set; }
		
		[ScriptIgnore]
		internal System.Collections.Generic.Dictionary<string, string> Args { get; set; }
		
		public abstract ResumableResult GetNextPage();
	}
}


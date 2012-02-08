using System;
using System.Web.Script.Serialization;
using System.Collections.Generic;

namespace LearningRegistry
{
	public abstract class ResumableResult : Result
	{
		public string resumption_token;
		
		[ScriptIgnore]
		internal Uri BaseUri { get; set; }

        [ScriptIgnore]
        internal string HttpUsername { get; set; }

        [ScriptIgnore]
        internal string HttpPassword { get; set; }
		
		[ScriptIgnore]
		public bool HasMoreRecords
		{
			get { return !String.IsNullOrEmpty(resumption_token); }
		}

        [ScriptIgnore]
        protected Dictionary<string, object> _Args = new Dictionary<string,object>();
		
		public ResumableResult GetNextPage()
        {
            if(!this.HasMoreRecords)
				throw new System.IndexOutOfRangeException("No resumption token present");
            _Args["resumption_token"] = resumption_token;
            return getPage();
        }

        protected abstract ResumableResult getPage();
	}
}


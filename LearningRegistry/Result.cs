using System;

namespace LearningRegistry
{
	public abstract class Result
	{
		public virtual string Serialize()
		{
			System.Web.Script.Serialization.JavaScriptSerializer ser = 
				new System.Web.Script.Serialization.JavaScriptSerializer();
			
			return ser.Serialize(this);
		}
	}
}


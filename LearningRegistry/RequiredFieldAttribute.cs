using System;

namespace LearningRegistry
{
	public class RequiredField : Attribute
	{
		private bool _immutable;
		public bool Immutable { get { return _immutable; } }	
		
		
		public RequiredField()
		{
			_immutable = false;
		}
		
		public RequiredField (bool immutable)
		{
			_immutable = immutable;
		}
	}
}


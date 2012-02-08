namespace LearningRegistry
{
	namespace Taxonomies
	{
		public struct PayloadPlacement
		{
			public const string Inline = "inline";
			public const string Attached = "attached";
			public const string Linked = "linked";
		}
			
		public struct ResourceDataType
		{
			public const string Metadata = "metadata";
			public const string Paradata = "paradata";
			public const string Assertion = "assertion";
			public const string Resource = "resource";
		}
		
		public struct SigningMethod
		{
			public const string LR_PGP_1_0 = "LR-PGP.1.0";
		}
		
		public struct SubmitterType
		{
			public const string Anonymous = "anonymous";
			public const string User = "user";
			public const string Agent = "agent";
		}
	}
}

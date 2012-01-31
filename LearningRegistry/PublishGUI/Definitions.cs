using System;

namespace PublishGUI
{
	public enum PayloadPlacement
	{
		inline = 0,
		linked
	}
	
	public enum SignatureType
	{
		None = 0,
		LR_PGP,
		X509
	}
	
	public enum SubmitterType
	{
		anonymous = 0,
		user = 1,
		agent = 2
	}
	
	public enum ResourceDataType
	{
		metadata = 0,
		paradata,
		resource,
		assertion,
		other
	}
	
	public enum AuthType
	{
		None = 0,
		Basic
	}
}


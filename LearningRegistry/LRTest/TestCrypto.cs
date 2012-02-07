using System;
using NUnit.Framework;
using LearningRegistry;
using LearningRegistry.RDDD;

namespace LRTest
{
	[TestFixture]
	public class TestCrypto
	{
		private LRClient client;
		private PgpSigner signer; 
		
		
		[SetUp]
		public void SetUpTest()
		{
			client = new LRClient("http://sandbox.learningregistry.org");
			signer = new PgpSigner(new System.Collections.Generic.List<string>(){"http://research.adlnet.gov/amontoya.pub"},
		                           @"C:\\Documents and Settings\\montoyaa\\Application Data\\gnupg\\secring.gpg",
		                           "DPxl*120590*");
		}
		
		[Test]
		public void TestSign()
		{
			lr_Envelope docEnv = TestUtils.buildParadataTestEnvelope(1);
			for(int i = 0; i < docEnv.documents.Count; i++)
			{
				docEnv.documents[i] = signer.Sign (docEnv.documents[i]);
				Assert.IsNotNull(docEnv.documents[i].digital_signature);
				Assert.IsNotEmpty(docEnv.documents[i].digital_signature.signature);
				Console.WriteLine("sig: "+docEnv.documents[i].digital_signature.signature);
			}
		}
	}
}


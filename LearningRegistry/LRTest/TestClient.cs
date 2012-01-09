using System;
using NUnit.Framework;
using LearningRegistry;
using LearningRegistry.RDDD;
using LearningRegistry.Paradata;

namespace LRTest
{
	[TestFixture()]
	public class TestClient
	{
		private LRClient _client;
		
		[SetUp]
		public void SetUpTest()
		{
			_client = new LRClient("http://localhost");
		}
		
		[Test()]
		public void TestPublish ()
		{
			lr_Envelope env = TestUtils.buildParadataTestEnvelope();
			PublishResponse response = _client.Publish(env);
			var docResult = response.document_results[0];
			Assert.IsTrue(response.OK);
			Assert.IsNull(response.error);
			Assert.IsTrue(docResult.OK);
			Assert.IsNull(docResult.error);
			Assert.IsNotEmpty(docResult.doc_ID);
		}
		
		
	}
}
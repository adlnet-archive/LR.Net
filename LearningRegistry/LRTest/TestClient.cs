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
			_client = new LRClient("http://10.100.30.60", "lrlocal", "password");
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
		
		[Test]
		public void TestObtainByDocId()
		{
			lr_Envelope env = TestUtils.buildParadataTestEnvelope(1);
			PublishResponse res = _client.Publish(env);
			Assert.IsTrue(res.OK);
			string docId = res.document_results[0].doc_ID;
			
			var obtainResult = _client.ObtainDocByID(docId);
			Assert.AreEqual(docId, obtainResult.doc_ID);
		}
		
		[Test]
		public void TestObtainByResourceLocator()
		{
			string locator = "http://foo.bar";
			lr_Envelope env = TestUtils.buildParadataTestEnvelope(1);
			env.documents[0].resource_locator = locator;
			_client.Publish(env);
			
			var result = _client.ObtainDocsByResourceLocator(locator);
			var docs = result.GetDocuments();
			Assert.Greater(0, docs.Count);
			foreach (lr_document doc in docs)
				Assert.AreEqual(locator, doc.resource_locator);
			
			var result2 = _client.ObtainDocsByResourceLocator(locator, true);
			var ids = result2.GetDocumentIds();
			foreach (string id in ids)
				Assert.IsNotEmpty(id);
			foreach (object o in result.GetDocuments())
				Assert.IsNull(o);
			
		}
		
	}
}
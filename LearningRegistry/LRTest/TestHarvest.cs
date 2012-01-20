using System;
using System.Collections.Generic;
using NUnit.Framework;
using LearningRegistry;
using LearningRegistry.RDDD;

namespace LRTest
{
	[TestFixture]
	public class TestHarvest
	{
		private LRClient _client;
		private Harvester _harvester;
		
		[SetUp]
		public void SetUpTest()
		{
			_client = new LRClient("http://10.100.30.60", "lrlocal", "password");
			_harvester = _client.Harvester;
		}
		
		[Test]
		public void TestGetRecordByDocId ()
		{
			lr_Envelope env = TestUtils.buildParadataTestEnvelope();
			lr_document doc = env.documents[0];
			
			var res = _client.Publish(env);
			string docId = res.document_results[0].doc_ID;
			HarvestRecord rec = _harvester.GetRecordByDocId(docId);

			Assert.IsTrue(res.OK);
			Assert.IsTrue(String.IsNullOrEmpty(res.error));
			
			Assert.AreEqual(rec.header.identifier, docId);
			Assert.AreEqual(rec.header.status, "active");
			Assert.AreEqual(rec.resource_data.doc_ID, docId);
			Assert.AreEqual(rec.resource_data.resource_locator, doc.resource_locator);
			Assert.AreEqual(rec.resource_data.identity.owner, doc.identity.owner);
		}
		
		//TODO: rewrite to publish/cleanup
		[Test()]
		public void TestListIdentifiers_noargs ()
		{
			ListIdentifiersHarvestResult result = _harvester.ListIdentifiers();
			
			AssertResponseOk(result);
			AssertValidStringList(result);
			
		}
		
		[Test()]
		public void TestListMetadataFormats ()
		{
			ListMetadataFormatsHarvestResult result = _harvester.ListMetadataFormats();
			
			AssertResponseOk(result);
			AssertValidStringList(result);
		}
		
		static void AssertResponseOk (HarvestResult result)
		{
			Assert.IsTrue(result.OK);
			Assert.IsTrue(String.IsNullOrEmpty(result.error));
		}
		
		static void AssertValidStringList(IStringListRetriever result)
		{
			List<string> items = result.GetList();
			Assert.Greater(items.Count, 0);
			foreach(string item in items)
				Assert.IsFalse(String.IsNullOrEmpty(item));
		}
	}
}


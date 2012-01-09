using System;
using LearningRegistry;
using LearningRegistry.RDDD;
using LearningRegistry.Paradata;

namespace LRTest
{
	public class TestUtils
	{	
		public static lr_Envelope buildParadataTestEnvelope(int numDocs = 1)
		{
			//create a document and an envelop
            lr_Envelope env = new lr_Envelope();
			
			for(int i = 0; i < numDocs; i++) 
			{
	            lr_document doc = new lr_document();
	
	            //Add the keys from the contentobject to the keys for the document
	            doc.keys.Add("3DR");
	            string[] keywords = new string[] { "keyword1", "keyword2" };
	            foreach (string key in keywords)
	                doc.keys.Add(key);
	
	            //This is the URI of the resource this data describes
	            doc.resource_locator = "http://www.example.com/LearningRegistryTest.html";
	
	            //Submitted by the ADL3DR agent
	            doc.identity.submitter = "austin montoya";//LearningRegistry.Settings.LearningRegistry_Integration_SubmitterName();
	            doc.identity.signer = "austin montoya";//LearningRegistry.Settings.LearningRegistry_Integration_SignerName();
	            doc.identity.submitter_type = "agent";
	            //The data is paradata
	            doc.resource_data_type = "paradata";
	
	            //Set ActivityStream as the paradata schema
	            doc.payload_schema.Add("LearningRegistry_Paradata_1_0");
	
	            LearningRegistry.Paradata.lr_Activity activity = new lr_Activity();
	            //Create a paradata object
	            LearningRegistry.Paradata.lr_Paradata pd = activity.activity;
	
	            //Create a complex actor type, set to 3dr user
	            lr_Actor.lr_Actor_complex mActor = new lr_Actor.lr_Actor_complex();
	            mActor.description.Add("AnonymousUser");
	            mActor.objectType = "3DR User";
	
	            //Set the paradata actor
	            pd.actor = mActor;
	
	            //Create a complex verb type
	            lr_Verb.lr_Verb_complex verb = new lr_Verb.lr_Verb_complex();
	            verb.action = "Published";
	            verb.context.id = "";
	            verb.date = DateTime.Now;
	            verb.measure = null;
	
	            //Set the paradata verb
	            pd.verb = verb;
	
	            //Create a complex object type
	            lr_Object.lr_Object_complex _object = new lr_Object.lr_Object_complex();
	            _object.id = "an object id";
	
	            //Set the paradata object
	            pd._object = _object;
	
	            //A human readable description for the paradata
	            pd.content = "The a user uploaded a new model which was assigned the PID ...";
	
	            //The resource_data of this Resource_data_description_document is the inline paradata
	            doc.resource_data = activity;
	            env.documents.Add(doc);
			}

            //sign the envelope 
            //env.Sign(LearningRegistry.Settings.LearningRegistry_Integration_KeyPassPhrase(), LearningRegistry.Settings.LearningRegistry_Integration_KeyID(), LearningRegistry.Settings.LearningRegistry_Integration_PublicKeyURL());
			return env;
		}
	}
}


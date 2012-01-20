using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LearningRegistry;
using LearningRegistry.RDDD;

public partial class MainWindow: Gtk.Window
{	
	private enum PayloadPlacement
	{
		Inline = 0,
		Linked
	}
	
	private enum AuthType
	{
		None = 0,
		Basic
	}
	
	private enum SignatureType
	{
		None = 0,
		LR_PGP,
		X509
	}
	
	public MainWindow (): base (Gtk.WindowType.Toplevel)
	{
		Build ();
	}
	
	protected void OnDeleteEvent (object sender, DeleteEventArgs a)
	{
		Application.Quit ();
		a.RetVal = true;
	}

	protected void UpdatePayloadChooseContainer (object sender, System.EventArgs e)
	{
		PayloadPlacement selectedVal = (PayloadPlacement)this.PayloadPlacementComboBox.Active;
		if (selectedVal == PayloadPlacement.Inline && this.PayloadLocatorContainer.Visible)
		{
			this.PayloadLocatorContainer.Visible = false;
			this.PayloadFileContainer.Visible = true;
		} else if (selectedVal == PayloadPlacement.Linked && this.PayloadFileContainer.Visible)
		{
			this.PayloadFileContainer.Visible = false;
			this.PayloadLocatorContainer.Visible = true;
		}
	}

	protected void PublishDocument (object sender, System.EventArgs e)
	{
		bool validated = ValidateRequiredFields();
		
		if(validated)
		{
			WriteLineToConsole("\n========BEGIN PUBLISH ATTEMPT========");
			lr_document doc = buildDoc();
			lr_Envelope envelope = buildEnvelope(new List<lr_document>(){doc});
			
			var client = new LRClient(this.NodeUrlTextBox.Text);
			if ((AuthType)this.AuthTypeComboBox.Active == AuthType.Basic)
			{
				client.Username = this.AuthUsernameTextBox.Text;
				client.Password = this.AuthPasswordTextBox.Text;
			}
			
			WriteToConsole("Publishing envelope...");
			try
			{
				PublishResponse pubResponse = client.Publish(envelope);
				WriteToConsole("Done\n");
				WriteLineToConsole("Document(s) were sucessfully published!"+
				                   "Here are the results:");
				WriteLineToConsole(pubResponse.Serialize());
			}
			catch (Exception exception)
			{
				WriteLineToConsole("Publish failed! Reason:\n"+exception.Message);
			}
			
		} else
			ShowMissingFields();	
	}

	protected void HandleAuthTypeChanged (object sender, System.EventArgs e)
	{
		AuthType selectedType = (AuthType)this.AuthTypeComboBox.Active;
		this.AuthCredentialsContainer.Visible = 
			selectedType == AuthType.Basic && !this.AuthCredentialsContainer.Visible;
	}
	
	protected void ShowMissingFields()
	{
	}
	
	private bool ValidateRequiredFields()
	{
		return true;
	}
	
	private lr_document buildDoc()
	{
		WriteToConsole("Building document...");
		lr_document doc = new lr_document();
			
		//Required fields
		doc.resource_data_type = this.ResourceDataTypeComboBox.ActiveText.ToLower();
		doc.resource_locator = this.ResourceLocatorTextBox.Text;
		doc.payload_placement = this.PayloadPlacementComboBox.ActiveText.ToLower();
		doc.payload_schema = this.PayloadSchemaTextBox.Text.Split(',').ToList();
		
		if(doc.payload_placement == "linked")
			doc.payload_locator = this.PayloadLocatorTextBox.Text;
		else
		{
			var reader = new StreamReader(this.PayloadFileChooser.Uri);
			doc.resource_data = reader.ReadToEnd();
		}
		
		//Optional Fields
		if(!String.IsNullOrEmpty(this.PayloadSchemaLocatorTextBox.Text))
			doc.payload_schema_locator = this.PayloadSchemaLocatorTextBox.Text;
		
		if(!String.IsNullOrEmpty(this.SchemaFormatTextBox.Text))
			doc.payload_schema_format = this.SchemaFormatTextBox.Text;
		
		var keysText = this.KeywordsTextBox.Text;
		if(!String.IsNullOrEmpty(keysText))
			doc.keys = keysText.Split(',').ToList();
		
		var ttlText = this.TimeToLiveTextBox.Text;
		if(!String.IsNullOrEmpty(ttlText))
			doc.resource_TTL = Int32.Parse(ttlText);
		
		//Terms of Service
		lr_TOS tos = new lr_TOS();
		if(!String.IsNullOrEmpty(this.TermsOfServiceTextBox.Text))
			tos.submission_TOS = this.TermsOfServiceTextBox.Text;
		if(!String.IsNullOrEmpty(this.AttributionStatementTextView.Buffer.Text))
			tos.submission_attribution = this.AttributionStatementTextView.Buffer.Text;
		doc.TOS = tos;
		
		//TODO: Signature
		
		WriteToConsole("Done\n");
		return doc;
	}
	
	protected lr_Envelope buildEnvelope(IEnumerable<lr_document> docs)
	{
		WriteToConsole("Stuffing the envelope...");
		lr_Envelope envelope = new lr_Envelope();
		if(this.SignatureTypeComboBox.Active > (int)SignatureType.None)
		{
				//TODO: Sign the envelope
		}
		envelope.documents.AddRange(docs);
		WriteToConsole("Done\n");
		return envelope;
	}

	protected void SaveResourceDescriptionDocument (object sender, System.EventArgs e)
	{
		
		FileChooserDialog dialog = new FileChooserDialog("Save Resource Description", 
		                                                 this, 
		                                                 FileChooserAction.Save,
		                                                 "Cancel", ResponseType.Cancel,
		                                                 "Save", ResponseType.Accept);
		dialog.AddFilter(getJsonFilter());
		if(dialog.Run() == (int)ResponseType.Accept)
		{
			lr_document doc = this.buildDoc();
			using(FileStream fs = new FileStream(dialog.Uri, FileMode.Create))
			{
				byte[] data = System.Text.Encoding.UTF8.GetBytes(doc.Serialize());
				fs.Write(data, 0, data.Length);
			}
		}
		dialog.Destroy();
	}

	protected void LoadResourceDescriptionDocument (object sender, System.EventArgs e)
	{
		FileChooserDialog dialog = new FileChooserDialog("Load Resource Description",
		                                                 this,
		                                                 FileChooserAction.Open,
		                                                 "Cancel", ResponseType.Cancel,
		                                                 "Open", ResponseType.Accept);
		
		dialog.AddFilter(getJsonFilter());
		if(dialog.Run() == (int)ResponseType.Accept)
		{
			WriteLineToConsole("Loading from "+dialog.Filename);
			using(FileStream fs = new FileStream(dialog.Filename, FileMode.Create))
			{
				using (StreamReader reader = new StreamReader(fs))
				{
					var doc = lr_document.Deserialize(reader.ReadToEnd());
					if(doc != null)
						PopulateFields(doc);
				}
			}
			WriteLineToConsole("Finished loading document.");
		}
		dialog.Destroy();
	}
	
	protected void PopulateFields(lr_document doc)
	{
		
	}
	
	private static FileFilter getJsonFilter()
	{
		FileFilter filter = new FileFilter();
		filter.Name = "JSON Documents";
		filter.AddMimeType("application/json");
		filter.AddPattern("*.json");
		return filter;
	}

	protected void WriteToConsole(string text)
	{
		ConsoleWindow.Buffer.Text += text;
	}
	protected void WriteLineToConsole(string text)
	{
		ConsoleWindow.Buffer.Text += "\n"+text+"\n";
	}
}

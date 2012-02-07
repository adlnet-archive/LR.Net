using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using Gtk;
using LearningRegistry;
using LearningRegistry.RDDD;
using PublishGUI;

public partial class MainWindow: Gtk.Window
{	
	private List<Gtk.Label> _missingFields;
	
	protected string _resourceDataRaw { get; set; }

	protected void OnSignatureTypeComboBoxChanged (object sender, System.EventArgs e)
	{
		throw new System.NotImplementedException ();
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
		if (selectedVal == PayloadPlacement.inline && this.PayloadLocatorContainer.Visible)
		{
			this.PayloadLocatorContainer.Visible = false;
			this.PayloadFileContainer.Visible = true;
		} else if (selectedVal == PayloadPlacement.linked)
		{
			this.PayloadFileContainer.Visible = false;
			this.PayloadEditorButtonBox.Visible = false;
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
			
			//Examine signature information and sign if necessary
			SignatureInformationWidget sigInfo = this.SignatureInformationWidget;
			if(sigInfo.SignatureType == SignatureType.LR_PGP)
			{
				PgpSigner signer = new PgpSigner(sigInfo.PgpPublicKeyLocations,
				                                 sigInfo.PgpKeyringLocation,
				                                 sigInfo.PgpSecretKeyPassphrase);
				
				doc = signer.Sign(doc);
			}
			
			lr_Envelope envelope = buildEnvelope(new List<lr_document>(){doc});
			
			var client = new LRClient(this.NodeInfo.NodeUrl);
			if (this.NodeInfo.AuthenticationType == AuthType.Basic)
			{
				client.Username = this.NodeInfo.HttpUsername;
				client.Password = this.NodeInfo.HttpPassword;
			}
			
			WriteToConsole("Publishing envelope...");
			try
			{
				PublishResponse pubResponse = client.Publish(envelope);
				WriteToConsole("Done\n");
				
				//Make sure the response was ok
				if(!pubResponse.OK)
					throw new Exception(pubResponse.error);
				else if(!pubResponse.document_results[0].OK)
					throw new Exception(pubResponse.document_results[0].error);
				
				WriteLineToConsole("Document(s) were sucessfully published! "+
				                   "Here are the results:");
				WriteLineToConsole(pubResponse.Serialize());
				PublishGUI.Helper.SavePublishHistory(this.NodeInfo.NodeUrl, pubResponse);
			}
			catch (Exception exception)
			{
				WriteLineToConsole("Publish failed! Reason:\n"+exception.Message);
				WriteToConsole("Stack Trace:\n" + exception.StackTrace);
			}
		} 
		else
			ShowMissingFields();
		
	}
	
	protected void ShowMissingFields()
	{
		Gdk.Color col = new Gdk.Color();
		Gdk.Color.Parse("red", ref col);
		
		foreach(var lbl in _missingFields)
			lbl.ModifyFg(StateType.Normal, col);
		
		Helper.CreateNotficationWindow(Helper.MSG_MISSING_FIELDS);
	}
	
	private bool ValidateRequiredFields()
	{
		List<Gtk.Label> missingFields = new List<Gtk.Label>();
		
		if(String.IsNullOrEmpty(this.ResourceLocatorTextBox.Text))
			missingFields.Add(lbl_ResourceLocator);
		
		if(PayloadPlacementComboBox.Active == (int)PayloadPlacement.inline
		   && String.IsNullOrEmpty(PayloadFileChooser.Filename))
			missingFields.Add(this.lbl_PayloadFile);
		else if(String.IsNullOrEmpty(this.PayloadLocatorTextBox.Text))
			missingFields.Add(this.lbl_PayloadLocator);
		
		if(String.IsNullOrEmpty(this.PayloadSchemaTextBox.Text))
			missingFields.Add(this.lbl_PayloadSchema);
		
		if(this.SubmitterTypeComboBox.Active > (int)SubmitterType.anonymous
		   && String.IsNullOrEmpty(SubmitterNameTextBox.Text))
			missingFields.Add(this.lbl_SubmitterName);
		
		missingFields.AddRange(this.SignatureInformationWidget.GetMissingFields());
		missingFields.AddRange(this.NodeInfo.GetMissingFields());
		
		Gdk.Color col = new Gdk.Color();
		Gdk.Color.Parse("black", ref col);
		
		//Check the old ones to see if they have been fixed
		if(_missingFields != null)
		{
			foreach(var lbl in _missingFields)
			{
				if(missingFields.Where( x => x.LabelProp == lbl.LabelProp ).Count() == 0)
					lbl.ModifyFg(StateType.Normal, col);
			}
		}
		
		_missingFields = missingFields;
		
		return _missingFields.Count == 0;
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
		
		if(doc.payload_placement == LearningRegistry.Taxonomies.PayloadPlacement.Linked)
			doc.payload_locator = this.PayloadLocatorTextBox.Text;
		else
			doc.resource_data = _resourceDataRaw;
		
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
		
		//Submitter Info
		lr_identity ident = new lr_identity();
		int submitterTypeSelection = this.SubmitterTypeComboBox.Active;
		ident.submitter_type = Enum.GetName(typeof(SubmitterType), submitterTypeSelection).ToLower();
		ident.submitter = (submitterTypeSelection > (int)SubmitterType.anonymous) 
						   ? SubmitterNameTextBox.Text 
						   : LearningRegistry.Taxonomies.SubmitterType.Anonymous;
		
		if(!String.IsNullOrEmpty(CuratorTextBox.Text))
			ident.curator = CuratorTextBox.Text;
		if(!String.IsNullOrEmpty(OwnerTextBox.Text))
			ident.owner = OwnerTextBox.Text;
		if(!String.IsNullOrEmpty(SignerTextBox.Text))
			ident.signer = SignerTextBox.Text;
		doc.identity = ident;
		
		WriteToConsole("Done\n");
		return doc;
	}
	
	protected lr_Envelope buildEnvelope(IEnumerable<lr_document> docs)
	{
		WriteToConsole("Stuffing the envelope...");
		lr_Envelope envelope = new lr_Envelope();
		
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
			using(FileStream fs = new FileStream(dialog.Filename, FileMode.Create))
			{
				byte[] data = System.Text.Encoding.UTF8.GetBytes(doc.Serialize());
				fs.Write(data, 0, data.Length);
			}
		}
		WriteLineToConsole("Resource description saved to "+dialog.Filename);
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
			using(FileStream fs = new FileStream(dialog.Filename, FileMode.Open))
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
		//Update fields
		if(!String.IsNullOrEmpty(doc.resource_locator))
			this.ResourceLocatorTextBox.Text = doc.resource_locator;
		if(doc.payload_schema.Count > 0)
			this.PayloadSchemaTextBox.Text = String.Join(",",doc.payload_schema.ToArray());
		
		if(doc.payload_placement == LearningRegistry.Taxonomies.PayloadPlacement.Linked)
		{
			if(!String.IsNullOrEmpty(doc.payload_locator))
				this.PayloadLocatorTextBox.Text = doc.payload_locator;
			this.PayloadLocatorContainer.Visible = true;
			this.PayloadFileContainer.Visible = false;
			this.PayloadEditorButtonBox.Visible = false;
		} 
		else
		{
			_resourceDataRaw = (string)doc.resource_data;
			this.PayloadFileContainer.Visible = false;
			this.PayloadLocatorContainer.Visible = false;
			this.PayloadEditorButtonBox.Visible = true;
		}
		if(!String.IsNullOrEmpty(doc.payload_schema_locator))
			this.PayloadSchemaLocatorTextBox.Text = doc.payload_schema_locator;
		if(!String.IsNullOrEmpty(doc.payload_schema_format))
			this.SchemaFormatTextBox.Text = doc.payload_schema_format;
		if(doc.keys.Count > 0)
			this.KeywordsTextBox.Text = String.Join (",", doc.keys.ToArray());
		if(doc.resource_TTL > 0)
			this.TimeToLiveTextBox.Text = doc.resource_TTL.ToString();
		if(!String.IsNullOrEmpty(doc.identity.submitter))
			this.SubmitterNameTextBox.Text = doc.identity.submitter;
		if(!String.IsNullOrEmpty(doc.identity.curator))
			this.CuratorTextBox.Text = doc.identity.curator;
		if(!String.IsNullOrEmpty(doc.identity.owner))
			this.OwnerTextBox.Text = doc.identity.owner;
		if(!String.IsNullOrEmpty(doc.identity.signer))
			this.SignerTextBox.Text = doc.identity.signer;
		if(!String.IsNullOrEmpty(doc.TOS.submission_TOS))
			this.TermsOfServiceTextBox.Text = doc.TOS.submission_TOS;
		if(!String.IsNullOrEmpty(doc.TOS.submission_attribution))
			this.AttributionStatementTextView.Buffer.Text = doc.TOS.submission_attribution;
		
		
		//Update Comboboxes
		PopulateComboBox<ResourceDataType>(doc.resource_data_type, ref this.ResourceDataTypeComboBox, ResourceDataType.other);
		PopulateComboBox<PayloadPlacement>(doc.payload_placement, ref this.PayloadPlacementComboBox, PayloadPlacement.inline);
		PopulateComboBox<SubmitterType>(doc.identity.submitter_type, ref this.SubmitterTypeComboBox, SubmitterType.anonymous);
		
		EventArgs args = new EventArgs();
		UpdatePayloadChooseContainer(this, args);
		UpdateSubmitterNameVisibility(this, args);
	}

	protected void WriteToConsole(string text)
	{
		ConsoleWindow.Buffer.Text += text;
	}
	protected void WriteLineToConsole(string text)
	{
		ConsoleWindow.Buffer.Text += "\n"+text+"\n";
	}

	protected void UpdateSubmitterNameVisibility (object sender, System.EventArgs e)
	{
		int submitterType = this.SubmitterTypeComboBox.Active;
		SubmitterNameContainer.Visible = submitterType > (int)SubmitterType.anonymous;
	}

	protected void CreateEditPayloadPopup (object sender, System.EventArgs e)
	{
		Window popup = new Window(WindowType.Toplevel);
		
		popup.BorderWidth = 10;
		VBox container = new VBox();
		Label lbl = new Label();
		lbl.Text = "Resource Data:";
		lbl.Xalign = 0;
		container.Add(lbl);
		TextView tview = new TextView();
		tview.HeightRequest = 300;
		tview.WidthRequest = 500;
		tview.WrapMode = WrapMode.Char;
		if(!String.IsNullOrEmpty(_resourceDataRaw))
			tview.Buffer.Text = _resourceDataRaw;
		container.Add(tview);
		
		HBox box = new HBox();
		Button doneBtn = new Button();
		doneBtn.Label = "Done";
		doneBtn.Clicked += delegate(object s, EventArgs eargs) {
			_resourceDataRaw = tview.Buffer.Text;
			popup.Destroy();
		};
		Button cancelBtn = new Button();
		cancelBtn.Label = "Cancel";
		cancelBtn.Clicked += (s, eargs) => popup.Destroy();
		box.Add(doneBtn);
		box.Add(cancelBtn);
		container.Add(box);
		
		
		popup.Add(container);
		popup.ShowAll();
	}

	protected void ResetFields (object sender, System.EventArgs e)
	{
		//Reset all of this container's fields
		this.ResourceDataTypeComboBox.Active = (int)ResourceDataType.metadata;
		this.ResourceLocatorTextBox.Text = "";
		this.PayloadPlacementComboBox.Active = (int)PayloadPlacement.inline;
		this.PayloadLocatorTextBox.Text = "";
		this.PayloadSchemaTextBox.Text = "";
		this.PayloadSchemaLocatorTextBox.Text = "";
		this.SchemaFormatTextBox.Text = "";
		this.KeywordsTextBox.Text = "";
		this.TimeToLiveTextBox.Text = "";
		this.SubmitterTypeComboBox.Active = (int)SubmitterType.anonymous;
		this.SubmitterNameTextBox.Text = "";
		this.CuratorTextBox.Text = "";
		this.OwnerTextBox.Text = "";
		this.SignerTextBox.Text = "";
		this.TermsOfServiceTextBox.Text = "";
		this.AttributionStatementTextView.Buffer.Text = "";
		this.PayloadFileChooser.UnselectAll();
		
		//Reset the widgets' fields
		this.SignatureInformationWidget.ResetFields();
		this.NodeInfo.ResetFields();
		
		_resourceDataRaw = null;
		UpdatePayloadChooseContainer(this, e);
		UpdateSubmitterNameVisibility(this, e);
		this.NodeInfo.HandleAuthTypeChanged(this, e);
	}

	protected void ChooseNewPayloadFile (object sender, System.EventArgs e)
	{
		FileChooserDialog dialog = new FileChooserDialog("Load Resource Data",
		                                                 this,
		                                                 FileChooserAction.Open,
		                                                 "Cancel", ResponseType.Cancel,
		                                                 "Open", ResponseType.Accept);
		if(dialog.Run() == (int)ResponseType.Accept)
		{
			using(FileStream fs = new FileStream(dialog.Filename, FileMode.Open))
				using (StreamReader reader = new StreamReader(fs))
					_resourceDataRaw = reader.ReadToEnd();
		}
		dialog.Destroy();
	}

	protected void OnPayloadFileChooserSelectionChanged (object sender, System.EventArgs e)
	{
		this.PayloadFileContainer.Visible = false;
		using(StreamReader reader = new StreamReader(this.PayloadFileChooser.Filename))
			_resourceDataRaw = reader.ReadToEnd();
		this.PayloadEditorButtonBox.Visible = true;
	}
	
	protected static void PopulateComboBox<TEnumType> (string docVal, ref ComboBox combo, TEnumType defaultVal)
	{
		TEnumType eType;
		try
		{
			eType = (TEnumType)Enum.Parse(typeof(TEnumType), docVal);
		} 
		catch
		{
			eType = defaultVal;
		}
		combo.Active = (int)Convert.ChangeType(eType, typeof(int));
	}
	
	private static FileFilter getJsonFilter()
	{
		FileFilter filter = new FileFilter();
		filter.Name = "JSON Documents";
		filter.AddMimeType("application/json");
		filter.AddPattern("*.json");
		return filter;
	}

	protected void OnPublishFromCSVActionActivated (object sender, System.EventArgs e)
	{
		FileChooserDialog dialog = new FileChooserDialog("Load CSV", 
		                                                 this,
		                                                 FileChooserAction.Open,
		                                                 "Cancel", ResponseType.Close,
		                                                 "Open", ResponseType.Accept);
		
		FileFilter filter = new FileFilter();
		filter.Name = "CSV files";
		filter.AddMimeType("text/csv");
		filter.AddPattern("*.csv");
		dialog.AddFilter(filter);

        if (dialog.Run() == (int)ResponseType.Accept)
        {
            CsvToLrWindow csvWin = new CsvToLrWindow();
            csvWin.PopulateFromCsv(dialog.Filename);
            dialog.Destroy();
            csvWin.ShowAll();
        }
        else
            dialog.Destroy();
	}
}

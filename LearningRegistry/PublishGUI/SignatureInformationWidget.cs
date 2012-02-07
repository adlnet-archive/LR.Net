using System;
using System.Linq;
using System.Collections.Generic;
using PublishGUI;


[System.ComponentModel.ToolboxItem(true)]
public partial class SignatureInformationWidget : Gtk.Bin
{
	public SignatureType SignatureType { get { return (SignatureType)this.SignatureTypeComboBox.Active; } }
	public string PgpKeyringLocation { get { return this.PgpSecretKeyRingLocationFileChooser.Filename; } }
	public string PgpSecretKeyPassphrase { get { return this.PgpSecretKeyPassphraseTextBox.Text; } }
	public List<string> PgpPublicKeyLocations 
	{ 
		get 
		{ 
			try
			{
				string[] vals = PgpPublicKeyLocationsTextBox.Text.Split(','); 
				List<string> retVals = new List<string>();
				foreach(var val in vals)
					retVals.Add(val);
				return retVals;
			}
			catch { return null; } 
		} 
	}
	
	
	public SignatureInformationWidget ()
	{
		this.Build ();
		this.Shown += UpdateSubcontainerVisibility;
	}

	protected void UpdateSubcontainerVisibility (object sender, System.EventArgs e)
	{
		PgpInfoContainer.Visible = this.SignatureTypeComboBox.Active == (int)PublishGUI.SignatureType.LR_PGP;
		PgpInfoContainer.ChildVisible = this.SignatureTypeComboBox.Active == (int)PublishGUI.SignatureType.LR_PGP;
	}
	
	public void ResetFields()
	{
		this.SignatureTypeComboBox.Active = 0;
		this.PgpPublicKeyLocationsTextBox.Text = String.Empty;
		this.PgpSecretKeyPassphraseTextBox.Text = String.Empty;
		this.PgpSecretKeyRingLocationFileChooser.UnselectAll();
		UpdateSubcontainerVisibility(this, null);
	}
			
	public List<Gtk.Label> GetMissingFields()
	{
		List<Gtk.Label> missingFields = new List<Gtk.Label>();
		if(this.SignatureTypeComboBox.Active == (int)PublishGUI.SignatureType.LR_PGP)
		{
			if(String.IsNullOrEmpty(this.PgpSecretKeyRingLocationFileChooser.Filename))
				missingFields.Add(this.lbl_PgpSecretKeyRingLocation);
			
			if(String.IsNullOrEmpty(this.PgpSecretKeyPassphraseTextBox.Text))
				missingFields.Add(this.lbl_PgpSecretKeyPassphrase);
			
			if(String.IsNullOrEmpty(this.PgpPublicKeyLocationsTextBox.Text))
				missingFields.Add(this.lbl_PgpPublicKeyLocations);
		}
		
		return missingFields;
	}
}



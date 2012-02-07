using System;
using System.Collections.Generic;

using Gtk;
using PublishGUI;


[System.ComponentModel.ToolboxItem(true)]
public partial class NodeInfoWidget : Gtk.Bin
{	
	public string NodeUrl
	{
		get 
		{ 
			return "http://"+this.NodeUrlTextBox.Text; 
		}
		set 
		{
			this.NodeUrlTextBox.Text = value.Replace("http://", "");
		}
	}
	
	public string HttpUsername
	{
		get { return this.AuthUsernameTextBox.Text; }
	}
	
	public string HttpPassword
	{
		get { return this.AuthPasswordTextBox.Text; }
	}
	
	public AuthType AuthenticationType
	{
		get { return (AuthType)this.AuthTypeComboBox.Active; }
	}
	
	public NodeInfoWidget ()
	{
		this.Build ();
		this.Shown += HandleAuthTypeChanged;
	}
	
	public void HandleAuthTypeChanged (object sender, System.EventArgs e)
	{
		AuthType selectedType = (AuthType)this.AuthTypeComboBox.Active;
		this.AuthCredentialsContainer.Visible = selectedType == AuthType.Basic;
		this.AuthCredentialsContainer.ChildVisible = selectedType == AuthType.Basic;
	}
	
	public void ResetFields()
	{
		this.AuthTypeComboBox.Active = 0;
		this.AuthUsernameTextBox.Text = "";
		this.AuthPasswordTextBox.Text = "";
	}
	
	public List<Gtk.Label> GetMissingFields()
	{
		List<Gtk.Label> missingFields = new List<Gtk.Label>();
		if(String.IsNullOrEmpty(this.NodeUrlTextBox.Text))
			missingFields.Add(this.lbl_NodeUrl);
		
		if(this.AuthTypeComboBox.Active == (int)AuthType.Basic)
		{
			if(String.IsNullOrEmpty(this.AuthUsernameTextBox.Text))
				missingFields.Add(this.lbl_AuthUsername);
			
			if(String.IsNullOrEmpty(this.AuthPasswordTextBox.Text))
				missingFields.Add(this.lbl_AuthPassword);
		}
		
		return missingFields;
	}
}



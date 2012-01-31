using System;
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
		this.AuthCredentialsContainer.Visible = 
			selectedType == AuthType.Basic && !this.AuthCredentialsContainer.Visible;
	}
	
	public void ResetFields()
	{
		this.AuthTypeComboBox.Active = 0;
		this.AuthUsernameTextBox.Text = "";
		this.AuthPasswordTextBox.Text = "";
	}
}



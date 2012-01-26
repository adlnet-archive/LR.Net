using System;
using System.Collections;
using System.Linq;
using Gtk;


[System.ComponentModel.ToolboxItem(true)]
public partial class CsvToLrMapRow : Gtk.Bin
{
	private bool _isConstant;
	public bool IsConstant { get { return _isConstant; } }
	public string Value 
	{
		get
		{
			if(_isConstant) 
				return CustomValueEntry.Text;
			else
			{
				if(ColumnOptionsComboBox.Active == 0)
					throw new NullReferenceException("No value was set for the column.");
				return ColumnOptionsComboBox.ActiveText;
			}
		}
	}
	
	public CsvToLrMapRow (IEnumerable options, string resourceDataDescriptionField)
	{
		this.Build ();
		
		ColumnOptionsComboBox.AppendText("Select Column...");
		foreach(string option in options)
			this.ColumnOptionsComboBox.AppendText(option);
		ColumnOptionsComboBox.AppendText("Default Value");
		ColumnOptionsComboBox.Active = 0;
		
		this.lbl_ResourceDataField.Text = resourceDataDescriptionField;
		CustomValueEntry.Shown += OnColumnSelectionChanged;
	}

	protected void OnColumnSelectionChanged (object sender, System.EventArgs e)
	{
		if(ColumnOptionsComboBox.ActiveText == "Default Value")
		{
			CustomValueEntry.Visible = true;
			_isConstant = true;
		}
		else
		{
			if (CustomValueEntry.Visible)
				CustomValueEntry.Visible = false;
			_isConstant = false;
		}
	}
}



using System;
using System.Collections;
using System.Linq;
using Gtk;


[System.ComponentModel.ToolboxItem(true)]
public partial class CsvToLrMapRow : Gtk.Bin
{
    private const string CONSTANT_VAL_TEXT = "<Constant Value>";
    private const string ROW_AS_CSV_TEXT = "<Entire Row (as JSON)>";
	public string Key
	{
		get { return this.lbl_ResourceDataField.Text; }
	}
	
	private bool _isConstant;
	public bool IsConstant { get { return _isConstant; } }

    private bool _isSerializeToRow;
    public bool IsSerializeToRow { get { return _isSerializeToRow; } }

	public string Value 
	{
		get
		{
			if(_isConstant) 
				return CustomValueEntry.Text;
			else
			{
				//TODO: improve exception throwing/handling for empty columns
				if(ColumnOptionsComboBox.Active == 0)
					return null;//throw new NullReferenceException("No value was set for the column.");
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
		ColumnOptionsComboBox.AppendText(CONSTANT_VAL_TEXT);
        ColumnOptionsComboBox.AppendText(ROW_AS_CSV_TEXT);
		ColumnOptionsComboBox.Active = 0;
		
		this.lbl_ResourceDataField.Text = resourceDataDescriptionField;
		CustomValueEntry.Shown += OnColumnSelectionChanged;
	}

	protected void OnColumnSelectionChanged (object sender, System.EventArgs e)
	{
		if(ColumnOptionsComboBox.ActiveText == CONSTANT_VAL_TEXT)
		{
			CustomValueEntry.Visible = true;
			_isConstant = true;
		}
		else
		{
            _isSerializeToRow = ColumnOptionsComboBox.ActiveText == ROW_AS_CSV_TEXT;

			if (CustomValueEntry.Visible)
				CustomValueEntry.Visible = false;

			_isConstant = false;
		}
	}
}



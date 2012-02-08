using System;
using System.Collections;
using System.Linq;
using Gtk;


[System.ComponentModel.ToolboxItem(true)]
public partial class CsvToLrMapRow : Gtk.Bin
{
    public const string NO_VAL_TEXT = "Select Column...";
    public const string CONSTANT_VAL_TEXT = "<Constant Value>";
    public const string ROW_AS_CSV_TEXT = "<Entire Row (as JSON)>";
	public string Key
	{
		get { return this.lbl_ResourceDataField.Text; }
	}
	
	private bool _isConstant;
	public bool IsConstant { get { return _isConstant; } }

    private bool _isSerializeToRow;
    public bool IsSerializeToRow { get { return _isSerializeToRow; } }

    private IEnumerable _options;

	public string DropDownValue 
	{
		get
		{
				//TODO: improve exception throwing/handling for empty columns
				if(ColumnOptionsComboBox.Active == 0 || ColumnOptionsComboBox.Active > _options.Cast<string>().Count() - 3)
					return null;
				return ColumnOptionsComboBox.ActiveText;
		}
	}

    public string ConstantValue
    {
        get { return this.CustomValueEntry.Text; }
    }

	public CsvToLrMapRow (IEnumerable options, string resourceDataDescriptionField)
	{
		this.Build ();
		
		ColumnOptionsComboBox.AppendText(NO_VAL_TEXT);
		foreach(string option in options)
			this.ColumnOptionsComboBox.AppendText(option);
        _options = options;
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



using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Gtk;
using LearningRegistry.RDDD;
using System.Text;


public partial class CsvToLrWindow : Gtk.Window
{
    private List<string> _rawRowDataList = new List<string>();
    private Dictionary<string, List<string>> _rowData;

	private string[] _columns;
    private DataTable _table;

    List<Type> nestedTypes = new List<Type>()
	{
		typeof(lr_identity),
		typeof(lr_TOS),
		typeof(lr_digital_signature)
	};


	public CsvToLrWindow () : 
			base(Gtk.WindowType.Toplevel)
	{
		
		this.Build ();
	}
	
	public void PopulateFromCsv(string path)
	{
        loadFromCsv(path);
		
		FieldInfo[] infos = typeof(lr_document).GetFields();
		foreach(var info in infos)
		{
			if(nestedTypes.Contains(info.FieldType))
			{
				//Nesting only goes one level, which makes this work
				//TODO: use flags to find non-primitives, then recursively descend into them.
				foreach(var subInfo in info.FieldType.GetFields())
				{
					string name = String.Join(".", info.Name, subInfo.Name);
					if(subInfo.GetCustomAttributes(typeof(LearningRegistry.RequiredField), true).Length > 0)
						name = name.Insert(0, "*");
					MapRowsContainer.Add(new CsvToLrMapRow(_columns, name));
				}
			}
			else
			{
				string name = info.Name;
				if(info.GetCustomAttributes(typeof(LearningRegistry.RequiredField), true).Length > 0)
					name = name.Insert(0, "*");
				CsvToLrMapRow row = new CsvToLrMapRow(_columns, name);
				MapRowsContainer.Add(row);
			}
		}
	}
	
	protected void PublishDocuments(object sender, EventArgs e)
	{


		//Input: rows
		//Need to read each row in and store the value in a mapped list
		//Output: map of column name to row value
		//Create a dictionary map the columns
		Dictionary<string, string> map = new Dictionary<string, string>();
        FieldInfo[] infos = typeof(lr_document).GetFields();
        int j = 0;
		for(int i = 0; i < MapRowsContainer.Children.Length; i++)
        {
            var info = infos[j];
            var mapRow = (CsvToLrMapRow)MapRowsContainer.Children[i];
            if(nestedTypes.Contains(info.FieldType))
            {
                foreach (var subInfo in info.FieldType.GetFields())
                {
                    mapRow = (CsvToLrMapRow)MapRowsContainer.Children[i++];
                    string key = String.Join(".", info.Name, subInfo.Name);
                    map[key] = mapRow.DropDownValue;
                }
                j++;
            } else
                map[infos[j++].Name] = mapRow.DropDownValue;
        }

		//Create the docs from the map and the dataDict
		lr_Envelope envelope = new lr_Envelope();
		for(int i = 0; i < _rawRowDataList.Count; i++)
		{
			lr_document doc = new lr_document();
            int rowIndex = 0;
			foreach(var info in infos)
			{
                CsvToLrMapRow currentRow = (CsvToLrMapRow)MapRowsContainer.Children[rowIndex++];
                if (currentRow.DropDownValue == null)
                    continue; //Nothing to map to or assign if they did not choose a value from the dropdown

                if (!nestedTypes.Contains(info.FieldType))
                {
                    string val;
                    if (currentRow.IsConstant)
                        val = currentRow.ConstantValue;
                    else if (currentRow.IsSerializeToRow)
                        val = _rawRowDataList[i];
                    else
                        val = _rowData[map[info.Name]][i];

                    if (info.FieldType == typeof(List<string>))
                    {
                        List<string> vals = val.Split(',').ToList<string>();
                        info.SetValue(doc, vals);
                    }
                    else
                        info.SetValue(doc, Convert.ChangeType(val, info.FieldType));
                }
                else //This is a nested object property
                {
                    var subFields = info.GetType().GetFields();
                    object currentObj = info.GetValue(doc);
                    if(currentObj == null)
                        currentObj = new object();
                    foreach (var subField in subFields)
                    {
                        var rowToAdd = ((CsvToLrMapRow[])MapRowsContainer.Children)
                                            .Where( x => 
                                                        x.Key.Split('.')[0] == info.Name &&
                                                        x.Key.Split('.')[1] == subField.Name)
                                            .ToList()[0];
                        string val = getNewDocValue(rowToAdd, _rawRowDataList[i], _rowData[map[rowToAdd.Key]][i]);
                        if(subField.FieldType == typeof(List<string>))
                        {
                            List<string> vals = val.Split(',').ToList<string>();
                            subField.SetValue(currentObj, vals);
                        } 
                        else
                            subField.SetValue(currentObj, val);
                    }
                }
			}
			envelope.documents.Add(doc);
		}
		
		
		Console.WriteLine("Done!");
	}

    private string getNewDocValue(CsvToLrMapRow currentRow, string rawRowData, string mappedData)
    {
        string val;
        if (currentRow.IsConstant)
            val = currentRow.ConstantValue;
        else if (currentRow.IsSerializeToRow)
            val = rawRowData;
        else
            val = mappedData;
        return val;
    }

    private void loadFromCsv(string path)
    {
        _table = ParseCSV(path);
        _columns = _table.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray<string>();

        var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
        Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
        List<string> rawRowDataList = new List<string>();

        //Add the row data by column names
        for (int i = 0; i < _columns.Length; i++)
        {
            List<string> dataList = new List<string>();
            foreach (DataRow row in _table.Rows)
                dataList.Add(Convert.ToString(row.ItemArray[i]));
            data[_columns[i]] = dataList;
        }

        //Add the serialized (CSV) rows
        foreach (DataRow row in _table.Rows)
        {
            int columnIndex = 0;
            var rowDict = new Dictionary<string, string>();
            foreach (object item in row.ItemArray)
                rowDict[_columns[columnIndex++]] = Convert.ToString(item);
            rawRowDataList.Add(serializer.Serialize(rowDict));
        }
        _rawRowDataList = rawRowDataList;
        _rowData = data;
    }

    private static DataTable ParseCSV(string path)
    {
        if (!File.Exists(path))
            return null;

        string full = System.IO.Path.GetFullPath(path);
        string file = System.IO.Path.GetFileName(full);
        string dir = System.IO.Path.GetDirectoryName(full);

        //create the "database" connection string
        string connString = "Provider=Microsoft.Jet.OLEDB.4.0;"
          + "Data Source=\"" + dir + "\\\";"
          + "Extended Properties=\"text;HDR=Yes;FMT=Delimited\"";

        //create the database query
        string query = "SELECT * FROM " + file;

        //create a DataTable to hold the query results
        DataTable dTable = new DataTable();

        //create an OleDbDataAdapter to execute the query
        OleDbDataAdapter dAdapter = new OleDbDataAdapter(query, connString);

        try
        {
            //fill the DataTable
            dAdapter.Fill(dTable);
        }
        catch (InvalidOperationException /*e*/)
        { }

        dAdapter.Dispose();

        return dTable;
    }
	
}


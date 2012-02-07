using System;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Gtk;
using LearningRegistry;
using LearningRegistry.RDDD;
using System.Text;


public partial class CsvToLrWindow : Gtk.Window
{
	private List<Label> _missingFields;
	
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
			var attrs = info.GetCustomAttributes(typeof(RequiredField), false);
			if(attrs.Length > 0)
			{
				if(((RequiredField)attrs[0]).Immutable)
					continue;
			}
				
			if(nestedTypes.Contains(info.FieldType))
			{
				//this is done internally
				if(info.FieldType.Equals(typeof(lr_digital_signature)))
					continue;
				
				//Nesting only goes one level, which makes this work
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
	
	private bool ValidateRequiredFields()
	{
		bool valid = true;
		
		
		Gdk.Color black = new Gdk.Color();
		Gdk.Color.Parse("black", ref black);
		
		List<Label> missingFields = new List<Label>();
		
		//Validate the rows
		foreach(CsvToLrMapRow row in MapRowsContainer)
		{
			if(row.Key.Contains("*") && 
			   	(row.DropDownValue == null || 
			 	(row.IsConstant && String.IsNullOrEmpty(row.ConstantValue))))
			{
				HBox container = (HBox)row.Children[0];
				missingFields.Add((Label)container.Children[0]);
				valid = false;
			}	
		}
		
		missingFields.AddRange(this.SignatureInformationWidget.GetMissingFields());
		missingFields.AddRange(this.ServerInfoWidget.GetMissingFields());
		
		if(this._missingFields != null)
		{
			foreach(var lbl in _missingFields)
			{
				if(missingFields.Where( x => x.LabelProp == lbl.LabelProp ).Count() == 0)
					lbl.ModifyFg(StateType.Normal, black);
			}
		}
		
		_missingFields = missingFields;
		
		return valid;
	}
	
	protected void ShowMissingFields()
	{
		Gdk.Color red = new Gdk.Color();
		Gdk.Color.Parse("red", ref red);
		
		foreach(var field in _missingFields)
			field.ModifyFg(StateType.Normal, red);
		
		PublishGUI.Helper.CreateNotficationWindow(PublishGUI.Helper.MSG_MISSING_FIELDS);
	}
	
	protected void PublishDocuments(object sender, EventArgs e)
	{
		bool validated = ValidateRequiredFields();
		if(validated)
		{
	        lr_Envelope envelope = buildEnvelopeFromMapping();
	
	        LRClient client = new LRClient(this.ServerInfoWidget.NodeUrl);
	        if (!String.IsNullOrEmpty(this.ServerInfoWidget.HttpUsername))
	        {
	            client.Username = ServerInfoWidget.HttpUsername;
	            client.Password = ServerInfoWidget.HttpPassword;
	        }
	        PublishResponse res = client.Publish(envelope);
			buildAndShowNotification(res);
		}
		else
			ShowMissingFields();
	}
	
	private void buildAndShowNotification(PublishResponse response)
	{
		StringBuilder sb = new StringBuilder();
		bool success =  response.OK;
		if(!response.OK)
				sb.AppendLine("General response error: " + response.error);
		foreach(DocPublishResult docResult in response.document_results)
		{
			if(!docResult.OK)
			{
				success = false;
				sb.AppendLine("Doc error: " + docResult.error);
			}
		}
        if(success)
		{
			PublishGUI.Helper.SavePublishHistory(this.ServerInfoWidget.NodeUrl, response);
			sb.AppendLine("Documents were successfully published!");
			sb.AppendLine("To view the docs online, or for further information from the responses,"
                         +"you may go to History -> Show All in the main menu.");
		}
		else
			sb.AppendLine("One or more errors occurred while publishing your documents. See above for detailed error report.");	
		
		PublishGUI.Helper.CreateNotficationWindow(sb.ToString());
	}
	
    private lr_Envelope buildEnvelopeFromMapping()
    {
		
		var sigInfo = this.SignatureInformationWidget;
		PgpSigner signer = null;
		bool needToSign = false;
		if(sigInfo.SignatureType == PublishGUI.SignatureType.LR_PGP)
		{
			signer = new PgpSigner(sigInfo.PgpPublicKeyLocations,
			                       sigInfo.PgpKeyringLocation,
			                       sigInfo.PgpSecretKeyPassphrase);
			needToSign = true;
		}
        Dictionary<string, string> map = new Dictionary<string, string>();
        FieldInfo[] infos = typeof(lr_document).GetFields();
        int j = 0;
        for (int i = 0; i < MapRowsContainer.Children.Length; i++)
        {
            var info = infos[j];
            var mapRow = (CsvToLrMapRow)MapRowsContainer.Children[i];

            var customAttributes = info.GetCustomAttributes(typeof(RequiredField), false);
            if (customAttributes.Length > 0)
            {
                RequiredField attr = (RequiredField)customAttributes[0];
                if (attr.Immutable)
                {
                    i--; //It was never added to the RowContainer, so we need to examine mapRow again with the next property
                    j++;  //We examined the property, increment this index
                    continue;
                }
            }

            if (nestedTypes.Contains(info.FieldType))
            {
                //Never added to RowContainer, decrement i and increment j
                if (info.FieldType == typeof(lr_digital_signature))
                {
                    i--;
                    j++;
                    continue;
                }

                foreach (var subInfo in info.FieldType.GetFields())
                {                  
                    mapRow = (CsvToLrMapRow)MapRowsContainer.Children[i++];
                    string key = String.Join(".", info.Name, subInfo.Name);
                    map[key] = mapRow.DropDownValue;
                }
                if(info.FieldType.GetFields().Length > 0)
                    i--; //avoid double incrementing from loop iterator statment
                j++;
            }
            else
                map[infos[j++].Name] = mapRow.DropDownValue;
        }

        //Create the docs from the map and the dataDict
        lr_Envelope envelope = new lr_Envelope();
        for (int i = 0; i < _rawRowDataList.Count; i++)
        {
            lr_document doc = new lr_document();
            int rowIndex = 0;
            foreach (var info in infos)
            {
                CsvToLrMapRow currentRow = (CsvToLrMapRow)MapRowsContainer.Children[rowIndex++];
				
				var customAttributes = info.GetCustomAttributes(typeof(RequiredField), false);
				if(customAttributes.Length > 0 )
				{
					RequiredField attr = (RequiredField)customAttributes[0];
                    if (attr.Immutable)
                    {
                        rowIndex--;
                        continue;
                    }
				}
                
                if (!nestedTypes.Contains(info.FieldType))
                {
                    string val;
                    if (currentRow.IsConstant)
                        val = currentRow.ConstantValue;
                    else if (currentRow.IsSerializeToRow)
                        val = _rawRowDataList[i];
                    else if (currentRow.DropDownValue == null)
                        continue; //Nothing to map to or assign if they did not choose a value from the dropdown
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
                    var subFields = info.FieldType.GetFields();
                    object currentObj = info.GetValue(doc);
                    if (currentObj == null)
                        currentObj = new object();

                    rowIndex--;

                    if (typeof(lr_digital_signature).Equals(info.FieldType))
                        continue;

                    foreach (var subField in subFields)
                    {
                        var rowToAdd = MapRowsContainer.Children.Cast<CsvToLrMapRow>()
                                            .Where(x =>
                                                        x.Key.Split('.')[0] == info.Name &&
                                                        x.Key.Split('.')[1] == subField.Name)
                                            .ToList()[0];

                        rowIndex++; //Used another row from subInfo, need to update the index

                        string val;
                        if (rowToAdd.IsConstant)
                            val = rowToAdd.ConstantValue;
                        else if (rowToAdd.IsSerializeToRow)
                            val = _rawRowDataList[i];
                        else if (rowToAdd.DropDownValue == null)
                            continue;
                        else
                            val = _rowData[map[String.Join(".", info.Name, subField.Name)]][i];

                        if (subField.FieldType == typeof(List<string>))
                        {
                            List<string> vals = val.Split(',').ToList<string>();
                            subField.SetValue(currentObj, vals);
                        }
                        else
                            subField.SetValue(currentObj, val);


                    }
                }
            }
			if(needToSign)
				doc = signer.Sign(doc);
			
            envelope.documents.Add(doc);
        }
        return envelope;
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


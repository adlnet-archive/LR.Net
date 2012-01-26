using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using Gtk;
using LearningRegistry.RDDD;


public partial class CsvToLrWindow : Gtk.Window
{
	private string _csvPath;
	private StreamReader _sReader;
	
	
	public CsvToLrWindow () : 
			base(Gtk.WindowType.Toplevel)
	{
		
		this.Build ();
	}
	
	private string[] getColumnsFromCsv()
	{	
		string headerLine = _sReader.ReadLine();
		string[] headers = headerLine.Split('\t');
		return headers;
	}
	
	public void PopulateFromCsv(string path)
	{
		_csvPath = path;
		_sReader = new StreamReader(_csvPath);
		
		string[] columns = getColumnsFromCsv();
		
		FieldInfo[] infos = typeof(lr_document).GetFields();
		foreach(var info in infos)
		{
			List<Type> nestedTypes = new List<Type>()
			{
				typeof(lr_identity),
				typeof(lr_TOS),
				typeof(lr_digital_signature)
			};
			if(nestedTypes.Contains(info.FieldType))
			{
				//Nesting only goes one level, which makes this work
				foreach(var subInfo in info.FieldType.GetFields())
				{
					string name = String.Join(".", info.Name, subInfo.Name);
					if(subInfo.GetCustomAttributes(typeof(LearningRegistry.RequiredField), true).Length > 0)
						name = name.Insert(0, "*");
					MapRowsContainer.Add(new CsvToLrMapRow(columns, name));
				}
			}
			else
			{
				string name = info.Name;
				if(info.GetCustomAttributes(typeof(LearningRegistry.RequiredField), true).Length > 0)
					name = name.Insert(0, "*");
				CsvToLrMapRow row = new CsvToLrMapRow(columns, name);
				MapRowsContainer.Add(row);
			}
		}
	}
}


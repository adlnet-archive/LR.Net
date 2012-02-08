using System;
using System.IO;

public partial class HistoryWindow : Gtk.Window
{
	public HistoryWindow () : 
			base(Gtk.WindowType.Toplevel)
	{
		this.Build ();
		populate();	
	}
	
	private void populate()
	{
		using(StreamReader sr = new StreamReader(".history"))
		{
			while(!sr.EndOfStream)
			{
				//Column 0: lr node published to
				//Column 1: docId
				//Column 2: publish date
				string[] items = sr.ReadLine().Split(' ');
				string url = String.Format("{0}/obtain?by_doc_ID=true&request_id={1}", items[0], items[1]);
				string dateString = DateTime.Parse(items[2]).ToShortDateString();
				
				Gtk.LinkButton lb = new Gtk.LinkButton(url, "View record in browser");
				lb.Clicked += (sender, e) => System.Diagnostics.Process.Start(url);
				Gtk.HBox row = new Gtk.HBox();
				row.Add(new Gtk.Label(dateString));
				row.Add(new Gtk.Label(items[1]));
				row.Add(lb);
				
				HistoryTable.Add(row);
			}
		}
	}
	
}



using System;
using System.IO;
using Gtk;

using LearningRegistry;
using LearningRegistry.RDDD;

namespace PublishGUI
{
	public static class Helper
	{
		public const string MSG_MISSING_FIELDS = "One or more fields are missing that are required to publish your data. Please check" +
												 " the fields highlighted in red and try again";
		
		public static void SavePublishHistory(string publishDestinationUrl, PublishResponse response)
		{
			string timestamp = DateTime.Now.ToString("yyyy-MM-ddThh:mm:ssZ");
			using(FileStream fs = new FileStream(".history", FileMode.Append))
				using(StreamWriter writer = new StreamWriter(fs))
					foreach(DocPublishResult result in response.document_results)
							writer.WriteLine(String.Format("{0} {1} {2}", publishDestinationUrl,
						                               result.doc_ID, timestamp));
		}
		
		public static void CreateNotficationWindow(string notification)
		{
			Gtk.Window nWin = new Gtk.Window(WindowType.Toplevel);
			Gtk.VBox ctr = new Gtk.VBox();
			ctr.Add( new Gtk.Label(notification));
			Gtk.Button btn = new Gtk.Button();
			btn.Label = "Close";
			btn.Clicked += (sender, e) => nWin.Destroy();
			ctr.Add(btn);
			nWin.Add(ctr);
			nWin.ShowAll();
		}
	}
}


using System;

namespace PublishGUI
{
	public partial class CsvToLrWindow : Gtk.Window
	{
		public CsvToLrWindow () : 
				base(Gtk.WindowType.Toplevel)
		{
			this.Build ();
		}
	}
}


// 
// MainWindow.cs
//  
// Author:
//       Trent McPheron <twilightinzero@gmail.com>
// 
// Copyright (c) 2009 Trent McPheron
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using Gtk;
using Glade;

namespace Arbiter
{
	// The main window of the program.
	public class MainWindow
	{
		#region Widgets
		// Widgets to be attached by Glade#.
		[Widget] private Window mainWin;
		[Widget] private Notebook duels;
		[Widget] private ComboBoxEntry duelistAName;
		[Widget] private ComboBoxEntry duelistBName;
		[Widget] private ComboBoxEntry ringName;
		[Widget] private ComboBox duelVenue;
		[Widget] private ComboBox duelType;
		[Widget] private CheckButton fightNight;
		[Widget] private ComboBox tabPosition;
		[Widget] private FileChooserButton logDirectory;
		[Widget] private Image newDuelTabIcon;
		[Widget] private Image duelistAEditImage;
		[Widget] private Image duelistBEditImage;
		[Widget] private Image ringEditImage;
		[Widget] private HBox newDuelTabBox;
		[Widget] private TextView shiftReportTextView;
		#endregion
		
		// Constructor.
		public MainWindow()
		{
			// Load the Glade file.
			XML xml = new XML("Arbiter.GUI.glade", "mainWin");
			xml.Autoconnect(this); 
			
			#region Settings
			// Set default size.
			mainWin.DefaultWidth = MainClass.WindowWidth;
			mainWin.DefaultHeight = MainClass.WindowHeight;
			
			// Use settings to determine tab location.
			switch (MainClass.TabPosition)
			{
			case "Left":
				tabPosition.Active = 0;
				duels.TabPos = PositionType.Left;
				break;
			case "Right":
				tabPosition.Active = 1;
				duels.TabPos = PositionType.Right;
				break;
			case "Top":
				tabPosition.Active = 2;
				duels.TabPos = PositionType.Top;
				break;
			case "Bottom":
				tabPosition.Active = 3;
				duels.TabPos = PositionType.Bottom;
				break;
			}
			
			// Set log directory path in the folder chooser.
			logDirectory.SetCurrentFolder(MainClass.LogDirectory);
			#endregion
			
			#region Widgets
			// Set the window and new duel tab icon.
			mainWin.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
			newDuelTabIcon.Pixbuf = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
			
			// For the popup menu.
			duels.SetMenuLabelText(duels.CurrentPageWidget, "New Duel");
			
			// Set the icons for the three edit buttons. Necessary on Windows.
			duelistAEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			duelistBEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			ringEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			
			// Connect the list stores to their comboboxes.
			duelistAName.Model = MainClass.Duelists;
			duelistBName.Model = MainClass.Duelists;
			ringName.Model = MainClass.Rings;
			duelistAName.TextColumn = 0;
			duelistBName.TextColumn = 0;
			ringName.TextColumn = 0;
			
			// Autocompletion for the comboboxes.
			EntryCompletion[] c = new EntryCompletion[3];
			ListStore[] l = new ListStore[]
				{ MainClass.Duelists, MainClass.Duelists, MainClass.Rings };
			ComboBoxEntry[] e = new ComboBoxEntry[]
				{ duelistAName, duelistBName, ringName };
			for (int n = 0; n < 3; n++)
			{
				c[n] = new EntryCompletion();
				c[n].Model = l[n];
				c[n].TextColumn = 0;
				c[n].InlineCompletion = true;
				c[n].InlineSelection = true;
				c[n].PopupSingleMatch = false;
				e[n].Entry.Completion = c[n];
			}
			
			// Prepare shift report displayer.
			MainClass.ShiftReport = shiftReportTextView.Buffer;
			
			// For whatever reason, Glade doesn't automatically connect this one.
			logDirectory.SelectionChanged += SaveLogDirectory;
			
			// And for whatever reason, the comboboxes don't start at defaults.
			duelVenue.Active = 0;
			duelType.Active = 0;
			
			// We may have restored from a fight night shift report.
			fightNight.Active = MainClass.FightNight;
			#endregion
		}
		
		// Just for convenience.
		public void Show ()
			{ mainWin.ShowAll(); }
		
		// Save settings then quit.
		private void AppClose (object sender, DeleteEventArgs a)
		{
			MainClass.SaveSettings();
			Application.Quit ();
			a.RetVal = true;
		}
		
		// Starts a duel according to the provided settings.
		private void StartDuel (object sender, System.EventArgs e)
		{
			// Make sure the directory exists.
			MainClass.CreateShiftReport();
			
			#region Failsafe
			// First of all, we have to make sure the user
			// didn't mess up.
			if(duelistAName.ActiveText == duelistBName.ActiveText ||
			   duelistAName.ActiveText == "" ||
			   duelistBName.ActiveText == "" ||
			   ringName.ActiveText == "")
			{
				// Let the user know they messed up.
				Dialog dialog = new Dialog("Superman will--", mainWin,
			                           DialogFlags.NoSeparator | DialogFlags.Modal,
			                           new object[] {Stock.Ok, ResponseType.Ok});
				dialog.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
				Label dlabel = new Label();
				dlabel.Markup = "<span size='32768'><b>WRONG!!!</b></span>";
				dlabel.SetAlignment(0.5f, 0.5f);
				dlabel.Justify = Justification.Center;
				dialog.VBox.PackStart(dlabel);
				dialog.Default = dialog.ActionArea.Children[0];
				dialog.VBox.ShowAll();
				dialog.Run();
				dialog.Destroy();
				
				// End the method.
				return;
			}
			#endregion
			
			#region Venue and Type
			// For duel venue.
			Venue venue = new Venue();
			switch (duelVenue.ActiveText)
			{
				case "Swords": venue = MainClass.DuelOfSwords; break;
				case "Fists":  venue = MainClass.DuelOfFists;  break;
				case "Magic":  venue = MainClass.DuelOfMagic;  break;
			}
			
			// For duel type.
			bool overtime = false;
			bool madness = false;
			switch (duelType.ActiveText)
			{
				case "Challenge": overtime = true; madness = false; break;
				case "Madness":   overtime = true; madness = true; break;
			}
			#endregion
			
			#region Tab Label
			HBox label = new HBox();
			label.Spacing = 2;
			
			// Close button.
			EventBox closeButton = new EventBox();
			Gdk.Pixbuf pb = IconTheme.Default.LoadIcon(Stock.Close, 16, 0);
			closeButton.Add(new Image(pb.ScaleSimple(12, 12, Gdk.InterpType.Hyper)));
			closeButton.VisibleWindow = false;
			
			// Tab icon.
			Gdk.Pixbuf icon = Gdk.Pixbuf.LoadFromResource(
					"Arbiter." + venue.ShortName + duelType.ActiveText + ".png");
			icon = icon.ScaleSimple(16, 16, Gdk.InterpType.Hyper);
			
			// Pack the hbox and show it.
			label.PackStart(new Image(icon), false, false, 0);
			label.PackStart(new Label(ringName.ActiveText), true, true, 0);
			label.PackEnd(closeButton, false, false, 0);
			label.ShowAll();
			#endregion
			
			#region Start Duel
			// Make the tab.
			MainClass.NumDuels++;
			Duel duel = new Duel(MainClass.NumDuels, ringName.ActiveText,
								duelistAName.ActiveText, duelistBName.ActiveText,
								venue, overtime, madness);
			//Widget duelWidget = duel.DuelWidget;
			duels.InsertPage(duel, label, duels.NPages - 1);
			duels.ShowAll();
			duels.CurrentPage = duels.NPages - 2;
			duels.CurrentPageWidget.Name = duelistAName.ActiveText;
			
			// For the popup menu.
			duels.SetMenuLabelText(duels.CurrentPageWidget, ringName.ActiveText);
			
			// Make the close button work.
			closeButton.ButtonReleaseEvent += delegate { duels.Remove(duel); };
			#endregion
			
			#region List Update
			// This will automatically add new duelists to the duelist list.
			bool presentA = false;
			bool presentB = false;
			foreach(object[] s in MainClass.Duelists)
			{
				presentA = presentA || (string)s[0] == duelistAName.ActiveText;
				presentB = presentB || (string)s[0] == duelistBName.ActiveText;
			}
			if (!presentA) MainClass.Duelists.AppendValues(
										duelistAName.ActiveText);
			if (!presentB) MainClass.Duelists.AppendValues(
										duelistBName.ActiveText);
			#endregion
			
			#region Cleanup
			// Clear the widgets.
			duelistAName.Entry.Text = "";
			duelistBName.Entry.Text = "";
			ringName.Entry.Text = "";
			if (MainClass.FightNight) duelVenue.Active = 0;
			duelType.Active = 0;
			#endregion
		}
		
		#region Other Widgets
		// Update tab position and store the setting.
		private void SetTabPosition (object sender, System.EventArgs e)
		{
			MainClass.TabPosition = tabPosition.ActiveText;
			switch (tabPosition.ActiveText)
			{
			case "Left":
				duels.TabPos = PositionType.Left;
				break;
			case "Right":
				duels.TabPos = PositionType.Right;
				break;
			case "Top":
				duels.TabPos = PositionType.Top;
				break;
			case "Bottom":
				duels.TabPos = PositionType.Bottom;
				break;
			}
		}
		
		// Store the log directory setting.
		private void SaveLogDirectory (object sender, System.EventArgs e)
			{ MainClass.LogDirectory = logDirectory.Filename; }

		// Store the size setting.
		private void SaveSize (object o, Gtk.SizeAllocatedArgs args)
		{
			int width, height;
			mainWin.GetSize(out width, out height);
			MainClass.WindowWidth = width;
			MainClass.WindowHeight = height;
		}
		
		// Toggle the Fight Night switch.
		private void FightNightToggled (object sender, System.EventArgs e)
			{ MainClass.FightNight = fightNight.Active; }
		
		// Make the expander fill the window when expanded.
		private void ExpandExpander (object sender, System.EventArgs e)
		{
			Expander ex = (Expander)sender;
			VBox box = (VBox)(ex.Parent);
			box.SetChildPacking(ex, ex.Expanded, true, 0, PackType.Start);
		}
		
		// Create a window to edit saved duelists.
		private void EditDuelists (object sender, System.EventArgs e)
		{
			EditDialog ed = new EditDialog("Edit Duelists", MainClass.Duelists, false);
			ed.Run();
		}
		
		// Create a window to edit saved ring names.
		private void EditRings (object sender, System.EventArgs e)
		{
			EditDialog ed = new EditDialog("Edit Rings", MainClass.Rings, true);
			ed.Run();
		}
		#endregion
	}
}

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
using System.IO;
using Gtk;
using Glade;

namespace Arbiter
{
	// The main window of the program.
	public class MainWindow : Window
	{
		private static MainWindow instance;
		
		#region Widgets
		// Widgets to be attached by Glade#.
		//[Widget] private Window mainWin;
		[Widget] private VBox mainWidget;
		[Widget] private Notebook duelNotebook;
		[Widget] private ComboBoxEntry duelistANameCEntry;
		[Widget] private ComboBoxEntry duelistBNameCEntry;
		[Widget] private ComboBoxEntry ringNameCEntry;
		[Widget] private ComboBox sportCombo;
		[Widget] private ComboBox typeCombo;
		[Widget] private CheckMenuItem fightNightCheckMenuItem;
		[Widget] private Image duelistAEditImage;
		[Widget] private Image duelistBEditImage;
		[Widget] private Image ringEditImage;
		[Widget] private TextView shiftReportView;
		[Widget] private MenuItem duelMenuItem;
		[Widget] private ImageMenuItem saveReportMenuItem;
		[Widget] private ImageMenuItem quitMenuItem;
		[Widget] private ImageMenuItem resolveMenuItem;
		[Widget] private ImageMenuItem undoMenuItem;
		[Widget] private CheckMenuItem manualMenuItem;
		[Widget] private ImageMenuItem saveLogMenuItem;
		[Widget] private ImageMenuItem endDuelMenuItem;
		[Widget] private ImageMenuItem closeMenuItem;
		[Widget] private ImageMenuItem logDirectoryMenuItem;
		[Widget] private RadioMenuItem leftVertTabMenuItem;
		[Widget] private RadioMenuItem rightVertTabMenuItem;
		[Widget] private RadioMenuItem leftTabMenuItem;
		[Widget] private RadioMenuItem rightTabMenuItem;
		[Widget] private RadioMenuItem topTabMenuItem;
		[Widget] private RadioMenuItem bottomTabMenuItem;
		[Widget] private CheckMenuItem hideIconsMenuItem;
		[Widget] private CheckMenuItem hideCloseMenuItem;
		[Widget] private CheckMenuItem hideButtonsMenuItem;
		[Widget] private CheckMenuItem smallScoreMenuItem;
		#endregion
		
		// Convenience property.
		private Duel CurrentDuel
			{ get { return (Duel)duelNotebook.CurrentPageWidget; } }
		
		// Constructor.
		public MainWindow() : base("Arbiter")
		{
			// Load the Glade file.
			XML xml = new XML("MainWindow.glade", "mainWidget");
			xml.Autoconnect(this);
			Add(mainWidget);
			
			#region Settings
			// Set default window properties.
			DefaultWidth = Arbiter.WindowWidth;
			DefaultHeight = Arbiter.WindowHeight;
			WindowPosition = WindowPosition.Center;
			
			// Connect signals.
			SizeAllocated += SaveSize;
			DeleteEvent += QuitArbiter;
			
			// Anonymous reordering delegate.
			duelNotebook.PageReordered += delegate(object sender, PageReorderedArgs args) {
				if (args.P1 == duelNotebook.NPages - 1)
					duelNotebook.ReorderChild(args.P0, duelNotebook.NPages - 2);
			};
			
			// Use settings to determine tab orientation.
			switch (Arbiter.TabPosition)
			{
			case "LeftVert":
			case "RightVert":
				Arbiter.VertTabs = true;
				break;
			default:
				Arbiter.VertTabs = false;
				break;
			}
			
			// Create tab label.
			Box newDuelTab;
			if (Arbiter.VertTabs) newDuelTab = new VBox();
			else newDuelTab = new HBox();
			newDuelTab.Spacing = 2;
			Label newDuelTabLabel = new Label("New");
			EventBox dummyBox = new EventBox();
			dummyBox.VisibleWindow = false;
			dummyBox.AboveChild = true;
			
			// Packing type depends on orientation.
			if (Arbiter.VertTabs)
			{
				newDuelTab.PackEnd(new Image(Gdk.Pixbuf.LoadFromResource("RoH.png")),
			                       false, false, 0);
				newDuelTab.PackEnd(newDuelTabLabel, true, true, 0);
				newDuelTab.PackEnd(dummyBox, false, false, 0);
				newDuelTabLabel.Angle = 90.0;
			}
			else
			{
				newDuelTab.PackStart(new Image(Gdk.Pixbuf.LoadFromResource("RoH.png")),
			                       false, false, 0);
				newDuelTab.PackStart(newDuelTabLabel, true, true, 0);
				newDuelTab.PackStart(dummyBox, false, false, 0);
				newDuelTabLabel.Angle = 0.0;
			}
			newDuelTab.ShowAll();
			duelNotebook.SetTabLabel(duelNotebook.CurrentPageWidget, newDuelTab);
			
			// Hide parts of the new duel tab label depending on settings.
			if (Arbiter.VertTabs)
			{
				newDuelTab.Children[2].Visible = !Arbiter.HideIcons;
				newDuelTab.Children[2].NoShowAll = Arbiter.HideIcons;
				newDuelTab.Children[0].Visible = !Arbiter.HideClose;
				newDuelTab.Children[0].NoShowAll = Arbiter.HideClose;
			}
			else
			{
				newDuelTab.Children[0].Visible = !Arbiter.HideIcons;
				newDuelTab.Children[0].NoShowAll = Arbiter.HideIcons;
				newDuelTab.Children[2].Visible = !Arbiter.HideClose;
				newDuelTab.Children[2].NoShowAll = Arbiter.HideClose;
			}
			
			// Use settings to determine tab location.
			switch (Arbiter.TabPosition)
			{
			case "LeftVert":
				leftVertTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Left;
				break;
			case "RightVert":
				rightVertTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Right;
				break;
			case "Left":
				leftTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Left;
				break;
			case "Right":
				rightTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Right;
				break;
			case "Top":
				topTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Top;
				break;
			case "Bottom":
				bottomTabMenuItem.Active = true;
				duelNotebook.TabPos = PositionType.Bottom;
				break;
			}
			
			// Toggle checkboxes in compact mode menu.
			hideIconsMenuItem.Active = Arbiter.HideIcons;
			hideCloseMenuItem.Active = Arbiter.HideClose;
			hideButtonsMenuItem.Active = Arbiter.HideButtons;
			smallScoreMenuItem.Active = Arbiter.SmallScore;
			#endregion
			
			#region Widgets
			// Set window icon.
			this.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			
			// For the popup menu.
			duelNotebook.SetMenuLabelText(duelNotebook.CurrentPageWidget, "New Duel");
			
			// Set the icons for the three edit buttons. Necessary on Windows.
			duelistAEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			duelistBEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			ringEditImage.Pixbuf = IconTheme.Default.LoadIcon(Stock.Edit, 16, 0);
			
			// Connect the list stores to their comboboxes.
			duelistANameCEntry.Model = Arbiter.Duelists;
			duelistBNameCEntry.Model = Arbiter.Duelists;
			ringNameCEntry.Model = Arbiter.Rings;
			duelistANameCEntry.TextColumn = 0;
			duelistBNameCEntry.TextColumn = 0;
			ringNameCEntry.TextColumn = 0;
			
			// Autocompletion for the comboboxes.
			EntryCompletion[] c = new EntryCompletion[3];
			ListStore[] l = new ListStore[]
				{ Arbiter.Duelists, Arbiter.Duelists, Arbiter.Rings };
			ComboBoxEntry[] e = new ComboBoxEntry[]
				{ duelistANameCEntry, duelistBNameCEntry, ringNameCEntry };
			for (int n = 0; n < 3; n++)
			{
				c[n] = new EntryCompletion();
				c[n].Model = l[n];
				c[n].TextColumn = 0;
				c[n].InlineCompletion = true;
				c[n].InlineSelection = true;
				c[n].PopupSingleMatch = true;
				if (n < 2) c[n].MatchFunc = DuelistMatchFunc;
				e[n].Entry.Completion = c[n];
			}
			
			// Prepare shift report displayer.
			shiftReportView.Buffer = Arbiter.ShiftReport;
			
			// For whatever reason, the comboboxes don't start at defaults.
			sportCombo.Active = 0;
			typeCombo.Active = 0;
			
			// We may have restored from a fight night shift report.
			fightNightCheckMenuItem.Active = Arbiter.FightNight;
			
			// Create accelerators for the menu items.
			AccelGroup ag = new AccelGroup();
			this.AddAccelGroup(ag);
			saveReportMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.S, Gdk.ModifierType.ControlMask |
							Gdk.ModifierType.ShiftMask, AccelFlags.Visible);
			quitMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.Q, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			resolveMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.R, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			undoMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.Z, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			saveLogMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.S, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			endDuelMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.E, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			closeMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.Escape, Gdk.ModifierType.None, AccelFlags.Visible);
			logDirectoryMenuItem.AddAccelerator("activate", ag,
					(uint)Gdk.Key.L, Gdk.ModifierType.ControlMask, AccelFlags.Visible);
			
			// Toggle sensitivity of duel menu and its items.
			CheckDuelMenu();
			
			// Initialize shift report.
			if (Arbiter.NumDuels == 0) Arbiter.CreateShiftReport();
			#endregion
			
			// Save instance.
			instance = this;
		}
		
		// Starts a duel according to the provided settings.
		private void StartDuel (object sender, EventArgs args)
		{	
			#region Failsafe
			// First of all, we have to make sure the user
			// didn't mess up.
			if(duelistANameCEntry.ActiveText == duelistBNameCEntry.ActiveText ||
			   duelistANameCEntry.ActiveText == "" ||
			   duelistBNameCEntry.ActiveText == "" ||
			   ringNameCEntry.ActiveText == "")
			{
				// Let the user know they messed up.
				Dialog dialog = new Dialog("Superman will--", this,
			                           DialogFlags.NoSeparator | DialogFlags.Modal,
			                           new object[] {Stock.Ok, ResponseType.Ok});
				dialog.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
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
			
			#region Sport and Type
			// For duel sport.
			Sport sport = new Sport();
			switch (sportCombo.ActiveText)
			{
				case "Swords": sport = Arbiter.DuelOfSwords; break;
				case "Fists":  sport = Arbiter.DuelOfFists;  break;
				case "Magic":  sport = Arbiter.DuelOfMagic;  break;
			}
			
			// For duel type.
			bool overtime = false;
			bool madness = false;
			switch (typeCombo.ActiveText)
			{
				case "Challenge": overtime = true; madness = false; break;
				case "Madness":   overtime = true; madness = true; break;
			}
			#endregion
			
			#region Tab Label
			// Determine orientation of label.
			Box tabLabel;
			if (Arbiter.VertTabs) tabLabel = new VBox();
			else tabLabel = new HBox();
			tabLabel.Spacing = 2;
			
			// Tab icon.
			Gdk.Pixbuf icon = Gdk.Pixbuf.LoadFromResource(
					"Arbiter." + sport.ShortName + typeCombo.ActiveText + ".png");
			icon = icon.ScaleSimple(16, 16, Gdk.InterpType.Hyper);
			
			// Label.
			Label label = new Label(ringNameCEntry.ActiveText);
			if (Arbiter.VertTabs) label.Angle = 90.0;
			
			// Close button.
			EventBox closeButton = new EventBox();
			Gdk.Pixbuf pb = IconTheme.Default.LoadIcon(Stock.Close, 16, 0);
			closeButton.Add(new Image(pb.ScaleSimple(12, 12, Gdk.InterpType.Hyper)));
			closeButton.VisibleWindow = false;
			
			// Pack the box and show it.
			if (Arbiter.VertTabs)
			{
				tabLabel.PackEnd(new Image(icon), false, false, 0);
				tabLabel.PackEnd(label, true, true, 0);
				tabLabel.PackEnd(closeButton, false, false, 0);
			}
			else
			{
				tabLabel.PackStart(new Image(icon), false, false, 0);
				tabLabel.PackStart(label, true, true, 0);
				tabLabel.PackStart(closeButton, false, false, 0);
			}
			tabLabel.ShowAll();
			
			// Hide parts of the label depending on settings.
			if (Arbiter.VertTabs)
			{
				tabLabel.Children[2].Visible = !Arbiter.HideIcons;
				tabLabel.Children[2].NoShowAll = Arbiter.HideIcons;
				tabLabel.Children[0].Visible = !Arbiter.HideClose;
				tabLabel.Children[0].NoShowAll = Arbiter.HideClose;
			}
			else
			{
				tabLabel.Children[0].Visible = !Arbiter.HideIcons;
				tabLabel.Children[0].NoShowAll = Arbiter.HideIcons;
				tabLabel.Children[2].Visible = !Arbiter.HideClose;
				tabLabel.Children[2].NoShowAll = Arbiter.HideClose;
			}
			#endregion
			
			#region Start Duel
			// Make the tab.
			Duel duel = new Duel(ringNameCEntry.ActiveText,
			                     duelistANameCEntry.ActiveText,
			                     duelistBNameCEntry.ActiveText,
			                     sport, overtime, madness);
			duelNotebook.InsertPage(duel, tabLabel, duelNotebook.NPages - 1);
			duelNotebook.ShowAll();
			duelNotebook.CurrentPage = duelNotebook.NPages - 2;
			
			// For the popup menu.
			duelNotebook.SetMenuLabelText(duel, ringNameCEntry.ActiveText);
			
			// Enable drag-and-drop reordering.
			duelNotebook.SetTabReorderable(duel, true);
			
			// Make the close button work.
			closeButton.ButtonReleaseEvent += delegate { duelNotebook.Remove(duel); };
			#endregion
			
			#region List Update
			// This will automatically add new duelists to the duelist list.
			bool presentA = false;
			bool presentB = false;
			foreach(object[] s in Arbiter.Duelists)
			{
				presentA = presentA || (string)s[0] == duelistANameCEntry.ActiveText;
				presentB = presentB || (string)s[0] == duelistBNameCEntry.ActiveText;
			}
			if (!presentA) Arbiter.Duelists.AppendValues(
										duelistANameCEntry.ActiveText);
			if (!presentB) Arbiter.Duelists.AppendValues(
										duelistBNameCEntry.ActiveText);
			#endregion
			
			#region Cleanup
			// Clear the widgets.
			duelistANameCEntry.Entry.Text = "";
			duelistBNameCEntry.Entry.Text = "";
			ringNameCEntry.Entry.Text = "";
			if (Arbiter.FightNight) sportCombo.Active = 0;
			typeCombo.Active = 0;
			
			// And now that we're running a duel, disable Fight Night
			// toggle. This keeps our internal duel lists from getting
			// mixed up.
			fightNightCheckMenuItem.Sensitive = false;
			#endregion
		}
		
		#region Other Widgets
		// Custom entry completion match function.
		private bool DuelistMatchFunc (EntryCompletion comp, string key, TreeIter iter)
			{ return ((string)comp.Model.GetValue(iter, 0)).ToLower().Contains(key.ToLower()); }
		
		// Toggle the Fight Night switch.
		private void FightNightToggled (object sender, EventArgs args)
			{ Arbiter.FightNight = fightNightCheckMenuItem.Active; }
		
		// Create a window to edit saved duelists.
		private void EditDuelists (object sender, EventArgs args)
		{
			EditDialog ed = new EditDialog("Edit Duelists", Arbiter.Duelists, false);
			ed.Run();
		}
		
		// Create a window to edit saved ring names.
		private void EditRings (object sender, EventArgs e)
		{
			EditDialog ed = new EditDialog("Edit Rings", Arbiter.Rings, true);
			ed.Run();
		}

		// Store the size setting.
		private void SaveSize (object sender, SizeAllocatedArgs args)
		{
			if (this.Child == mainWidget)
			{
				int width, height;
				this.GetSize(out width, out height);
				Arbiter.WindowWidth = width;
				Arbiter.WindowHeight = height;
			}
		}
		#endregion
		
		#region Shift Menu
		// Save the shift report to a file.
		private void SaveShiftReport (object sender, EventArgs args)
		{
			// Prompt the user to pick a file.
			FileChooserDialog fc = new FileChooserDialog(
										"Save Shift Report As...",
										null, FileChooserAction.Save,
										new object[] {Stock.Cancel, ResponseType.Cancel,
													Stock.SaveAs, ResponseType.Accept});
			fc.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			fc.Modal = true;
			int r = fc.Run();
			if (r != (int)ResponseType.Accept)
			{
				fc.Destroy();
				return;
			}
			string path = fc.Filename;
			fc.Destroy();
			
			// Open the file and write the contents of the buffer to it.
			StreamWriter sw = new StreamWriter(path, false);
			sw.Write(Arbiter.ShiftReport.Text);
			sw.Close();
		}
		
		// Start a brawl.
		private void StartBrawl (object sender, EventArgs args)
		{
			BrawlSetup bs = new BrawlSetup();
			ReplaceWidget(bs);
			bs.ShowAll();
		}
		
		// Save settings then quit.
		private void QuitArbiter (object sender, EventArgs args)
		{
			Arbiter.SaveSettings();
			Application.Quit();
		}
		private void QuitArbiter (object sender, DeleteEventArgs args)
		{
			Arbiter.SaveSettings();
			Application.Quit();
			args.RetVal = true;
		}
		#endregion
		
		#region Duel Menu
		// Toggles sensitivity of certain items in the menu.
		public static void SCheckDuelMenu()
			{ instance.CheckDuelMenu(); }
		public void CheckDuelMenu ()
		{
			duelMenuItem.Sensitive = (duelNotebook.CurrentPage < duelNotebook.NPages - 1);
			resolveMenuItem.Sensitive = false;
			undoMenuItem.Sensitive = false;
			endDuelMenuItem.Sensitive = false;
			if (duelMenuItem.Sensitive)
			{
				resolveMenuItem.Sensitive = CurrentDuel.CanResolve;
				undoMenuItem.Sensitive = CurrentDuel.CanUndo;
				endDuelMenuItem.Sensitive = !CurrentDuel.End;
				manualMenuItem.Active = CurrentDuel.Manual;
			}
			closeMenuItem.Sensitive = duelMenuItem.Sensitive;
			saveLogMenuItem.Sensitive = duelMenuItem.Sensitive;
		}
		private void CheckDuelMenu (object sender, EventArgs args)
			{ CheckDuelMenu(); }
		private void CheckDuelMenu (object sender, SwitchPageArgs args)
			{ CheckDuelMenu(); }
		
		// Tell the selected duel to resolve.
		private void ResolveRound (object sender, EventArgs args)
			{ CurrentDuel.ResolveRound(sender, args); }
		
		// Tell the selected duel to undo its last round.
		private void UndoRound (object sender, EventArgs args)
			{ CurrentDuel.UndoRound(sender, args); }
		
		// Toggles Manual Mode on the selected duel.
		private void ToggleManual (object sender, EventArgs args)
			{ CurrentDuel.Manual = manualMenuItem.Active; }
		
		// Saves the duel log to a file of the user's choice.
		private void SaveDuelLog (object sender, EventArgs args)
			{ CurrentDuel.SaveDuelAs(); }
		
		// Prematurely end a duel.
		private void EndDuel (object sender, EventArgs args)
			{ CurrentDuel.EndDuel(); }
		
		// Close the duel tab.
		private void CloseDuel (object sender, EventArgs args)
		{ if (duelNotebook.CurrentPage < duelNotebook.NPages - 1)
			duelNotebook.Remove(duelNotebook.CurrentPageWidget); }
		#endregion
		
		#region Options Menu
		// Set the log directory and store it. Also moves the current date
		// directory to the new location.
		private void SetLogDirectory (object sender, EventArgs args)
		{
			// Prompt the user to pick a directory to store duels in.
			FileChooserDialog fc = new FileChooserDialog(
										"Select Log Directory",
										null, FileChooserAction.SelectFolder,
										new object[] {Stock.Cancel, ResponseType.Cancel,
													Stock.Save, ResponseType.Accept});
			fc.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			fc.Modal = true;
			int r = fc.Run();
			if (r != (int)ResponseType.Accept)
			{
				fc.Destroy();
				return;
			}
			
			// Save the old current dir.
			string oldDir = Arbiter.CurrentDir;
			
			// Save the new log dir.
			Arbiter.LogDirectory = fc.Filename;
			
			// Move the current date directory to the new location.
			Directory.Move(oldDir, Arbiter.CurrentDir);
			
			// Bye, dialog.
			fc.Destroy();
		}
		
		// Update tab position and store the setting.
		private void SetTabPosition (object sender, EventArgs args)
		{
			bool newVertTabs = false;
			
			// Update settings.
			if (leftVertTabMenuItem.Active)
			{
				Arbiter.TabPosition = "LeftVert";
				duelNotebook.TabPos = PositionType.Left;
				newVertTabs = true;
			}
			else if (rightVertTabMenuItem.Active)
			{
				Arbiter.TabPosition = "RightVert";
				duelNotebook.TabPos = PositionType.Left;
				newVertTabs = true;
			}
			else if (leftTabMenuItem.Active)
			{
				Arbiter.TabPosition = "Left";
				duelNotebook.TabPos = PositionType.Left;
				newVertTabs = false;
			}
			else if (rightTabMenuItem.Active)
			{
				Arbiter.TabPosition = "Right";
				duelNotebook.TabPos = PositionType.Right;
				newVertTabs = false;
			}
			else if (topTabMenuItem.Active)
			{
				Arbiter.TabPosition = "Top";
				duelNotebook.TabPos = PositionType.Top;
				newVertTabs = false;
			}
			else if (bottomTabMenuItem.Active)
			{
				Arbiter.TabPosition = "Bottom";
				duelNotebook.TabPos = PositionType.Bottom;
				newVertTabs = false;
			}
			
			// If the tab orientation is changed, recreate labels.
			if (newVertTabs != Arbiter.VertTabs)
			for (int i = 0; i < duelNotebook.NPages; i++)
			{
				// Create the new label.
				Box newLabel;
				if (newVertTabs) newLabel = new VBox();
				else newLabel = new HBox();
				newLabel.Spacing = 2;
				
				// Get the old label.
				Box oldLabel = (Box)duelNotebook.GetTabLabel(duelNotebook.Children[i]);
				
				// Store the children.
				Widget child0 = oldLabel.Children[0];
				Label child1 = (Label)oldLabel.Children[1];
				Widget child2 = oldLabel.Children[2];
				
				// Remove them from the old label.
				oldLabel.Remove(child0);
				oldLabel.Remove(child1);
				oldLabel.Remove(child2);
				
				// Add them to the new label. Always pack end
				// because they go in the opposite order from
				// before.
				if (newVertTabs)
				{
					newLabel.PackEnd(child0, false, false, 0);
					newLabel.PackEnd(child1, true, true, 0);
					newLabel.PackEnd(child2, false, false, 0);
					child1.Angle = 90.0;
				}
				else
				{
					newLabel.PackEnd(child0, false, false, 0);
					newLabel.PackEnd(child1, true, true, 0);
					newLabel.PackEnd(child2, false, false, 0);
					child1.Angle = 0;
				}
				
				// Show the tab label and set it.
				newLabel.ShowAll();
				duelNotebook.SetTabLabel(duelNotebook.Children[i], newLabel);
			}
			
			// Save new orientation.
			Arbiter.VertTabs = newVertTabs;
		}
		
		// Toggle the icon hiding option, and loop through
		// all tab labels to hide the icons.
		private void ToggleHideIcons (object sender, EventArgs args)
		{
			// Toggle the option.
			Arbiter.HideIcons = hideIconsMenuItem.Active;
			
			// Loop through each tab and hide the icon.
			foreach (Widget tab in duelNotebook.Children)
			{
				Box box = (Box)duelNotebook.GetTabLabel(tab);
				if (Arbiter.VertTabs)
				{
					box.Children[2].Visible = !Arbiter.HideIcons;
					box.Children[2].NoShowAll = Arbiter.HideIcons;
				}
				else
				{
					box.Children[0].Visible = !Arbiter.HideIcons;
					box.Children[0].NoShowAll = Arbiter.HideIcons;
				}
			}
		}
		
		// Toggle the close button hiding option, and loop through
		// all tab labels to hide the close buttons.
		private void ToggleHideClose (object sender, EventArgs args)
		{
			// Toggle the option.
			Arbiter.HideClose = hideCloseMenuItem.Active;
			
			// Loop through each tab and hide the icon.
			foreach (Widget tab in duelNotebook.Children)
			{
				Box box = (Box)duelNotebook.GetTabLabel(tab);
				if (Arbiter.VertTabs)
				{
					box.Children[0].Visible = !Arbiter.HideClose;
					box.Children[0].NoShowAll = Arbiter.HideIcons;
				}
				else
				{
					box.Children[2].Visible = !Arbiter.HideClose;
					box.Children[2].NoShowAll = Arbiter.HideIcons;
				}
			}
		}
		
		// Toggle the duel button hiding option, and loop through
		// all duel tabs to hide the duel buttons.
		private void ToggleHideButtons (object sender, EventArgs args)
		{
			// Toggle the option.
			Arbiter.HideButtons = hideButtonsMenuItem.Active;
			
			// Loop through each tab and hide the icon.
			for (int i = 0; i < duelNotebook.NPages - 1; i++)
				((Duel)duelNotebook.Children[i]).HideButtons =
					Arbiter.HideButtons;
		}
		
		// Toggle the small scoreboard option, and loop through
		// all duel tabs to reset the scoreboard.
		private void ToggleSmallScore (object sender, EventArgs args)
		{
			// Toggle the option.
			Arbiter.SmallScore = smallScoreMenuItem.Active;
			
			// Loop through each tab and hide the icon.
			for (int i = 0; i < duelNotebook.NPages - 1; i++)
				((Duel)duelNotebook.Children[i]).UpdateLabels();
		}
		#endregion
		
		#region Other Methods
		// This replaces the main widget with a new one.
		// Needed for the brawl tool.
		public static void ReplaceWidget(Widget widget)
		{
			instance.Remove(instance.Child);
			instance.Add(widget);
			instance.WindowPosition = WindowPosition.Center;
		}
		
		// This resets the main widget back to normal.
		public static void ReturnToDuels()
		{
			ReplaceWidget(instance.mainWidget);
			instance.Resize(Arbiter.WindowWidth, Arbiter.WindowHeight);
		}
		#endregion
	}
}

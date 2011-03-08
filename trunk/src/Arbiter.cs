// 
// Arbiter.cs
//  
// Author:
//       Trent McPheron <twilightinzero@gmail.com>
// 
// Copyright (c) 2009-2011 Trent McPheron
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
using System.Collections.Generic;
using System.Reflection;
using Gtk;

namespace Arbiter
{
	// This class stores properties and methods that are used
	// throughout the entire program.
	static class Arbiter
	{	
		#region Fields
		// List of duels.
		private static List<string> fightNightSwords;
		private static List<string> fightNightFists;
		private static List<string> fightNightMagic;
		private static List<string> normalDuelList;
		
		// Saves date when app was started.
		private static DateTime today;
		private static string currentDate;
		
		// Newline.
		private static string n = Environment.NewLine;
		#endregion
		
		#region Properties
		// Sports.
		public static Sport DuelOfSwords  { get; private set; }
		public static Sport DuelOfFists   { get; private set; }
		public static Sport DuelOfMagic   { get; private set; }
		
		// Settings.
		public static int WindowWidth     { get; set; }
		public static int WindowHeight    { get; set; }
		public static string TabPosition  { get; set; }
		public static string LogDirectory { get; set; }
		public static bool HideIcons      { get; set; }
		public static bool HideClose      { get; set; }
		public static bool HideButtons    { get; set; }
		public static bool SmallScore     { get; set; }
		public static bool VertTabs       { get; set; }
		
		// Lists of duelists and rings.
		public static ListStore Duelists  { get; set; }
		public static ListStore Rings     { get; set; }
		
		// Shift report text buffer.
		public static TextBuffer ShiftReport { get; set;}
		
		// Returns the current subdirectory for the night's duels.
		public static string CurrentDir
		{
			get { return Path.Combine(LogDirectory, currentDate); }
		}
		
		// This determines if the duel type will be logged
		// and sorted by sport.
		public static bool FightNight { get; set; }
		
		// This is for logging. Its obvious purpose is to figure out
		// what order the logs were made in. But the other purpose is
		// in case two duelists fight multiple times in a night, e.g.
		// challenges.
		public static int NumDuels { get; set; }
		#endregion
		
		#region Methods
		// This is where the party starts.
		public static void Main (string[] args)
		{
			Application.Init();
			
			// Make the program look less out of place on Windows.
			int p = (int) Environment.OSVersion.Platform;
			if (p != 4 && p != 128)
			{
				Rc.ParseString(@"
					gtk-icon-sizes = 'panel=16,16 : gtk-menu=16,16'
					gtk-button-images = 0
					gtk-menu-images = 0
					style 'default'				{ GtkWidget::focus-line-width = 0 }
					style 'tabs'  = 'default'	{ GtkWidget::focus-padding = 0 }
					style 'lists' = 'default'	{ GtkWidget::focus-line-width = 1 }
					class 'GtkWidget' style 'default'
					class 'GtkNotebook' style 'tabs'
					class 'GtkTreeView' style 'lists'
					");
			}
			
			#region Load Settings
			// Create ListStores for the duelists and rings.
			Duelists = new ListStore(typeof(string));
			Rings = new ListStore(typeof(string));
			Duelists.SetSortColumnId(0, SortType.Ascending);
			
			// Defaults.
			TabPosition = "LeftVert";
			LogDirectory = "";
			WindowWidth = 300;
			WindowHeight = 300;
			HideIcons = false;
			HideClose = false;
			HideButtons = false;
			SmallScore = false;
			
			// Load basic settings file.
			try  // Settings file already exists.
			{
				// Open the file.
				string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
				path = Path.Combine(path, "settings.cfg");
				
				// The using block will close the stream reader
				// if an exception occurs.
				using (StreamReader sr = new StreamReader(path))
				{
					// Load basic settings.
					WindowWidth = Int32.Parse(sr.ReadLine());
					WindowHeight = Int32.Parse(sr.ReadLine());
					TabPosition = String.Copy(sr.ReadLine());
					LogDirectory = String.Copy(sr.ReadLine());
					HideIcons = Boolean.Parse(sr.ReadLine());
					HideClose = Boolean.Parse(sr.ReadLine());
					HideButtons = Boolean.Parse(sr.ReadLine());
					SmallScore = Boolean.Parse(sr.ReadLine());
				}
			}
			catch {}
			
			if (LogDirectory == "")
			{
				// Prompt the user to pick a directory to store duels in.
				FileChooserDialog fc = new FileChooserDialog(
										"Select Log Directory",
										null, FileChooserAction.SelectFolder,
										new object[] {Stock.Open, ResponseType.Accept});
				fc.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
				
				// Keep running the dialog until we get OK.
				int r = 0;
				while (r != (int)ResponseType.Accept) r = fc.Run();
				LogDirectory = fc.Filename;
				fc.Destroy();
			}
			
			// Load duelist list.
			try  // Duelist list already exists.
			{
				// Open the file.
				string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
				path = Path.Combine(path, "duelists.cfg");
				StreamReader sr = new StreamReader(path);
				
				// Read 'em in.
				while (!sr.EndOfStream)
					Duelists.AppendValues(sr.ReadLine());
				
				// Done.
				sr.Close();
			}
			catch  // Duelist list doesn't exist.
			{
				// Load embedded list of duelists.
				StreamReader sr = new StreamReader(
					Assembly.GetExecutingAssembly().
						GetManifestResourceStream("duelists.cfg"));
				while (!sr.EndOfStream)
					Duelists.AppendValues(sr.ReadLine());
				sr.Close();
			}
			
			// Load ring list.
			try  // Ring list already exists.
			{
				// Open the file.
				string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
				path = Path.Combine(path, "rings.cfg");
				StreamReader sr = new StreamReader(path);
				
				// Read 'em in.
				while (!sr.EndOfStream)
					Rings.AppendValues(sr.ReadLine());
				
				// Done.
				sr.Close();
			}
			catch  // Ring list doesn't exist.
			{
				// Load embedded list of rings.
				StreamReader sr = new StreamReader(
					Assembly.GetExecutingAssembly().
						GetManifestResourceStream("rings.cfg"));
				while (!sr.EndOfStream)
					Rings.AppendValues(sr.ReadLine());
				sr.Close();
			}
			#endregion
			
			#region Initialization
			// Set date now so that it doesn't change at midnight.
			today = DateTime.Now;
			// Further midnight-proofing.
			if (today.Hour < 3) today = DateTime.Now.Subtract(TimeSpan.FromDays(1));
			
			// Save the date in string format.
			currentDate = today.Year.ToString("0000") + "-" +
				today.Month.ToString("00") + "-" + today.Day.ToString("00");
			
			// Load the sports.
			DuelOfSwords = new Sport("swords");
			DuelOfFists = new Sport("fists");
			DuelOfMagic = new Sport("magic");
			
			// Initialize Fight Night switch and duel lists.
			FightNight = false;
			fightNightSwords = new List<string>();
			fightNightFists = new List<string>();
			fightNightMagic = new List<string>();
			normalDuelList = new List<string>();
			NumDuels = 0;
			#endregion
			
			// Detect if a shift report already exists, and if so, load it.
			ShiftReport = new TextBuffer(null);
			DetectShiftReport();
			
			// Create the main window and go.
			MainWindow mw = new MainWindow();
			mw.ShowAll();
			Application.Run();
		}
		
		// Save settings to file.
		public static void SaveSettings ()
		{
			// Open the basic settings file.
			string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
			path = Path.Combine(path, "settings.cfg");
			StreamWriter sw = new StreamWriter(path);
			
			// Write the basic settings.
			sw.WriteLine(WindowWidth.ToString());
			sw.WriteLine(WindowHeight.ToString());
			sw.WriteLine(TabPosition);
			sw.WriteLine(LogDirectory);
			sw.WriteLine(HideIcons.ToString());
			sw.WriteLine(HideClose.ToString());
			sw.WriteLine(HideButtons.ToString());
			sw.WriteLine(SmallScore.ToString());
			
			// Close that one.
			sw.Close();
			
			// Now the duelist list.
			path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
			path = Path.Combine(path, "duelists.cfg");
			sw = new StreamWriter(path);
			
			// Loop through the list and write 'em.
			foreach(object[] s in Duelists)
				sw.WriteLine((string)s[0]);
			
			// That one's done now too.
			sw.Close();
			
			// Now the duelist list.
			path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
			path = Path.Combine(path, "rings.cfg");
			sw = new StreamWriter(path);
			
			// Loop through the list and write 'em.
			foreach(object[] s in Rings)
				sw.WriteLine((string)s[0]);
			
			// Now we're all done.
			sw.Close();
		}
		
		// Creates a bare-bones shift report and also makes sure the folder exists.
		public static void CreateShiftReport ()
		{
			// Create the subdirectory if it doesn't exist.
			if (!File.Exists(CurrentDir))
				Directory.CreateDirectory(CurrentDir);
				
			// Open the file.
			StreamWriter sr = new StreamWriter(
					Path.Combine(CurrentDir, "00. Shift Report.txt"), false);
			
			// First lines.
			ShiftReport.Text = today.ToLongDateString() + n + n
						+ "[Insert comments here.]";
			sr.Write(ShiftReport.Text);
			
			// Close it.
			sr.Close();
		}
		
		// Write a duel result to the shift report.
		// This remakes the entire shift report each time so that
		// Fight Night duels can be sorted.
		public static void UpdateShiftReport (string final, bool delete)
		{
			// Open the file.
			StreamWriter sr = new StreamWriter(
					Path.Combine(CurrentDir, "00. Shift Report.txt"), false);
			
			// First lines.
			ShiftReport.Text = today.ToLongDateString() + n + n
						+ "[Insert comments here.]";
			
			// Normal dueling.
			if (!FightNight)
			{
				// Add or delete the final line to the list of duels.
				if (!delete) normalDuelList.Add(final);
				if (delete) normalDuelList.Remove(final);
				
				// Write them to the report.
				ShiftReport.Text += n;
				foreach (string s in normalDuelList)
					ShiftReport.Text += n + s;
			}
			// It's more complicated for Fight Night.
			if (FightNight)
			{
				// Determine duel type from last three characters of
				// final line string.
				if (!delete)
					switch (final.Substring(final.Length - 3, 3))
					{
						case "DoS": fightNightSwords.Add(final); break;
						case "DoF": fightNightFists.Add(final); break;
						case "DoM": fightNightMagic.Add(final); break;
					}
				
				if (delete)
					switch (final.Substring(final.Length - 3, 3))
					{
						case "DoS": fightNightSwords.Remove(final); break;
						case "DoF": fightNightFists.Remove(final); break;
						case "DoM": fightNightMagic.Remove(final); break;
					}
				
				// This prevents duplicating code.
				List<string>[] lists = new List<string>[3]
					{fightNightSwords, fightNightFists, fightNightMagic};
				
				foreach (List<string> l in lists)
				{
					if (l.Count > 0)
					{
						// Line break to separate each list.
						ShiftReport.Text += n;
						foreach (string s in l)
							ShiftReport.Text += n + s;
					}
				}
			}
			
			// Write the text in the buffer to the file.
			sr.Write(ShiftReport.Text);
			
			// Now close it.
			sr.Close();
		}
		
		// Detect if a shift report already exists, and if so, ask what to do.
		// Returns a string because the shift report text buffer doesn't exist
		// at the point in the program at which this runs.
		public static void DetectShiftReport ()
		{
			// Check if the shift report exists.
			if (File.Exists(Path.Combine(CurrentDir, "00. Shift Report.txt")))
			{
				// If it does, ask the user what to do.
				Dialog dialog = new Dialog("Shift report detected", null,
			                           DialogFlags.NoSeparator | DialogFlags.Modal,
			                           new object[] {Stock.No, ResponseType.No,
										Stock.Yes, ResponseType.Yes});
				dialog.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
				Label label = new Label("An existing shift report for today has\n" +
				                        "been detected. Would you like to load it?\n" +
				                        "if not, it will be overwritten when you\n" +
				                        "start a duel.");
				dialog.VBox.PackStart(label);
				dialog.Default = dialog.ActionArea.Children[0];
				dialog.VBox.ShowAll();
				bool load = dialog.Run() == (int)ResponseType.Yes;
				dialog.Destroy();
				
				// The list of duels completed will be loaded.
				if (load)
				{
					StreamReader sr = new StreamReader(
							Path.Combine(CurrentDir, "00. Shift Report.txt"));
					ShiftReport.Text += sr.ReadLine() + n; // The first three
					ShiftReport.Text += sr.ReadLine() + n; // lines don't matter.
					ShiftReport.Text += sr.ReadLine() + n;
					while (!sr.EndOfStream)
					{
						string s = sr.ReadLine();
						ShiftReport.Text += s + n;
						if (s != "") // Can't do anything with blank lines.
						{
							switch (s.Substring(s.Length - 3, 3))
							{
							case "DoS":
								fightNightSwords.Add(s);
								FightNight = true;
								break;
							case "DoF":
								fightNightFists.Add(s);
								FightNight = true;
								break;
							case "DoM":
								fightNightMagic.Add(s);
								FightNight = true;
								break;
							default:
								normalDuelList.Add(s);
								FightNight = false;
								break;
							}
							NumDuels++;
						}
					}
				}
			}
		}
		#endregion
	}
}

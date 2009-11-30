// 
// Main.cs
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
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Reflection;
using Gtk;

namespace Arbiter
{
	// This class stores properties and methods that are used
	// throughout the entire program.
	static class MainClass
	{
		#region Properties
		// Venues.
		public static Venue DuelOfSwords  { get; private set; }
		public static Venue DuelOfFists   { get; private set; }
		public static Venue DuelOfMagic   { get; private set; }
		
		// Settings.
		public static int WindowWidth     { get; set; }
		public static int WindowHeight    { get; set; }
		public static string TabPosition  { get; set; }
		public static string LogDirectory { get; set; }
		
		// Lists of duelists and rings.
		public static ListStore Duelists  { get; set; }
		public static ListStore Rings     { get; set; }
		
		// Returns the current subdirectory for the night's duels.
		public static string CurrentDir
		{
			get { return Path.Combine(LogDirectory, currentDate); }
		}
		
		// This determines if the duel type will be logged
		// and sorted by venue.
		public static bool FightNight { get; set; }
		
		// This is for logging. Its obvious purpose is to figure out
		// what order the logs were made in. But the other purpose is
		// in case two duelists fight multiple times in a night, e.g.
		// challenges.
		public static int NumDuels { get; set; }
		#endregion
		
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
		
		#region Methods
		// This is where the party starts.
		public static void Main (string[] args)
		{
			Application.Init();
			
			#region Load Settings
			// Create ListStores for the duelists and rings.
			Duelists = new ListStore(typeof(string));
			Rings = new ListStore(typeof(string));
			Duelists.SetSortColumnId(0, SortType.Ascending);
			
			// Load settings.
			try  // Settings file already exists.
			{
				// Basic settings.
				//IsolatedStorageFileStream settings = new IsolatedStorageFileStream(
			    //             "rohcallertool.cfg", FileMode.Open, FileAccess.Read);
				string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
				path = Path.Combine(path, "settings.cfg");
				StreamReader sr = new StreamReader(path);
				WindowWidth = Int32.Parse(sr.ReadLine());
				WindowHeight = Int32.Parse(sr.ReadLine());
				TabPosition = sr.ReadLine();
				LogDirectory = sr.ReadLine();
				
				// Rings.
				sr.ReadLine(); // Reads the ----- separator.;
				string s = sr.ReadLine();
				while (s != "-----")
				{
					Rings.AppendValues(s);
					s = sr.ReadLine();
				}
				
				// Duelists.
				s = sr.ReadLine();
				while (s != "-----")
				{
					Duelists.AppendValues(s);
					s = sr.ReadLine();
				}
				
				// Done.
				sr.Close();
				//settings.Close();
			}
			catch  // Settings file doesn't exist; set defaults.
			{
				// Prompt the user to pick a directory to store duels in.
				FileChooserDialog fc = new FileChooserDialog(
											"Select Log Directory",
											null, FileChooserAction.SelectFolder,
											new object[] {Stock.Open, ResponseType.Accept});
				fc.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
				
				// Keep running the dialog until we get OK.
				int r = 0;
				while (r != (int)ResponseType.Accept) r = fc.Run();
				LogDirectory = fc.Filename;
				fc.Destroy();
				
				// Set other defaults.
				TabPosition = "Left";
				WindowWidth = 360;
				WindowHeight = 360;
				
				// Load embedded list of duelists.
				StreamReader sr = new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"Arbiter.duelists.cfg"));
				while (!sr.EndOfStream)
					Duelists.AppendValues(sr.ReadLine());
				sr.Close();
				
				// Load embedded list of rings.
				sr = new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"Arbiter.rings.cfg"));
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
			currentDate = today.Year.ToString("0000") + "-" +
				today.Month.ToString("00") + "-" + today.Day.ToString("00");
			
			// Load the venues.
			DuelOfSwords = new Venue("swords");
			DuelOfFists = new Venue("fists");
			DuelOfMagic = new Venue("magic");
			
			// Initialize Fight Night switch and duel lists.
			FightNight = false;
			fightNightSwords = new List<string>();
			fightNightFists = new List<string>();
			fightNightMagic = new List<string>();
			normalDuelList = new List<string>();
			NumDuels = 0;
			#endregion
			
			// Detect if a shift report already exists, and if so, load it.
			DetectShiftReport();
			
			// Create the main window and go.
			MainWindow MainWin = new MainWindow();
			MainWin.Show();
			Application.Run();
		}
		
		// Save settings to file.
		public static void SaveSettings ()
		{
			string path = Path.GetDirectoryName(
                     Assembly.GetExecutingAssembly().Location);
			path = Path.Combine(path, "settings.cfg");
			StreamWriter sw = new StreamWriter(path);
			sw.WriteLine(WindowWidth.ToString());
			sw.WriteLine(WindowHeight.ToString());
			sw.WriteLine(TabPosition);
			sw.WriteLine(LogDirectory);
			sw.WriteLine("-----");
			foreach(object[] s in Rings)
				sw.WriteLine((string)s[0]);
			sw.WriteLine("-----");
			foreach(object[] s in Duelists)
				sw.WriteLine((string)s[0]);
			sw.WriteLine("-----");
			sw.Close();
			//settings.Close();
		}
		
		// Creates a bare-bones shift report and also makes sure the folder exists.
		public static void CreateShiftReport ()
		{
			// Create the subdirectory if it doesn't exist.
			if (!File.Exists(CurrentDir))
				Directory.CreateDirectory(CurrentDir);
				
			// Open the file.
			StreamWriter shiftReport = new StreamWriter(
					Path.Combine(CurrentDir, "00. Shift Report.txt"), false);
			
			// First lines.
			shiftReport.WriteLine(today.ToLongDateString() + n + n
						+ "[Insert comments here.]");
			
			shiftReport.Close();
		}
		
		// Write a duel result to the shift report.
		// This remakes the entire shift report each time so that
		// Fight Night duels can be sorted.
		public static void UpdateShiftReport (string final, bool delete)
		{
			// Open the file.
			StreamWriter shiftReport = new StreamWriter(
					Path.Combine(CurrentDir, "00. Shift Report.txt"), false);
			
			// First lines.
			shiftReport.WriteLine(today.ToLongDateString() + n + n
						+ "[Insert comments here.]");
			
			// Normal dueling.
			if (!FightNight)
			{
				// Add or delete the final line to the list of duels.
				if (!delete) normalDuelList.Add(final);
				if (delete) normalDuelList.Remove(final);
				
				// Write them to the report.
				shiftReport.WriteLine();
				foreach (string s in normalDuelList)
					shiftReport.WriteLine(s);
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
						shiftReport.WriteLine();
						foreach (string s in l)
							shiftReport.WriteLine(s);
					}
				}
			}
			
			// Now close it.
			shiftReport.Close();
		}
		
		// Detect if a shift report already exists, and if so, ask what to do.
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
				dialog.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
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
					sr.ReadLine(); // The first three
					sr.ReadLine(); // lines don't matter.
					sr.ReadLine();
					while (!sr.EndOfStream)
					{
						string s = sr.ReadLine();
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

// 
// Sport.cs
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Gtk;

namespace Arbiter
{
	// A simple data structure representing a sport.
	public struct Sport
	{
		#region Properties
		public string ShortName   { get; set; }
		public bool Fancies       { get; set; }
		public bool Feints        { get; set; }
		public bool Advantages    { get; set; }
		public string ScoreFormat { get; set; }
		public char[,] Matrix     { get; set; }
		public string[] Abbrev    { get; set; }
		public string[] Moves     { get; set; }
		public ListStore MoveLS   { get; set; }
		#endregion
		
		// Load a sport from an embedded config.
		public Sport (string sportName)
		{
			// Load the embedded config for the sport.
			StreamReader sr = new StreamReader(
					Assembly.GetExecutingAssembly().GetManifestResourceStream(
						"Arbiter.sport-" + sportName + ".cfg"));
			
			// Load the comma separated header.
			string[] header = sr.ReadLine().Split(',');
			ShortName = header[0];
			Fancies = Boolean.Parse(header[1]);
			Feints = Boolean.Parse(header[2]);
			Advantages = Boolean.Parse(header[3]);
			ScoreFormat = header[4];
			int m = Int32.Parse(header[5]); // Number of moves.
			
			// Read the matrix.
			Matrix = new char[m,m];
			for (int a = 0; a < m; a++)
			{
				for (int b = 0; b < m; b++)
					Matrix[a,b] = (char)sr.Read();
				sr.Read();  // Reads the newline character.
			}
			
			// Verify the matrix.
			bool error = false;
			int errA = 0, errB = 0;
			for (int a = 0; a < m; a++)
			for (int b = 0; b < m; b++)
			{
				bool e = false;
				switch (Matrix[a,b])
				{
					case 'A': e = Matrix[b,a] != 'B'; break;
					case 'a': e = Matrix[b,a] != 'b'; break;
					case 'B': e = Matrix[b,a] != 'A'; break;
					case 'b': e = Matrix[b,a] != 'a'; break;
					case '1': e = Matrix[b,a] != '1'; break;
					case '+': e = Matrix[b,a] != '+'; break;
					case '0': e = Matrix[b,a] != '0'; break;
					case '!': e = Matrix[b,a] != '!'; break;
				}
				if (e) errA = a; errB = b;
				error = error || e;
			}
			
			if (error)
			{
				Dialog dialog = new Dialog("Matrix error", null,
			                           DialogFlags.NoSeparator | DialogFlags.Modal,
			                           new object[] {Stock.Ok, ResponseType.Ok});
				dialog.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
				Label dlabel = new Label();
				dlabel.Text = "A consistency error was detected in the " + sportName +
					" matrix\n at "+ errA.ToString() + "," + errB.ToString() + ". " +
					"Please inform the author of this error immediately.";
				dlabel.SetAlignment(0.5f, 0.5f);
				dlabel.Justify = Justification.Center;
				dialog.VBox.PackStart(dlabel);
				dialog.Default = dialog.ActionArea.Children[0];
				dialog.VBox.ShowAll();
				dialog.Run();
				dialog.Destroy();
			}
			
			// Read the abbreviations.
			Abbrev = sr.ReadLine().Split(',');
			
			// Read the move list.
			Moves = new string[m];
			for (int l = 0; l < m; l++)
				Moves[l] = sr.ReadLine();
			
			MoveLS = new ListStore(typeof(string));
			foreach (string s in Moves)
				MoveLS.AppendValues(s);
		}
	}
}

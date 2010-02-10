// 
// Duel.cs
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
using Gtk;
using Glade;

namespace Arbiter
{
	// This widget represents one duel. All the properties of the
	// duel are combined with the widget to tightly integrate it
	// with the duel being called inside it, and to keep each duel
	// separate from the others. It also allows us to create as
	// many duels as we need. OOP at its best.
	public class Duel : Bin
	{
		#region Fields
		private string duelistA, duelistB;
		private string shortNameA, shortNameB;
		private string logHeader;
		private bool overtime, madness;
		private bool usedEFA, usedEFB;
		private float scoreA, scoreB;
		private int duelNum, round;
		private List<int> moveA, moveB;
		private List<float> roundScoreA, roundScoreB;
		private List<bool> advA, advB;
		private List<string> log;
		private static string n = Environment.NewLine;
		private Sport sport;
		#endregion
		
		#region Widgets
		[Widget] private VBox duelWidget;
		[Widget] private Label duelistANameLabel;
		[Widget] private Label duelistBNameLabel;
		[Widget] private Label duelistAScoreLabel;
		[Widget] private Label duelistBScoreLabel;
		[Widget] private Label duelistARoundScoreLabel;
		[Widget] private Label duelistBRoundScoreLabel;
		[Widget] private Label roundLabel;
		[Widget] private CheckButton duelistAFeint;
		[Widget] private CheckButton duelistBFeint;
		[Widget] private HBox duelistAModBox;
		[Widget] private HBox duelistBModBox;
		[Widget] private CheckButton duelistAModCheck;
		[Widget] private CheckButton duelistBModCheck;
		[Widget] private ComboBox duelistAMoveCombo;
		[Widget] private ComboBox duelistBMoveCombo;
		[Widget] private HButtonBox actionBox;
		[Widget] private Button resolveButton;
		[Widget] private Button undoButton;
		[Widget] private TextView duelLogView;
		// End duel window
		[Widget] private Dialog endDuelWin;
		[Widget] private RadioButton duelistAWinRadio;
		[Widget] private RadioButton duelistBWinRadio;
		[Widget] private RadioButton tieRadio;
		[Widget] private Entry duelistAScoreEntry;
		[Widget] private Entry duelistBScoreEntry;
		[Widget] private Entry reasonEntry;
		[Widget] private Button okButton;
		#endregion
		
		#region Properties
		// Convenience properties.
		public TextBuffer DuelLog
			{ get { return duelLogView.Buffer; } }
		public bool DuelistAFancy
			{ get { return duelistAModCheck.Active && sport.Fancies; } }
		public bool DuelistBFancy
			{ get { return duelistBModCheck.Active && sport.Fancies; } }
		public bool DuelistAFeint
			{ get { return duelistAModCheck.Active && sport.Feints; } }
		public bool DuelistBFeint
			{ get { return duelistBModCheck.Active && sport.Feints; } }
		
		// Used to hide the button box.
		public bool HideButtons
		{
			get { return !actionBox.Visible; }
			set
			{
				actionBox.Visible = !value;
				actionBox.NoShowAll = value;
			}
		}
		
		// Used to override automatic ending of the duel,
		// and always enables the resolver. A manual property
		// is used so that this property can enable the
		// resolver.
		private bool manual;
		public bool Manual
		{
			get  { return manual; }
			set
			{
				manual = value;
				resolveButton.Sensitive = CanResolve;
			}
		}
		
		// Used to determine if the duel can be ended.
		public bool End  { get; private set; }
		
		// Determines whether or not the round can be resolved.
		public bool CanResolve {
			get {
				return manual ||
				((duelistAMoveCombo.Active != moveA[round - 1] || duelistAMoveCombo.ActiveText == "Disengage")
				&& (duelistBMoveCombo.Active != moveB[round - 1] || duelistBMoveCombo.ActiveText == "Disengage")
			    && !(moveA[round - 1] == 15 && duelistAMoveCombo.ActiveText == "Reflection")
			    && !(moveB[round - 1] == 15 && duelistBMoveCombo.ActiveText == "Reflection")
			    && !(usedEFA && duelistAMoveCombo.Active == 14)
			    && !(usedEFB && duelistBMoveCombo.Active == 14)
				&& !End); } }
		
		// Determines whether or not the last round can be undoed.
		public bool CanUndo {
			get { return (round > 1); } }
		#endregion
		
		// Constructor.
		public Duel(int duelNum, string ringName, string duelistA, string duelistB,
		            Sport sport, bool overtime, bool madness) : base()
		{
			// Load the Glade file.
			XML xml = new XML("Arbiter.GUI.glade", "duelWidget");
			xml.Autoconnect(this);
			this.Add(duelWidget);
			
			#region Copy Parameters
			this.duelNum = duelNum;
			this.duelistA = duelistA;
			this.duelistB = duelistB;
			this.sport = sport;
			this.overtime = overtime;
			this.madness = madness;
			#endregion
			
			#region Widget Properties	
			// Set widget properties.
			duelistAMoveCombo.Model = sport.Moves;
			duelistBMoveCombo.Model = sport.Moves;
			if (sport.Fancies && sport.Feints)
			{
				duelistAModCheck.Label = "Fancy / Feint";
				duelistBModCheck.Label = "Fancy / Feint";
			}
			else if (sport.Fancies)
			{
				duelistAModCheck.Label = "Fancy";
				duelistBModCheck.Label = "Fancy";
			}
			else if (sport.Feints)
			{
				duelistAModCheck.Label = "Feint";
				duelistBModCheck.Label = "Feint";
			}
			else
			{
				duelistAModBox.Visible = false;
				duelistAModBox.NoShowAll = true;
				duelistBModBox.Visible = false;
				duelistBModBox.NoShowAll = true;
			}
			HideButtons = Arbiter.HideButtons;
			#endregion
			
			#region Short Names
			// Determine short names for each duelist.
			// First, Duelist A.
			if (duelistA.Split('/').Length > 1)
			{
				this.shortNameA = duelistA.Split('/')[1];
				this.duelistA = duelistA.Split('/')[0];
			}
			else
			{
				this.shortNameA = duelistA.Split(' ')[0];
				if (this.shortNameA.Length > 10)
					this.shortNameA = shortNameA.Substring(0, 9);
			}
			// Now, Duelist B.
			if (duelistB.Split('/').Length > 1)
			{
				this.shortNameB = duelistB.Split('/')[1];
				this.duelistB = duelistB.Split('/')[0];
			}
			else
			{
				this.shortNameB = duelistB.Split(' ')[0];
				if (shortNameB.Length > 10)
					this.shortNameB = shortNameB.Substring(0, 9);
			}
			#endregion
			
			#region Initialization
			// Set all the variables.
			duelistANameLabel.Markup = @"<b>" + shortNameA + @"</b>";
			duelistBNameLabel.Markup = @"<b>" + shortNameB + @"</b>";
			round = 1;
			scoreA = 0;
			scoreB = 0;
			usedEFA = false;
			usedEFB = false;
			End = false;
			roundScoreA = new List<float>();
			roundScoreB = new List<float>();
			advA = new List<bool>();
			advB = new List<bool>();
			moveA = new List<int>();
			moveB = new List<int>();
			log = new List<string>();
			
			// Set beginning of duel defaults.
			moveA.Add(-1);
			moveB.Add(-1);
			advA.Add(false);
			advB.Add(false);
			roundScoreA.Add(0);
			roundScoreB.Add(0);
			UpdateLabels(0);
			#endregion
			
			#region Start duel log
			// Create log header.
			logHeader = this.duelistA + " .vs. " +
				this.duelistB + n + "Ring " + ringName;
			if (Arbiter.FightNight) logHeader +=
					" (" + sport.ShortName + ")";
			logHeader += n + n +
				"Rd. | " + shortNameA + " / "
				+ shortNameB + " | Score" + n;
			
			// For automatic scrolling.
			DuelLog.CreateMark("scroll", DuelLog.EndIter, true);
			
			// Write initial text to the log.
			UpdateDuelLog();
			
			// Disable Manual Mode by default.
			manual = false;
			
			// Participate in size negotiation.
			SizeRequested += delegate(object sender, SizeRequestedArgs args) 
				{ args.Requisition = duelWidget.SizeRequest(); };
			SizeAllocated += delegate(object sender, SizeAllocatedArgs args)
				{ duelWidget.Allocation = args.Allocation; };
			#endregion
		}
		
		// Move resolver. Public so that it can be called by
		// the menu bar.
		public void ResolveRound (object sender, EventArgs args)
		{
			#region Start of Resolve
			// Set these now for convenience.
			moveA.Add(duelistAMoveCombo.Active);
			moveB.Add(duelistBMoveCombo.Active);
			
			// Reset the round scores.
			roundScoreA.Add(0);
			roundScoreB.Add(0);
			advA.Add(false);
			advB.Add(false);
			#endregion
			
			#region Resolve
			// Act according to the result indicated on the matrix.
			switch (sport.Matrix[moveA[round], moveB[round]])
			{
			case 'A':  // A scores.
				if (!DuelistAFeint) roundScoreA[round] = 1;
				break;
			case 'B':  // B scores.
				if (!DuelistBFeint) roundScoreB[round] = 1;
				break;
			case 'a':  // A gets advantage.
				if (DuelistBFeint) roundScoreB[round] = 1;
				else if (DuelistAFancy) roundScoreA[round] = 1;
				else if (sport.Advantages)
				{
					if (advA[round - 1])
						roundScoreA[round] = 1;
					else advA[round] = true;
				}
				else roundScoreA[round] = 0.5f;
				break;
			case 'b':  // B gets advantage.
				if (DuelistAFeint) roundScoreA[round] = 1;
				else if (DuelistBFancy) roundScoreB[round] = 1;
				else if (sport.Advantages)
				{
					if (advB[round - 1])
						roundScoreB[round] = 1;
					else advB[round] = true;
				}
				else roundScoreB[round] = 0.5f;
				break;
			case '!':  // Dual RF, Magic only. Yay for C-style switch.
			case '1':  // Both score.
				if (!DuelistAFeint) roundScoreA[round] = 1;
				if (!DuelistBFeint) roundScoreB[round] = 1;
				break;
			case '+':  // Magic only: dual advantage.
				roundScoreA[round] = 0.5f;
				roundScoreB[round] = 0.5f;
				break;
			case '0':  // Null round.
				break;
			}
			
			// Add the scores.
			scoreA += roundScoreA[round];
			scoreB += roundScoreB[round];
			
			// Disable Elemental Fury when used.
			if (moveA[round] == 14) usedEFA = true;
			if (moveB[round] == 14) usedEFB = true;
			#endregion
			
			// Add a line to the duel log.
			UpdateDuelLog(moveA[round], moveB[round]);
			
			// Check for RFx2 and take care of it. It could probably be done
			// without duplicating the above, but I don't care right now. :P
			if (sport.Matrix[moveA[round], moveB[round]] == '!')
			{
				// Add stuff to the lists.
				moveA.Add(15);
				moveB.Add(15);
				roundScoreA.Add(1);
				roundScoreB.Add(1);
				advA.Add(false);
				advB.Add(false);
				
				// Increment round.
				round++;
				
				// Add the special reflected round.
				UpdateDuelLog(15, 15);
			}
			
			// Self-explanatory.
			UpdateLabels(round);
			
			// Increment round.
			round++;
			
			// Also self-explanatory.
			if (!manual) CheckDuelEnd();
			
			#region End of Resolve
			// Uncheck mod boxes.
			duelistAModCheck.Active = false;
			duelistBModCheck.Active = false;
			
			// Disable the resolver until new moves are picked,
			// unless Manual Mode is enabled.
			resolveButton.Sensitive = Manual || false;
			
			// Enable the undoer.
			undoButton.Sensitive = true;
			
			// Tell the main window to check its menu items.
			Arbiter.MainWin.CheckDuelMenu();
			#endregion
		}
		
		// Undoes the round that just happened. Public for
		// the same reason as ResolveRound.
		public void UndoRound (object sender, EventArgs args)
		{
			// Un-end the duel.
			if (End)
			{
				// Remove the final line.
				string final = log[log.Count - 1];
				Arbiter.UpdateShiftReport(final, true);
				log.RemoveAt(log.Count - 1);
				log.RemoveAt(log.Count - 1);
				
				// Re-enable all the widgets.
				duelistAModCheck.Sensitive = true;
				duelistAMoveCombo.Sensitive = true;
				duelistBModCheck.Sensitive = true;
				duelistBMoveCombo.Sensitive = true;
				
				// Set end switch to false.
				End = false;
			}
			
			// Check for RFx2
			if (moveA[round - 1] == 15)
			{
				round--;
				
				// Remove the last round from all the lists.
				moveA.RemoveAt(round);
				moveB.RemoveAt(round);
				roundScoreA.RemoveAt(round);
				roundScoreB.RemoveAt(round);
				advA.RemoveAt(round);
				advB.RemoveAt(round);
				log.RemoveAt(log.Count - 1);
			}
			
			round--;
			
			// Roll back the scores.
			scoreA -= roundScoreA[round];
			scoreB -= roundScoreB[round];
			
			// Detect if EF was used, and re-enable it.
			if (moveA[round] == 14) usedEFA = false;
			if (moveB[round] == 14) usedEFB = false;
			
			// Remove the last round from all the lists.
			moveA.RemoveAt(round);
			moveB.RemoveAt(round);
			roundScoreA.RemoveAt(round);
			roundScoreB.RemoveAt(round);
			advA.RemoveAt(round);
			advB.RemoveAt(round);
			log.RemoveAt(log.Count - 1);
			
			// Set the comboboxes back one round.
			duelistAMoveCombo.Active = moveA[round - 1];
			duelistBMoveCombo.Active = moveB[round - 1];
			
			// Update labels and logs.
			UpdateLabels(round - 1);
			UpdateDuelLog();
			
			// Disable the undoer if it's round 1 now.
			undoButton.Sensitive = CanUndo;
			Arbiter.MainWin.CheckDuelMenu();
		}
		
		// Ends the duel prematurely, presenting a dialog
		// that asks what to do.
		public void EndDuel()
		{
			// Load the GUI.
			XML xml = new XML("Arbiter.GUI.glade", "endDuelWin");
			xml.Autoconnect(this);
			endDuelWin.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
			
			// Set the labels and entries.
			duelistAWinRadio.Label = duelistA;
			duelistBWinRadio.Label = duelistB;
			duelistAScoreEntry.Text = scoreA.ToString(sport.ScoreFormat);
			duelistBScoreEntry.Text = scoreB.ToString(sport.ScoreFormat);
			
			// Pre-select the winner.
			if (scoreA > scoreB) duelistAWinRadio.Active = true;
			else if (scoreB > scoreA) duelistBWinRadio.Active = true;
			else tieRadio.Active = true;
			
			// Create anonymous method to end the duel if OK is clicked.
			okButton.Clicked += delegate { End = true; };
			
			// Run the dialog.
			endDuelWin.Run();
			endDuelWin.Destroy();
			
			// Abort if any button other than OK is pressed.
			if (!End) return;
			
			if (duelistAWinRadio.Active)
			{
				//Add final line to log and shift report.
				log.Add("");
				string final = duelistA + " .def. " + duelistB + ", " +
					duelistAScoreEntry.Text + " - " + duelistBScoreEntry.Text +
					" in " + (round - 1).ToString();
				if (Arbiter.FightNight) final += ", " + sport.ShortName;
				final += " (" + reasonEntry.Text + ")";
				log.Add(final);
				Arbiter.UpdateShiftReport(final, false);
			}
			if (duelistBWinRadio.Active)
			{
				//Add final line to log.
				log.Add("");
				string final = duelistB + " .def. " + duelistA + ", " +
					duelistBScoreEntry.Text + " - " + duelistAScoreEntry.Text +
					" in " + (round - 1).ToString();
				if (Arbiter.FightNight) final += ", " + sport.ShortName;
				final += " (" + reasonEntry.Text + ")";
				log.Add(final);
				Arbiter.UpdateShiftReport(final, false);
			}
			if (tieRadio.Active)
			{
				if ( Single.Parse(duelistAScoreEntry.Text) >=
				    	Single.Parse(duelistBScoreEntry.Text) )
				{
					//Add final line to log.
					log.Add("");
					string final = duelistA + " .ties. " + duelistB + ", " +
						duelistAScoreEntry.Text + " - " + duelistBScoreEntry.Text +
						" in " + (round - 1).ToString();
					if (Arbiter.FightNight) final += ", " + sport.ShortName;
					final += " (" + reasonEntry.Text + ")";
					log.Add(final);
					Arbiter.UpdateShiftReport(final, false);
				}
				else
				{
					//Add final line to log.
					log.Add("");
					string final = duelistB + " .ties. " + duelistA + ", " +
						duelistBScoreEntry.Text + " - " + duelistAScoreEntry.Text +
						" in " + (round - 1).ToString();
					if (Arbiter.FightNight) final += ", " + sport.ShortName;
					final += " (" + reasonEntry.Text + ")";
					log.Add(final);
					Arbiter.UpdateShiftReport(final, false);
				}
			}
			
			// Refresh the duel log.
			UpdateDuelLog();
			
			// Disable all the widgets.
			duelistAModCheck.Sensitive = false;
			duelistAMoveCombo.Sensitive = false;
			duelistBModCheck.Sensitive = false;
			duelistBMoveCombo.Sensitive = false;
		}
		
		#region Methods
		// Updates the labels.
		private void UpdateLabels (int r)
		{
			// Determine scoreboard size.
			string mainSize = "28672";
			string roundSize = "14336";
			if (Arbiter.SmallScore)
			{
				mainSize = "18432";
				roundSize = "9216";
			}
			
			// Create main score parts.
			duelistAScoreLabel.Markup = @"<span size='" + mainSize +
				@"'><b>" + scoreA.ToString(sport.ScoreFormat) + @"</b></span>";
			duelistBScoreLabel.Markup = @"<span size='" + mainSize +
				@"'><b>" + scoreB.ToString(sport.ScoreFormat) + @"</b></span>";
			
			// Create round score parts.
			duelistARoundScoreLabel.Markup = @"<span size='" + roundSize + @"'>" +
				roundScoreA[r].ToString(sport.ScoreFormat) + @"</span>";
			duelistBRoundScoreLabel.Markup = @"<span size='" + roundSize + @"'>" +
				roundScoreB[r].ToString(sport.ScoreFormat) + @"</span>";
			if (advA[r])
				duelistARoundScoreLabel.Markup = @"<span size='" + roundSize + @"'>+</span>";
			if (advB[r])
				duelistBRoundScoreLabel.Markup = @"<span size='" + roundSize + @"'>+</span>";
			
			// Update the round label.
			roundLabel.Markup =
				@"<span size='x-large'><b>Round " + (r + 1).ToString() + @"</b></span>";
		}
		
		// Public overload for the scoreboard size toggler.
		public void UpdateLabels ()
			{ UpdateLabels(round - 1); }
		
		// Updates the duel log.
		private void UpdateDuelLog ()
		{
			DuelLog.Text = logHeader;
			foreach (string s in log)
			{
				DuelLog.Text += n + s;
			}
			
			// Scroll the log view to the bottom.
			DuelLog.MoveMark("scroll", DuelLog.EndIter);
			duelLogView.ScrollMarkOnscreen(DuelLog.GetMark("scroll"));
			
			// Save the current log.
			this.SaveDuel();
		}
		
		// Updates the duel log.
		private void UpdateDuelLog (int mA, int mB)
		{
			// Create score strings.
			string scoreStringA = scoreA.ToString(sport.ScoreFormat);
			string scoreStringB = scoreB.ToString(sport.ScoreFormat);
			
			// Adjust score strings to include advantages.
			if (advA[round]) scoreStringA += "+";
			if (advB[round]) scoreStringB += "+";
			
			// Create special abbreviations in case of fancy or feint.
			string abbrevA, abbrevB;
			if (duelistAModCheck.Active == true)      abbrevA = "F" + sport.Abbrev[mA];
			else abbrevA = " " + sport.Abbrev[mA];  // For consistent spacing.
			if (duelistBModCheck.Active == true)      abbrevB = "F" + sport.Abbrev[mB];
			else abbrevB = sport.Abbrev[mB] + " ";  // For consistent spacing.
			
			// Update log.
			if ((scoreA > scoreB) || advA[round])  // A leads.
				 log.Add(round.ToString("00") + ". | " +
					abbrevA + " / " + abbrevB + " | " +
					scoreStringA + " - " + scoreStringB +
					" " + shortNameA);
			else if ((scoreB > scoreA) || advB[round])  // B leads.
				 log.Add(round.ToString("00") + ". | " +
					abbrevA + " / " + abbrevB + " | " +
					scoreStringB + " - " + scoreStringA +
					" " + shortNameB);
			else  // Tied.
				log.Add(round.ToString("00") + ". | " +
					abbrevA + " / " + abbrevB + " | " +
					scoreStringA + " all");
			
			// Update the log buffer.
			UpdateDuelLog();
		}
		
		// Checks for the end of the duel.
		private void CheckDuelEnd()
		{
			// Create score strings.
			string scoreStringA = scoreA.ToString(sport.ScoreFormat);
			string scoreStringB = scoreB.ToString(sport.ScoreFormat);
			
			// Check for the end of the duel. This is going to get really complicated.
			// It could probably be done better, and any changes are welcome.
			bool duelistAwin = ((scoreA >= 5.0f || (overtime ? false : round > 15)) &&
				scoreA > scoreB && scoreA - scoreB >= 1.0f) ||
				((overtime ? false : round > 15) && madness &&
				 scoreA > scoreB && scoreA - scoreB >= 1.0f);
			bool duelistBwin = ((scoreB >= 5.0f || (overtime ? false : round > 15)) &&
				scoreB > scoreA && scoreB - scoreA >= 1.0f) ||
				((overtime ? false : round > 15) && madness &&
				 scoreB > scoreA && scoreB - scoreA >= 1.0f);
			bool tie = (overtime ? false : round > 15) && !duelistAwin && !duelistBwin;
			End = duelistAwin || duelistBwin || tie;
			
			if (duelistAwin)
			{
				//Add final line to log and shift report.
				log.Add("");
				string final = duelistA + " .def. " + duelistB + ", " +
					scoreStringA + " - " + scoreStringB +
					" in " + (round - 1).ToString();
				if (Arbiter.FightNight) final += ", " + sport.ShortName;
				log.Add(final);
				Arbiter.UpdateShiftReport(final, false);
			}
			if (duelistBwin)
			{
				//Add final line to log.
				log.Add("");
				string final = duelistB + " .def. " + duelistA + ", " +
					scoreStringB + " - " + scoreStringA +
					" in " + (round - 1).ToString();
				if (Arbiter.FightNight) final += ", " + sport.ShortName;
				log.Add(final);
				Arbiter.UpdateShiftReport(final, false);
			}
			if (tie)
			{
				//Add final line to log.
				log.Add("");
				string final = duelistA + " .ties. " + duelistB + ", " +
					scoreStringA + " - " + scoreStringB +
					" in " + (round - 1).ToString();
				if (Arbiter.FightNight) final += ", " + sport.ShortName;
				log.Add(final);
				Arbiter.UpdateShiftReport(final, false);
			}
			if (End)
			{
				// Refresh the duel log.
				UpdateDuelLog();
				
				// Set the round label back.
				roundLabel.Markup =
					@"<span size='x-large'><b>Round " + (round - 1).ToString() + @"</b></span>";
				
				// Disable all the widgets.
				duelistAModCheck.Sensitive = false;
				duelistAMoveCombo.Sensitive = false;
				duelistBModCheck.Sensitive = false;
				duelistBMoveCombo.Sensitive = false;
			}
		}
		
		// Logs the duel after each round.
		private void SaveDuel()
		{
			// Figure out the file name.
			string fileName = duelNum.ToString("00") + ". " + duelistA + " .vs. " + duelistB;
			if (Arbiter.FightNight) fileName += " (" + sport.ShortName + ")";
			fileName += ".txt";
			string path = System.IO.Path.Combine(Arbiter.CurrentDir, fileName);
			
			// Open the file and write the contents of the buffer to it.
			StreamWriter sw = new StreamWriter(path, false);
			sw.Write(DuelLog.Text);
			sw.Close();
		}
		
		// Saves the duel log to a specific file.
		public void SaveDuelAs()
		{
			// Prompt the user to pick a file.
			FileChooserDialog fc = new FileChooserDialog(
									"Save Duel Log As...",
									null, FileChooserAction.Save,
									new object[] {Stock.Cancel, ResponseType.Cancel,
									Stock.Save, ResponseType.Accept});
			fc.Icon = Gdk.Pixbuf.LoadFromResource("Arbiter.RoH.png");
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
			sw.Write(DuelLog.Text);
			sw.Close();
		}
		#endregion
		
		#region Other Widgets
		// Enables the resolver once new moves have been selected.
		private void MoveChanged (object sender, EventArgs args)
		{
			resolveButton.Sensitive = CanResolve;
			Arbiter.MainWin.CheckDuelMenu();
		}
		#endregion
	}
}

// 
// Duel.cs
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
using System.Collections.Generic;
using System.IO;
using Gtk;
using Glade;

namespace Arbiter {

public class Duel : Bin {
    
    ///////////////////
    // Fields
    ///////////////////
    
    private string        duelistA, duelistB;
    private string        shortNameA, shortNameB;
    private string        logHeader;
    private bool          overtime, madness;
    private bool          usedEFA, usedEFB;
    private bool          manual;
    private float         scoreA, scoreB;
    private int           duelNum, round;
    private bool          madnessTie;
    private float         tieScoreA, tieScoreB;
    private int           tieRound;
    private List<int>     moveA, moveB;
    private List<float>   roundScoreA, roundScoreB;
    private List<bool>    advA, advB;
    private List<string>  log;
    private static string n = Environment.NewLine;
    private Sport         sport;
    
    
    ///////////////////
    // Widgets
    ///////////////////
    
    [Widget] private VBox        duelWidget;
    [Widget] private Label       duelistANameLabel;
    [Widget] private Label       duelistBNameLabel;
    [Widget] private Label       duelistAScoreLabel;
    [Widget] private Label       duelistBScoreLabel;
    [Widget] private Label       duelistARoundScoreLabel;
    [Widget] private Label       duelistBRoundScoreLabel;
    [Widget] private Label       roundLabel;
    [Widget] private CheckButton duelistAFancyCheck;
    [Widget] private CheckButton duelistBFancyCheck;
    [Widget] private CheckButton duelistAFeintCheck;
    [Widget] private CheckButton duelistBFeintCheck;
    [Widget] private CheckButton duelistAFocusCheck;
    [Widget] private CheckButton duelistBFocusCheck;
    [Widget] private Label       duelistAFancyPad;
    [Widget] private Label       duelistBFancyPad;
    [Widget] private Label       duelistAFeintPad;
    [Widget] private Label       duelistBFeintPad;
    [Widget] private Label       duelistAFocusPad;
    [Widget] private Label       duelistBFocusPad;
    [Widget] private ComboBox    duelistAMoveCombo;
    [Widget] private ComboBox    duelistBMoveCombo;
    [Widget] private HButtonBox  actionBox;
    [Widget] private Button      resolveButton;
    [Widget] private Button      undoButton;
    [Widget] private TextView    duelLogView;
    
    // End duel window
    [Widget] private Dialog      endDuelWin;
    [Widget] private RadioButton duelistAWinRadio;
    [Widget] private RadioButton duelistBWinRadio;
    [Widget] private RadioButton tieRadio;
    [Widget] private Entry       duelistAScoreEntry;
    [Widget] private Entry       duelistBScoreEntry;
    [Widget] private Entry       reasonEntry;
    [Widget] private Button      okButton;
    
    
    ///////////////////
    // Properties
    ///////////////////
    
    public bool End  { get; private set; }
        
    private int DuelistAMove {
        get {
            return duelistAMoveCombo.Active;
        }
    }
    private int DuelistBMove {
        get {
            return duelistBMoveCombo.Active;
        }
    }
    private bool DuelistAFancy {
        get {
            return duelistAFancyCheck.Active;
        }
    }
    private bool DuelistBFancy {
        get {
            return duelistBFancyCheck.Active;
        }
    }
    private bool DuelistAFeint {
        get {
            return duelistAFeintCheck.Active;
        }
    }
    private bool DuelistBFeint {
        get {
            return duelistBFeintCheck.Active;
        }
    }
    private bool DuelistAFocus {
        get {
            return duelistAFocusCheck.Active;
        }
    }
    private bool DuelistBFocus {
        get {
            return duelistBFocusCheck.Active;
        }
    }
    public bool CanResolve {
        get {
            return manual || (DuelistAValid && DuelistBValid) && !End;
        }
    }
    public bool CanUndo {
        get {
            return (round > 1);
        }
    }
    public bool HideButtons {
        get {
            return !actionBox.Visible;
        } set {
            actionBox.Visible = !value;
            actionBox.NoShowAll = value;
        }
    }
    private bool DuelistAValid {
        get {
            return (round < 2 ? DuelistAMove > -1 : true) &&
             (((DuelistAMove != moveA[round - 1]) ||
             (sport.Moves[DuelistAMove] == "Disengage") ||
             (sport.Moves[DuelistAMove] == "(null)")) &&
             !(usedEFA && DuelistAMove == 14) && !(moveA[round - 1] == 16 &&
             sport.Moves[DuelistAMove] == "Reflection"));
        }
    }
    private bool DuelistBValid {
        get {
            return (round < 2 ? DuelistBMove > -1 : true) &&
             (((DuelistBMove != moveB[round - 1]) ||
             (sport.Moves[DuelistBMove] == "Disengage") ||
             (sport.Moves[DuelistBMove] == "(null)")) &&
             !(usedEFB && DuelistBMove == 14) && !(moveB[round - 1] == 16 &&
             sport.Moves[DuelistBMove] == "Reflection"));
        }
    }
    
    // Makes it easier to manipulate the duel log buffer.
    private string DuelLog {
        get {
            return duelLogView.Buffer.Text;
        } set {
            duelLogView.Buffer.Text = value;
        }
    }
    
    // Used to override automatic ending of the duel, and always enables the
    // resolver. A manual property is used so that this property can enable the
    // resolver.
    public bool Manual {
        get {
            return manual;
        } set {
            manual = value;
            resolveButton.Sensitive = CanResolve;
        }
    }
    
    
    ///////////////////
    // Constructor
    ///////////////////
    
    public Duel(string ringName, string duelistA, string duelistB,
     Sport sport, bool overtime, bool madness) : base() {
        // Load the Glade file.
        XML xml = new XML("Duel.glade", "duelWidget");
        xml.Autoconnect(this);
        this.Add(duelWidget);
        
        this.duelNum = ++Arbiter.NumDuels;
        this.duelistA = duelistA;
        this.duelistB = duelistB;
        this.sport = sport;
        this.overtime = overtime;
        this.madness = madness;
        
        // Determine short names for each duelist.
        if (duelistA.Split('/').Length > 1) {
            this.shortNameA = duelistA.Split('/')[1];
            this.duelistA = duelistA.Split('/')[0];
        } else {
            this.shortNameA = duelistA.Split(' ')[0];
            if (this.shortNameA.Length > 10) {
                this.shortNameA = shortNameA.Substring(0, 9);
            }
        }
        if (duelistB.Split('/').Length > 1) {
            this.shortNameB = duelistB.Split('/')[1];
            this.duelistB = duelistB.Split('/')[0];
        } else {
            this.shortNameB = duelistB.Split(' ')[0];
            if (shortNameB.Length > 10) {
                this.shortNameB = shortNameB.Substring(0, 9);
            }
        }
        
        // Set all the variables.
        duelistANameLabel.Markup = "<b>" + shortNameA + "</b>";
        duelistBNameLabel.Markup = "<b>" + shortNameB + "</b>";
        round = 1;
        scoreA = 0;
        scoreB = 0;
        tieRound = 0;
        tieScoreA = 0;
        tieScoreB = 0;
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
        UpdateLabels();
          
        // Set widget properties.
        ListStore ls = new ListStore(typeof(string));
        foreach (string s in sport.Moves) {
            ls.AppendValues(s);
        }
        duelistAMoveCombo.Model = ls;
        duelistBMoveCombo.Model = ls;
        duelistAFancyCheck.NoShowAll = !(duelistAFancyCheck.Visible =
         sport.Fancies);
        duelistAFancyPad.NoShowAll = !(duelistAFancyPad.Visible =
         sport.Fancies);
        duelistAFeintCheck.NoShowAll = !(duelistAFeintCheck.Visible =
         sport.Feints);
        duelistAFeintPad.NoShowAll = !(duelistAFeintPad.Visible =
         sport.Feints);
        duelistAFocusCheck.NoShowAll = !(duelistAFocusCheck.Visible =
         sport.Focuses);
        duelistAFocusPad.NoShowAll = !(duelistAFocusPad.Visible =
         sport.Focuses);
        duelistBFancyCheck.NoShowAll = !(duelistBFancyCheck.Visible =
         sport.Fancies);
        duelistBFancyPad.NoShowAll = !(duelistBFancyPad.Visible =
         sport.Fancies);
        duelistBFeintCheck.NoShowAll = !(duelistBFeintCheck.Visible =
         sport.Feints);
        duelistBFeintPad.NoShowAll = !(duelistBFeintPad.Visible =
         sport.Feints);
        duelistBFocusCheck.NoShowAll = !(duelistBFocusCheck.Visible =
         sport.Focuses);
        duelistBFocusPad.NoShowAll = !(duelistBFocusPad.Visible =
         sport.Focuses);
        HideButtons = Arbiter.HideButtons;
        
        // Simple anonymous delegates for unchecking the opposite mod box.
        duelistAFancyCheck.Toggled += delegate {
            duelistAFeintCheck.Active = false;
        };
        duelistAFeintCheck.Toggled += delegate {
            duelistAFancyCheck.Active = false;
        };
        duelistBFancyCheck.Toggled += delegate {
            duelistBFeintCheck.Active = false;
        };
        duelistBFeintCheck.Toggled += delegate {
            duelistBFancyCheck.Active = false;
        };
        
        // Delegates to color the comboboxes based on move validity.
        duelistAMoveCombo.AddNotification("active", VerifyMoveA);
        duelistBMoveCombo.AddNotification("active", VerifyMoveB);
        duelistAMoveCombo.AddNotification("popup-shown", VerifyMoveA);
        duelistBMoveCombo.AddNotification("popup-shown", VerifyMoveB);
        
        // Prepare the log.
        logHeader = this.duelistA + " .vs. " + this.duelistB + n + "Ring " +
         ringName;
        if (Arbiter.FightNight) {
            logHeader += " (" + sport.ShortName + ")";
        }
        logHeader += n + n + "Rd. | " + shortNameA + " / " + shortNameB +
         " | Score" + n;
        duelLogView.Buffer.CreateMark("scroll", duelLogView.Buffer.EndIter,
         true);
        UpdateDuelLog();
        
        // Disable Manual Mode by default.
        manual = false;
        
        // Participate in size negotiation.
        SizeRequested += delegate (object sender, SizeRequestedArgs args) {
            args.Requisition = duelWidget.SizeRequest();
        };
        SizeAllocated += delegate (object sender, SizeAllocatedArgs args) {
            duelWidget.Allocation = args.Allocation;
        };
    }
    
    
    ///////////////////
    // Methods
    ///////////////////
    
    // Move resolver. Public so that it can be called by the menu bar.
    public void ResolveRound (object sender, EventArgs args) {
        // Set these now for convenience.
        moveA.Add(duelistAMoveCombo.Active);
        moveB.Add(duelistBMoveCombo.Active);
        
        // Reset the round scores.
        roundScoreA.Add(0);
        roundScoreB.Add(0);
        advA.Add(false);
        advB.Add(false);
        
        // Resolve the moves and mods and store the results.
        float resultA, resultB;
        sport.Resolve(DuelistAMove, DuelistAFancy, DuelistAFeint, DuelistAFocus,
         DuelistBMove, DuelistBFancy, DuelistBFeint, DuelistBFocus,
         out resultA, out resultB);
        roundScoreA[round] = resultA;
        roundScoreB[round] = resultB;
        
        // Check for advantages.
        if (sport.Advantages && roundScoreA[round] == 0.5) {
            roundScoreA[round] = 0; advA[round] = true;
        }
        if (sport.Advantages && roundScoreB[round] == 0.5) {
            roundScoreB[round] = 0; advB[round] = true;
        }
        if (advA[round] && advA[round - 1]) {
            advA[round] = false; roundScoreA[round] = 1;
        }
        if (advB[round] && advB[round - 1]) {
            advB[round] = false; roundScoreB[round] = 1;
        }
        
        // Disable Elemental Fury when used.
        if (moveA[round] == 14) {
            usedEFA = true;
        }
        if (moveB[round] == 14) {
            usedEFB = true;
        }
        
        // Wrap up resolution.
        scoreA += roundScoreA[round];
        scoreB += roundScoreB[round];
        UpdateDuelLog(moveA[round], moveB[round]);
        if (!manual) {
            CheckDuelEnd();
        }
        round++;
        UpdateLabels();
        
        // Uncheck mod boxes.
        duelistAFancyCheck.Active = false;
        duelistBFancyCheck.Active = false;
        duelistAFeintCheck.Active = false;
        duelistBFeintCheck.Active = false;
        duelistAFocusCheck.Active = false;
        duelistBFocusCheck.Active = false;
        
        // Color the comboboxes.
        duelistAMoveCombo.Child.ModifyText(StateType.Normal,
         new Gdk.Color(128, 128, 128));
        duelistAMoveCombo.Child.ModifyText(StateType.Prelight,
         new Gdk.Color(128, 128, 128));
        duelistBMoveCombo.Child.ModifyText(StateType.Normal,
         new Gdk.Color(128, 128, 128));
        duelistBMoveCombo.Child.ModifyText(StateType.Prelight,
         new Gdk.Color(128, 128, 128));
        
        // Disable the resolver until new moves are picked, unless Manual Mode
        // is enabled.
        resolveButton.Sensitive = Manual || false;
        
        // Enable the undoer.
        undoButton.Sensitive = true;
        
        // Tell the main window to check its menu items.
        MainWindow.SCheckDuelMenu();
    }
    
    // Undoes the round that just happened. Public for the same reason as
    // ResolveRound.
    public void UndoRound (object sender, EventArgs args) {
        // Un-end the duel.
        if (End) {
            // Remove the final line.
            string final = log[log.Count - 1];
            Arbiter.UpdateShiftReport(final, true);
            log.RemoveAt(log.Count - 1);
            log.RemoveAt(log.Count - 1);
            
            // Re-enable all the widgets.
            duelistAFancyCheck.Sensitive = true;
            duelistAFeintCheck.Sensitive = true;
            duelistAFocusCheck.Sensitive = true;
            duelistAMoveCombo.Sensitive = true;
            duelistBFancyCheck.Sensitive = true;
            duelistBFeintCheck.Sensitive = true;
            duelistBFocusCheck.Sensitive = true;
            duelistBMoveCombo.Sensitive = true;
            
            // Set end switch to false.
            End = false;
        }
        
        round--;
        
        // Roll back the scores.
        scoreA -= roundScoreA[round];
        scoreB -= roundScoreB[round];
        
        // Detect if EF was used, and re-enable it.
        if (moveA[round] == 14) {
            usedEFA = false;
        }
        if (moveB[round] == 14) {
            usedEFB = false;
        }
        
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
        
        // Color the comboboxes.
        duelistAMoveCombo.Child.ModifyText(StateType.Normal,
         new Gdk.Color(128, 128, 128));
        duelistAMoveCombo.Child.ModifyText(StateType.Prelight,
         new Gdk.Color(128, 128, 128));
        duelistBMoveCombo.Child.ModifyText(StateType.Normal,
         new Gdk.Color(128, 128, 128));
        duelistBMoveCombo.Child.ModifyText(StateType.Prelight,
         new Gdk.Color(128, 128, 128));
        
        // Update labels and logs.
        UpdateLabels();
        UpdateDuelLog();
        
        // Disable the undoer if it's round 1 now.
        undoButton.Sensitive = CanUndo;
        MainWindow.SCheckDuelMenu();
    }
    
    // Ends the duel prematurely, presenting a dialog that asks what to do.
    public void EndDuel () {
        // Load the GUI.
        XML xml = new XML("Duel.glade", "endDuelWin");
        xml.Autoconnect(this);
        endDuelWin.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
        
        // Set the labels and entries.
        duelistAWinRadio.Label = duelistA;
        duelistBWinRadio.Label = duelistB;
        duelistAScoreEntry.Text = scoreA.ToString(sport.ScoreFormat);
        duelistBScoreEntry.Text = scoreB.ToString(sport.ScoreFormat);
        
        // Pre-select the winner.
        if (scoreA > scoreB) {
            duelistAWinRadio.Active = true;
        } else if (scoreB > scoreA) {
            duelistBWinRadio.Active = true;
        } else {
            tieRadio.Active = true;
        }
        
        // Create anonymous method to end the duel if OK is clicked.
        okButton.Clicked += delegate {
            End = true;
        };
        
        // Abort if any button other than OK is pressed.
        endDuelWin.Run();
        if (!End) {
            endDuelWin.Destroy();
            return;
        }
        
        // End the duel depending on selections and log it.
        if (duelistAWinRadio.Active) {
            log.Add("");
            string final = duelistA + " .def. " + duelistB + ", " +
             duelistAScoreEntry.Text + " - " + duelistBScoreEntry.Text +
             " in " + (round - 1).ToString();
            if (Arbiter.FightNight) {
                final += ", " + sport.ShortName;
            }
            final += " (" + reasonEntry.Text + ")";
            log.Add(final);
            Arbiter.UpdateShiftReport(final, false);
        } else if (duelistBWinRadio.Active) {
            log.Add("");
            string final = duelistB + " .def. " + duelistA + ", " +
             duelistBScoreEntry.Text + " - " + duelistAScoreEntry.Text +
             " in " + (round - 1).ToString();
            if (Arbiter.FightNight) {
                final += ", " + sport.ShortName;
            }
            final += " (" + reasonEntry.Text + ")";
            log.Add(final);
            Arbiter.UpdateShiftReport(final, false);
        } else if (tieRadio.Active) {
            if (Single.Parse(duelistAScoreEntry.Text) >=
             Single.Parse(duelistBScoreEntry.Text)) {
                log.Add("");
                string final = duelistA + " .ties. " + duelistB + ", " +
                 duelistAScoreEntry.Text + " - " + duelistBScoreEntry.Text +
                 " in " + (round - 1).ToString();
                if (Arbiter.FightNight) {
                    final += ", " + sport.ShortName;
                }
                final += " (" + reasonEntry.Text + ")";
                log.Add(final);
                Arbiter.UpdateShiftReport(final, false);
            } else {
                log.Add("");
                string final = duelistB + " .ties. " + duelistA + ", " +
                 duelistBScoreEntry.Text + " - " + duelistAScoreEntry.Text +
                 " in " + (round - 1).ToString();
                if (Arbiter.FightNight) {
                    final += ", " + sport.ShortName;
                }
                final += " (" + reasonEntry.Text + ")";
                log.Add(final);
                Arbiter.UpdateShiftReport(final, false);
            }
        }
        
        // Close the dialog.
        endDuelWin.Destroy();
        
        // Refresh the duel log.
        UpdateDuelLog();
        
        // Disable all the widgets.
        duelistAFancyCheck.Sensitive = false;
        duelistAFeintCheck.Sensitive = false;
        duelistAFocusCheck.Sensitive = false;
        duelistAMoveCombo.Sensitive = false;
        duelistBFancyCheck.Sensitive = false;
        duelistBFeintCheck.Sensitive = false;
        duelistBFocusCheck.Sensitive = false;
        duelistBMoveCombo.Sensitive = false;
    }
    
    // Updates the labels.
    public void UpdateLabels () {
        // Determine scoreboard size.
        string mainSize = "28672";
        string roundSize = "14336";
        if (Arbiter.SmallScore) {
            mainSize = "22528";
            roundSize = "11264";
        }
        
        // Create main score parts.
        duelistAScoreLabel.Markup = "<span size='" + mainSize +
         "'><b>" + scoreA.ToString(sport.ScoreFormat) + "</b></span>";
        duelistBScoreLabel.Markup = "<span size='" + mainSize +
         "'><b>" + scoreB.ToString(sport.ScoreFormat) + "</b></span>";
        
        // Create round score parts.
        duelistARoundScoreLabel.Markup = "<span size='" + roundSize + "'>" +
         roundScoreA[round - 1].ToString(sport.ScoreFormat) + "</span>";
        duelistBRoundScoreLabel.Markup = "<span size='" + roundSize + "'>" +
         roundScoreB[round - 1].ToString(sport.ScoreFormat) + "</span>";
        if (advA[round - 1]) {
            duelistARoundScoreLabel.Markup = "<span size='" + roundSize +
             "'>+</span>";
        }
        if (advB[round - 1]) {
            duelistBRoundScoreLabel.Markup = "<span size='" + roundSize +
             "'>+</span>";
        }
        
        // Update the round label.
        roundLabel.Markup = "<span size='x-large'><b>Round " + (End ? round - 1
         : round).ToString() + "</b></span>";
    }
    
    // Refreshes the log buffer, scrolls it down, and saves it.
    private void UpdateDuelLog () {
        DuelLog = logHeader;
        foreach (string s in log) {
            DuelLog += n + s;
        }
        duelLogView.Buffer.MoveMark("scroll", duelLogView.Buffer.EndIter);
        duelLogView.ScrollMarkOnscreen(duelLogView.Buffer.GetMark("scroll"));
        this.SaveDuel();
    }
    
    // Updates the duel log with moves.
    private void UpdateDuelLog (int mA, int mB)
    {
        // Create score strings.
        string scoreStringA = scoreA.ToString(sport.ScoreFormat);
        string scoreStringB = scoreB.ToString(sport.ScoreFormat);
        
        // Adjust score strings to include advantages.
        if (advA[round]) {
            scoreStringA += "+";
        }
        if (advB[round]) {
            scoreStringB += "+";
        }
        
        // Create special abbreviations in case of fancy, feint, or focus.
        string abbrevA, abbrevB;
        if (DuelistAFancy && sport.ShortName == "DoF") {
            abbrevA = "Fa" + sport.Abbrev[mA];
        } else if (DuelistAFancy || DuelistAFocus) {
            abbrevA = " F" + sport.Abbrev[mA];
        } else if (DuelistAFeint) {
            abbrevA = "Fe" + sport.Abbrev[mA];
        } else {
            abbrevA = "  " + sport.Abbrev[mA];  // For consistent spacing.
        }
        if (DuelistBFancy && sport.ShortName == "DoF") {
            abbrevB = "Fa" + sport.Abbrev[mB];
        } else if (DuelistBFancy || DuelistBFocus) {
            abbrevB = "F" + sport.Abbrev[mB] + " ";
        } else if (DuelistBFeint) {
            abbrevB = "Fe" + sport.Abbrev[mB];
        } else {
            abbrevB = sport.Abbrev[mB] + "  ";  // For consistent spacing.
        }
        
        // Update log.
        if ((scoreA > scoreB) || advA[round]) {  // A leads.
             log.Add(round.ToString("00") + ". | " + abbrevA + " / " + abbrevB +
              " | " + scoreStringA + " - " + scoreStringB + " " + shortNameA);
        } else if ((scoreB > scoreA) || advB[round]) {  // B leads.
             log.Add(round.ToString("00") + ". | " + abbrevA + " / " + abbrevB +
              " | " + scoreStringB + " - " + scoreStringA + " " + shortNameB);
        } else  // Tied.
            log.Add(round.ToString("00") + ". | " + abbrevA + " / " + abbrevB +
             " | " + scoreStringA + " all");
        
        // Update the log buffer.
        UpdateDuelLog();
    }
    
    // Checks for the end of the duel.
    private void CheckDuelEnd () {
        bool duelistAwin = ((scoreA >= 5.0f || (overtime ? false : round > 14))
         && scoreA > scoreB && scoreA - scoreB >= 1.0f) || ((overtime ? false :
         round > 14) && madness && scoreA > scoreB && scoreA - scoreB >= 1.0f);
        bool duelistBwin = ((scoreB >= 5.0f || (overtime ? false : round > 14))
         && scoreB > scoreA && scoreB - scoreA >= 1.0f) || ((overtime ? false :
         round > 14) && madness && scoreB > scoreA && scoreB - scoreA >= 1.0f);
        bool tie = (overtime ? false : round > 14) && !duelistAwin &&
         !duelistBwin;
        
        End = duelistAwin || duelistBwin || tie;
        
        // If we've already determined that we're over r15 in madness, we don't
        // want to mess with anything.
        if (!madnessTie) {
            madnessTie = madness && (round > 14) && !End;
            
            // Save the scores so we can make the modified final line.
            if (madnessTie) {
                tieScoreA = scoreA;
                tieScoreB = scoreB;
                tieRound = round;
            }
        }
        
        // If the duel's not over, we're done here.
        if (!End) return;
        
        // Create score strings.
        string scoreStringA = scoreA.ToString(sport.ScoreFormat);
        string scoreStringB = scoreB.ToString(sport.ScoreFormat);
        string tieScoreStringA = tieScoreA.ToString(sport.ScoreFormat);
        string tieScoreStringB = tieScoreB.ToString(sport.ScoreFormat);
        
        // See who wins and update the shift report.
        if (duelistAwin) {
            log.Add("");
            string final;
            if (madnessTie) {
                final = duelistA + " .ties. " + duelistB + ", " +
                 tieScoreStringA + " - " + tieScoreStringB + " in " +
                 tieRound.ToString();
            } else {
                final = duelistA + " .def. " + duelistB + ", " + scoreStringA +
                 " - " + scoreStringB + " in " + round.ToString();
            }
            if (Arbiter.FightNight) {
                final += ", " + sport.ShortName;
            }
            if (madnessTie) {
                final += " (Madness: " + shortNameA + " .def. " + shortNameB +
                 ", " + scoreStringA + " - " + scoreStringB + " in " +
                 round.ToString() + ")";
            } else if (madness) {
                final += " (Madness)";
            }
            log.Add(final);
            Arbiter.UpdateShiftReport(final, false);
        } else if (duelistBwin) {
            log.Add("");
            string final;
            if (madnessTie) {
                final = duelistB + " .ties. " + duelistA + ", " +
                 tieScoreStringB + " - " + tieScoreStringA + " in " +
                 tieRound.ToString();
            } else {
                final = duelistB + " .def. " + duelistA + ", " + scoreStringB +
                 " - " + scoreStringA + " in " + round.ToString();
            }
            if (Arbiter.FightNight) {
                final += ", " + sport.ShortName;
            }
            if (madnessTie) {
                final += " (Madness: " + shortNameB + " .def. " + shortNameA +
                 ", " + scoreStringB + " - " + scoreStringA + " in " +
                 round.ToString() + ")";
            } else if (madness) {
                final += " (Madness)";
            }
            log.Add(final);
            Arbiter.UpdateShiftReport(final, false);
        } else if (tie) {
            log.Add("");
            string final;
            if (scoreA >= scoreB) {
                final = duelistA + " .ties. " + duelistB + ", " + scoreStringA +
                 " - " + scoreStringB + " in " + round.ToString();
            } else {
                final = duelistB + " .ties. " + duelistA + ", " + scoreStringB +
                 " - " + scoreStringA + " in " + round.ToString();
            }
            if (Arbiter.FightNight) {
                final += ", " + sport.ShortName;
            }
            log.Add(final);
            Arbiter.UpdateShiftReport(final, false);
        }
        
        // Refresh the duel log.
        UpdateDuelLog();
        
        // Disable all the widgets.
        duelistAFancyCheck.Sensitive = false;
        duelistAFeintCheck.Sensitive = false;
        duelistAFocusCheck.Sensitive = false;
        duelistAMoveCombo.Sensitive = false;
        duelistBFancyCheck.Sensitive = false;
        duelistBFeintCheck.Sensitive = false;
        duelistBFocusCheck.Sensitive = false;
        duelistBMoveCombo.Sensitive = false;
    }
    
    // Logs the duel after each round.
    private void SaveDuel () {
        string fileName = duelNum.ToString("00") + ". " + duelistA + " .vs. " +
         duelistB;
        if (Arbiter.FightNight) {
            fileName += " (" + sport.ShortName + ")";
        }
        fileName += ".txt";
        string path = System.IO.Path.Combine(Arbiter.CurrentDir, fileName);
        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(DuelLog);
        sw.Close();
    }
    
    // Saves the duel log to a specific file.
    public void SaveDuelAs () {
        FileChooserDialog fc = new FileChooserDialog("Save Duel Log As...",
         null, FileChooserAction.Save, new object[] {Stock.Cancel,
         ResponseType.Cancel, Stock.Save, ResponseType.Accept});
        fc.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
        fc.Modal = true;
        int r = fc.Run();
        if (r != (int)ResponseType.Accept) {
            fc.Destroy();
            return;
        }
        string path = fc.Filename;
        fc.Destroy();
        StreamWriter sw = new StreamWriter(path, false);
        sw.Write(DuelLog);
        sw.Close();
    }
    
    // Colors the combobox text depending on move validity.
    private void VerifyMoveA (object sender, GLib.NotifyArgs args)
    {
        duelistAMoveCombo.Child.ModifyText(StateType.Normal, (DuelistAValid ?
         new Gdk.Color(0, 128, 0) : new Gdk.Color(192, 0, 0)));
        duelistAMoveCombo.Child.ModifyText(StateType.Prelight, (DuelistAValid ?
         new Gdk.Color(0, 128, 0) : new Gdk.Color(192, 0, 0)));
        
        // Possibly enable resolver.
        resolveButton.Sensitive = CanResolve;
        MainWindow.SCheckDuelMenu();
    }
    private void VerifyMoveB (object sender, GLib.NotifyArgs args)
    {
        duelistBMoveCombo.Child.ModifyText(StateType.Normal, (DuelistBValid ?
         new Gdk.Color(0, 128, 0) : new Gdk.Color(192, 0, 0)));
        duelistBMoveCombo.Child.ModifyText(StateType.Prelight, (DuelistBValid ?
         new Gdk.Color(0, 128, 0) : new Gdk.Color(192, 0, 0)));
        
        resolveButton.Sensitive = CanResolve;
        MainWindow.SCheckDuelMenu();
    }
    
}

}

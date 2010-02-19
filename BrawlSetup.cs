// 
// BrawlSetup.cs
//  
// Author:
//       Trent McPheron <twilightinzero@gmail.com>
// 
// Copyright (c) 2010 Trent McPheron
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
using Gtk;
using Glade;

namespace Arbiter
{
	public class BrawlSetup : Bin
	{
		#region Widgets
		[Widget] private HBox brawlSetupWidget;
		[Widget] private TextView combatantsView;
		[Widget] private ComboBox sportCombo;
		[Widget] private SpinButton hpSpin;
		[Widget] private SpinButton mpSpin;
		[Widget] private CheckButton staticDefenseCheck;
		[Widget] private CheckButton fullFancyCheck;
		#endregion
		
		// Constructor.
		public BrawlSetup () : base()
		{
			// Load the widgets.
			XML xml = new XML("Arbiter.GUI.glade", "brawlSetupWidget");
			xml.Autoconnect(this);
			this.Add(brawlSetupWidget);
			
			// Set default option in the sport combo.
			sportCombo.Active = 0;
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = brawlSetupWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ brawlSetupWidget.Allocation = args.Allocation; };
		}
		
		// Makes various widgets insensitive if DoM is selected.
		private void CheckSport (object sender, EventArgs args)
			{ mpSpin.Sensitive = fullFancyCheck.Sensitive = sportCombo.ActiveText != "Magic"; }
		
		// Starts the brawl.
		private void StartBrawl (object sender, EventArgs args)
		{
			// Split the combatants by newline and convert them into a list.
			string[] combatants = combatantsView.Buffer.Text.Split(
					new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			List<string> order = new List<string>(combatants);
			
			// Determine sport.
			Sport sport;
			if (sportCombo.ActiveText == "Swords") sport = Arbiter.DuelOfSwords;
			else if (sportCombo.ActiveText == "Fists") sport = Arbiter.DuelOfFists;
			else if (sportCombo.ActiveText == "Magic") sport = Arbiter.DuelOfMagic;
			else return;  // Someone messed up.
			
			// Start the actual brawl.
			Brawl brawl = new Brawl(order, sport,
			                        (float)hpSpin.Value,
			                        (short)mpSpin.ValueAsInt,
			                        fullFancyCheck.Active,
			                        staticDefenseCheck.Active);
			
			// Replace the main window widget with it.
			Arbiter.MainWin.ReplaceWidget(brawl);
			brawl.ShowAll();
		}
		
		// Shuffles the list of combatants.
		private void ShuffleCombatants (object sender, EventArgs args)
		{
			// Split the combatants by newline and convert them into a list.
			string[] combatants = combatantsView.Buffer.Text.Split(
					new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			List<string> oldOrder = new List<string>(combatants);
			
			// Clear the list.
			combatantsView.Buffer.Clear();
			
			// Use a random number generator.
			Random random = new Random();
			int r = 0;
			
			// Add random candidates to the list and remove
			// them from the old list until it's empty.
			while (oldOrder.Count > 0)
			{
				r = random.Next(0, oldOrder.Count);
				combatantsView.Buffer.Text += oldOrder[r]
					+ (oldOrder.Count > 1 ? Environment.NewLine : "");
				oldOrder.RemoveAt(r);
     		}
		}
	}
}

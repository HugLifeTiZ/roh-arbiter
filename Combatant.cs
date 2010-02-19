// 
// Combatant.cs
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
using Gtk;
using Glade;

namespace Arbiter
{
	public class Combatant : Bin
	{
		#region Widgets
		[Widget] private HBox combatantWidget;
		[Widget] private Label nameLabel;
		[Widget] private Entry hpEntry;
		[Widget] private Entry mpEntry;
		[Widget] private CheckButton priFancyCheck;
		[Widget] private CheckButton priFeintCheck;
		[Widget] private CheckButton secFancyCheck;
		[Widget] private CheckButton secFeintCheck;
		[Widget] private CheckButton sdCheck;
		[Widget] private CheckButton eliminateCheck;
		[Widget] private ComboBox primaryCombo;
		[Widget] private ComboBox targetCombo;
		[Widget] private ComboBox secondaryCombo;
		#endregion
		
		#region Fields
		private Sport sport;
		private float hp;
		private short mp;
		private int lastPrimary;
		private int lastSecondary;
		private bool primaryValid;
		private bool secondaryValid;
		private bool targetValid;
		#endregion
		
		#region Properties
		// Automatically handles HP entry updating.
		public float HP
		{
			get { return hp; }
			set
			{
				hp = value;
				hpEntry.Text = hp.ToString(sport.ScoreFormat);
			}
		}
		
		// Automatically handles MP entry updating.
		public short MP
		{
			get { return mp; }
			set
			{
				mp = value;
				mpEntry.Text = mp.ToString();
			}
		}
		
		// Checks to see if the combatant's choices are valid.
		public bool Valid
		{ 
			get
				{ return primaryValid && secondaryValid && targetValid; }
			set
				{ primaryValid = secondaryValid = targetValid = value; }
		}
		
		// The combatant's name.
		public string CName { get; private set; }
		
		// These translate the selected items in the
		// comboboxes into publically accessible ints.
		public int Primary
			{ get { return primaryCombo.Active; } }
		public int Target
			{ get { return targetCombo.Active; } }
		public int Secondary
			{ get { return secondaryCombo.Active; } }
		
		// These translate the active states of the
		// checkboxes into publically accessible bools.
		public bool PriFancy
			{ get { return priFancyCheck.Active; } }
		public bool PriFeint
			{ get { return priFeintCheck.Active; } }
		public bool SecFancy
			{ get { return priFancyCheck.Active; } }
		public bool SecFeint
			{ get { return priFeintCheck.Active; } }
		public bool SD
			{ get { return sdCheck.Active; } }
		public bool Eliminate
			{ get { return eliminateCheck.Active; } }
		
		// Determines if the combatant has acted yet
		// during resolution.
		public bool Acted { get; set; }
		
		// Determines if the combatant has defended
		// yet during resolution.
		public bool Defended { get; set; }
		
		// Determines if the special full fancy defense
		// rule will take place.
		public bool FullFancy { get; set; }
		#endregion
		
		// Constructor.
		public Combatant (string name, float hp, short mp, Sport sport, bool sd) : base()
		{
			// Load the widgets.
			XML xml = new XML("Arbiter.GUI.glade", "combatantWidget");
			xml.Autoconnect(this);
			this.Add(combatantWidget);
			
			// Assign sport.
			this.sport = sport;
			
			// Set combatant's name.
			CName = name;
			nameLabel.Markup = "<b>" + name + "</b>";
			
			// Assign intial hp and mp, and set entries.
			HP = hp;
			MP = mp;
			
			// Allow manual editing of HP and MP.
			hpEntry.FocusOutEvent += delegate { HP = Single.Parse(hpEntry.Text); };
			mpEntry.FocusOutEvent += delegate { MP = Int16.Parse(mpEntry.Text); };
			hpEntry.Activated += delegate { HP = Single.Parse(hpEntry.Text); };
			mpEntry.Activated += delegate { MP = Int16.Parse(mpEntry.Text); };
			
			// Determine checkbutton visiblity.
			priFancyCheck.NoShowAll = !(priFancyCheck.Visible = sport.Fancies);
			priFeintCheck.NoShowAll = !(priFeintCheck.Visible = sport.Feints);
			secFancyCheck.NoShowAll = !(secFancyCheck.Visible = sport.Fancies);
			secFeintCheck.NoShowAll = !(secFeintCheck.Visible = sport.Feints);
			sdCheck.NoShowAll = !(sdCheck.Visible = sd);
			
			// Assign combobox lists.
			primaryCombo.Model = sport.MoveLS;
			secondaryCombo.Model = sport.MoveLS;
			targetCombo.Model = Brawl.Order;
			
			// Set last move selections and valid state.
			lastPrimary = -1;
			lastSecondary = -1;
			Valid = false;
			
			// Handle the checkboxes.
			priFancyCheck.Toggled += delegate(object sender, EventArgs args)
				{ priFeintCheck.Active = false; VerifyMod(sender, args); };
			priFeintCheck.Toggled += delegate(object sender, EventArgs args)
				{ priFancyCheck.Active = false; VerifyMod(sender, args); };
			secFancyCheck.Toggled += delegate(object sender, EventArgs args)
				{ secFeintCheck.Active = false; VerifyMod(sender, args); };
			secFeintCheck.Toggled += delegate(object sender, EventArgs args)
				{ secFancyCheck.Active = false; VerifyMod(sender, args); };
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = combatantWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ combatantWidget.Allocation = args.Allocation; };
		}
		
		// Verifies primary move selection.
		public void VerifyPrimary (object sender, EventArgs args)
		{
			if ((primaryCombo.ActiveText == "Disengage") ||
				(Primary != lastPrimary && Primary != lastSecondary))
			{
				primaryValid = true;
				primaryCombo.ModifyText(StateType.Normal, new Gdk.Color(0, 128, 0));
				primaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(0, 128, 0));
			}
			else
			{
				primaryValid = false;
				primaryCombo.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
				primaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			}
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Verifies secondary move selection.
		public void VerifySecondary (object sender, EventArgs args)
		{
			if ((secondaryCombo.ActiveText == "Disengage") ||
				(Secondary != lastSecondary))
			{
				secondaryValid = true;
				secondaryCombo.ModifyText(StateType.Normal, new Gdk.Color(0, 128, 0));
				secondaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(0, 128, 0));
			}
			else
			{
				secondaryValid = false;
				secondaryCombo.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
				secondaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			}
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Verifies target selection. Really, the only point
		// of this one is to make sure a target is selected.
		public void VerifyTarget (object sender, EventArgs args)
		{
			targetValid = true;
			targetCombo.ModifyText(StateType.Normal, new Gdk.Color(0, 128, 0));
			targetCombo.ModifyText(StateType.Prelight, new Gdk.Color(0, 128, 0));
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Ensures a mod is not activated when the combatant
		// has insufficient MP.
		public void VerifyMod (object sender, EventArgs args)
		{
			bool modValid = (Convert.ToByte(PriFancy) +
			            Convert.ToByte(PriFeint) +
			            Convert.ToByte(SecFancy) +
			            Convert.ToByte(SecFeint) <= MP);
			
			// Uncheck the box that was just checked.
			if (!modValid) ((CheckButton)sender).Active = false;
		}
		
		// Resets all the duelist's attributes for the next round.
		public void Reset ()
		{
			// Uncheck all the checkboxes.
			priFancyCheck.Active = false;
			priFeintCheck.Active = false;
			secFancyCheck.Active = false;
			secFeintCheck.Active = false;
			sdCheck.Active = false;
			eliminateCheck.Active = false;
			
			// Change the attributes of the combo boxes.
			primaryCombo.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			primaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			secondaryCombo.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			secondaryCombo.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			targetCombo.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			targetCombo.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			
			// Store the last moves.
			lastPrimary = Primary;
			lastSecondary = Secondary;
			
			// Reset the other variables.
			Acted = false;
			Defended = false;
			FullFancy = false;
			Valid = false;
		}
	}
}

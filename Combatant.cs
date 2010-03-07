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
		#region Fields
		private Sport sport;
		private float hp;
		private short mp;
		private int lastPrimary;
		private int lastSecondary;
		private bool primaryChosen;
		private bool secondaryChosen;
		private bool targetChosen;
		private bool finalTwo;
		#endregion
		
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
		[Widget] private Label targetLabel;
		[Widget] private Label secondaryLabel;
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
		
		// Checks to see if the primary move is valid.
		private bool PrimaryValid
		{
			get
			{
				return (primaryCombo.ActiveText == "Disengage") ||
					(Primary != lastPrimary && Primary != lastSecondary &&
			    	 ((Primary != Secondary) || (secondaryCombo.ActiveText == "Disengage")));
			}
		}
		
		// Checks to see if the secondary move is valid.
		private bool SecondaryValid
		{
			get
			{
				return (secondaryCombo.ActiveText == "Disengage") ||
					(Secondary != lastSecondary && 
			    	 ((Secondary != Primary) || (primaryCombo.ActiveText == "Disengage")));
			}
		}
		
		// Checks to see if the combatant's choices are valid.
		public bool Valid
		{ 
			get
				{ return primaryChosen && ((secondaryChosen && targetChosen) || FinalTwo); }
			set
				{ primaryChosen = secondaryChosen = targetChosen = value; }
		}
		
		// The combatant's name.
		public string CName { get; private set; }
		
		// These translate the selected items in the
		// comboboxes into publically accessible ints.
		public int Primary
			{ get { return primaryCombo.Active; } }
		public int Secondary
			{ get { return secondaryCombo.Active; } }
		public int Target
		{
			set { targetCombo.Active = value; }
			get { return targetCombo.Active; }
		}
		
		// These translate the active states of the
		// checkboxes into publically accessible bools.
		public bool PriFancy
			{ get { return priFancyCheck.Active; } }
		public bool PriFeint
			{ get { return priFeintCheck.Active; } }
		public bool SecFancy
			{ get { return secFancyCheck.Active; } }
		public bool SecFeint
			{ get { return secFeintCheck.Active; } }
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
		
		// This will be activated when there are only
		// two combatants remaining in the brawl.
		// It desensitizes the target and secondary
		// move widgets.
		public bool FinalTwo
		{
			get { return finalTwo; }
			set
			{
				finalTwo = value;
				lastPrimary = -1;
				lastSecondary = -1;
				secondaryCombo.Active = -1;
				targetLabel.Sensitive = !value;
				targetCombo.Sensitive = !value;
				sdCheck.Sensitive = !value;
				secondaryLabel.Sensitive = !value;
				secondaryCombo.Sensitive = !value;
				secFancyCheck.Sensitive = !value;
				secFeintCheck.Sensitive = !value;
			}
		}
		#endregion
		
		// Constructor.
		public Combatant (string name, float hp, short mp, Sport sport, bool sd) : base()
		{
			// Load the widgets.
			XML xml = new XML("Combatant.glade", "combatantWidget");
			xml.Autoconnect(this);
			this.Add(combatantWidget);
			
			// Save sport.
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
			ListStore ls = new ListStore(typeof(string));
			foreach (string s in sport.Moves)
				ls.AppendValues(s);
			primaryCombo.Model = ls;
			secondaryCombo.Model = ls;
			targetCombo.Model = Brawl.Order;
			
			// Set last move selections and bools.
			lastPrimary = -1;
			lastSecondary = -1;
			primaryChosen = false;
			secondaryChosen = false;
			targetChosen = false;
			finalTwo = false;
			
			// Checkbutton exvent handlers.
			priFancyCheck.Toggled += delegate(object sender, EventArgs args)
				{ priFeintCheck.Active = false; VerifyMod(priFancyCheck); };
			priFeintCheck.Toggled += delegate(object sender, EventArgs args)
				{ priFancyCheck.Active = false; VerifyMod(priFeintCheck); };
			secFancyCheck.Toggled += delegate(object sender, EventArgs args)
				{ secFeintCheck.Active = false; VerifyMod(secFancyCheck); };
			secFeintCheck.Toggled += delegate(object sender, EventArgs args)
				{ secFancyCheck.Active = false; VerifyMod(secFeintCheck); };
			
			// Combobox event handlers. Both active and popup-shown are used
			// because the active event is only emitted when the chosen item
			// is different from the last, and the popup-shown event is
			// emitted before the active item actually changes.
			primaryCombo.AddNotification("popup-shown", VerifyPrimary);
			secondaryCombo.AddNotification("popup-shown", VerifySecondary);
			targetCombo.AddNotification("popup-shown", VerifyTarget);
			primaryCombo.AddNotification("active", VerifyPrimary);
			secondaryCombo.AddNotification("active", VerifySecondary);
			targetCombo.AddNotification("active", VerifyTarget);
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = combatantWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ combatantWidget.Allocation = args.Allocation; };
		}
		
		// Verifies move selection.
		public void VerifyPrimary (object sender, GLib.NotifyArgs args)
		{
			// Since there's no undoer, it's safer to just ensure
			// that a move is chosen...
			primaryChosen = true;
			
			// But color the combobox text depending on validity.
			primaryCombo.Child.ModifyText(StateType.Normal, 
			                              (PrimaryValid ?
			                               new Gdk.Color(0, 128, 0) :
			                               new Gdk.Color(192, 0, 0)));
			primaryCombo.Child.ModifyText(StateType.Prelight, 
			                              (PrimaryValid ?
			                               new Gdk.Color(0, 128, 0) :
			                               new Gdk.Color(192, 0, 0)));
			
			// Do the same for the other box if it's been chosen yet.
			if (secondaryChosen)
			{
				secondaryCombo.Child.ModifyText(StateType.Normal, 
				                                (SecondaryValid ?
				                                 new Gdk.Color(0, 128, 0) :
				                                 new Gdk.Color(192, 0, 0)));
				secondaryCombo.Child.ModifyText(StateType.Prelight, 
				                                (SecondaryValid ?
				                                 new Gdk.Color(0, 128, 0) :
				                                 new Gdk.Color(192, 0, 0)));
			}
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Verifies secondary move selection.
		public void VerifySecondary (object sender, GLib.NotifyArgs args)
		{
			// Since there's no undoer, it's safer to just
			// ensure that a move is chosen...
			secondaryChosen = true;
			
			// But color the combobox text depending on validity.
			secondaryCombo.Child.ModifyText(StateType.Normal, 
			                                (SecondaryValid ?
			                                 new Gdk.Color(0, 128, 0) :
			                                 new Gdk.Color(192, 0, 0)));
			secondaryCombo.Child.ModifyText(StateType.Prelight, 
			                                (SecondaryValid ?
			                                 new Gdk.Color(0, 128, 0) :
			                                 new Gdk.Color(192, 0, 0)));
			
			// Do the same for the other box if it's been chosen yet.
			if (primaryChosen)
			{
				primaryCombo.Child.ModifyText(StateType.Normal, 
				                              (PrimaryValid ?
				                               new Gdk.Color(0, 128, 0) :
				                               new Gdk.Color(192, 0, 0)));
				primaryCombo.Child.ModifyText(StateType.Prelight, 
				                              (PrimaryValid ?
				                               new Gdk.Color(0, 128, 0) :
				                               new Gdk.Color(192, 0, 0)));
			}
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Verifies target selection. Really, the only point
		// of this one is to inform the main brawler that a
		// target has been selected.
		public void VerifyTarget (object sender, GLib.NotifyArgs args)
		{
			// Yup, target selected.
			targetChosen = true;
			
			// Inform the caller if the combatant is targeting self.
			targetCombo.Child.ModifyText(StateType.Normal, 
			                             (targetCombo.ActiveText != CName ?
			                              new Gdk.Color(0, 128, 0) :
			                              new Gdk.Color(192, 0, 0)));
			targetCombo.Child.ModifyText(StateType.Prelight, 
			                             (targetCombo.ActiveText != CName ?
			                              new Gdk.Color(0, 128, 0) :
			                              new Gdk.Color(192, 0, 0)));
			
			// Check to see if the resolver can be enabled.
			Brawl.CheckResolve();
		}
		
		// Ensures a mod is not activated when the combatant
		// has insufficient MP.
		public void VerifyMod (CheckButton check)
		{
			bool modValid = (Convert.ToInt16(PriFancy) +
			                 Convert.ToInt16(PriFeint) +
			                 Convert.ToInt16(SecFancy) +
			                 Convert.ToInt16(SecFeint) <= MP);
			
			// Uncheck the box that was just checked.
			if (!modValid) check.Active = false;
		}
		
		// Automatically makes the combatant's moves valid when
		// manual elimination is requested.
		public void VerifyElimination (object sender, EventArgs args)
		{
			if (Eliminate) Valid = true;
			Brawl.CheckResolve();
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
			primaryCombo.Child.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			primaryCombo.Child.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			secondaryCombo.Child.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			secondaryCombo.Child.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			targetCombo.Child.ModifyText(StateType.Normal, new Gdk.Color(128, 128, 128));
			targetCombo.Child.ModifyText(StateType.Prelight, new Gdk.Color(128, 128, 128));
			
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

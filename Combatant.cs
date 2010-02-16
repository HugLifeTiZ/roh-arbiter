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
		[Widget] private CheckButton fancyCheck;
		[Widget] private CheckButton feintCheck;
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
		public bool Fancy
			{ get { return fancyCheck.Active; } }
		public bool Feint
			{ get { return feintCheck.Active; } }
		public bool SD
			{ get { return sdCheck.Active; } }
		public bool Eliminate
			{ get { return eliminateCheck.Active; } }
		
		// The combatant's name.
		public string CombatantName  { get; private set; }
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
			CombatantName = name;
			nameLabel.Markup = "<b>" + name + "</b>";
			
			// Assign intial hp and mp, and set labels.
			HP = hp;
			MP = mp;
			
			// Allow manual editing of HP and MP.
			hpEntry.FocusOutEvent += delegate { HP = Single.Parse(hpEntry.Text); };
			mpEntry.FocusOutEvent += delegate { MP = Int16.Parse(mpEntry.Text); };
			hpEntry.Activated += delegate { HP = Single.Parse(hpEntry.Text); };
			mpEntry.Activated += delegate { MP = Int16.Parse(mpEntry.Text); };
			
			// Determine checkbutton visiblity.
			fancyCheck.NoShowAll = !(fancyCheck.Visible = sport.Fancies);
			feintCheck.NoShowAll = !(feintCheck.Visible = sport.Feints);
			sdCheck.NoShowAll = !(sdCheck.Visible = sd);
			
			// Assign combobox lists.
			primaryCombo.Model = sport.Moves;
			secondaryCombo.Model = sport.Moves;
			targetCombo.Model = Brawl.Order;
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = combatantWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ combatantWidget.Allocation = args.Allocation; };
		}
	}
}

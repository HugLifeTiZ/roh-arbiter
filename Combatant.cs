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
		[Widget] private Frame combatantWidget;
		[Widget] private Label combatantLabel;
		[Widget] private Label hpLabel;
		[Widget] private Label mpLabel;
		[Widget] private CheckButton modCheck;
		[Widget] private CheckButton sdCheck;
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
		// Automatically handles HP label updating.
		public float HP
		{
			get { return hp; }
			set
			{
				hp = value;
				hpLabel.Markup =
					"<span size='small'>HP</span> <span size='xx-large' weight='bold'>"
					+ hp.ToString(sport.ScoreFormat) + "</span>";
			}
		}
		
		// Automatically handles MP label updating.
		public short MP
		{
			get { return mp; }
			set
			{
				mp = value;
				mpLabel.Markup =
					"<span size='small'>HP</span> <span size='xx-large' weight='bold'>"
					+ mp.ToString() + "</span>";
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
		
		// These translated the active states of the
		// two checkboxes into publically accessible bools.
		public bool Mod
			{ get { return modCheck.Active; } }
		public bool SD
			{ get { return sdCheck.Active; } }
		
		// The combatant's name.
		public string Name  { get; private set; }
		#endregion
		
		// The constructor.
		public Combatant (string name, float hp, short mp, Sport sport, bool sd) : base()
		{
			// Load the widgets.
			XML xml = new XML("Arbiter.GUI.glade", "combatantWidget");
			xml.Autoconnect(this);
			this.Add(combatantWidget);
			
			// Assign sport.
			this.sport = sport;
			
			// Set combatant's name.
			Name = name;
			combatantLabel.Markup =
				"<span size='xx-large' weight='bold'>" + name + "</span>";
			
			// Assign intial hp and mp, and set labels.
			HP = hp;
			MP = mp;
			
			// Determine checkbutton visiblity.
			modCheck.Visible = !modCheck.NoShowAll = (sport.Fancies || sport.Feints);
			sdCheck.Visible = !sdCheck.NoShowAll = !sd;
			
			// Participate in size negotiation.
			SizeRequested += delegate(object sender, SizeRequestedArgs args) 
				{ args.Requisition = combatantWidget.SizeRequest(); };
			SizeAllocated += delegate(object sender, SizeAllocatedArgs args)
				{ combatantWidget.Allocation = args.Allocation; };
		}
	}
}

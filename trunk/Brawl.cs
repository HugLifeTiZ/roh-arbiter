// 
// Brawl.cs
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
	public class Brawl : Bin
	{
		#region Widgets
		// The main widget's still considered an HBox even though
		// I changed its orientation to Vertical in glade. :/
		[Widget] private HBox brawlWidget;
		[Widget] private VBox combatantBox;
		[Widget] private Label roundLabel;
		[Widget] private Label orderLabel;
		[Widget] private ScrolledWindow summaryScroll;
		[Widget] private TextView summaryView;
		[Widget] private Button resolveButton;
		#endregion
		
		#region Fields
		private List<Combatant> order;
		private int round;
		private Sport sport;
		#endregion
		
		public static ListStore Order { get; private set; }
		
		public Brawl (List<string> order, Sport sport, float hp,
		              short mp, bool fullFancy, bool sd) : base()
		{
			// Create widgets.
			XML xml = new XML("Arbiter.GUI.glade", "brawlWidget");
			xml.Autoconnect(this);
			this.Add(brawlWidget);
			
			// Create list of combatants.
			this.order = new List<Combatant>();
			Order = new ListStore(typeof(string));
			
			// Clear the order label.
			orderLabel.Text = "";
			
			// Create combatants using the list of strings we received.
			for (int i = 0; i < order.Count; i++)
			{
				Order.AppendValues(order[i]);
				Combatant c = new Combatant(order[i], hp, mp, sport, sd);
				this.order.Add(c);
				combatantBox.PackStart(c, false, false, 0u);
				orderLabel.Text += order[i];
				if (i < order.Count - 1) orderLabel.Text += ", ";
			}
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = brawlWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ brawlWidget.Allocation = args.Allocation; };
		}
	}
}

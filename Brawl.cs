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
using System.IO;
using Gtk;
using Glade;

namespace Arbiter
{
	public class Brawl : Bin
	{
		#region Fields
		private int duelNum;
		private Sport sport;
		private List<Combatant> order;
		private bool fullFancy;
		private bool gainMod;
		private string n = Environment.NewLine;
		private static Brawl instance;
		#endregion
		
		#region Widgets
		[Widget] private HBox brawlWidget;
		[Widget] private VBox combatantBox;
		[Widget] private SpinButton roundSpin;
		[Widget] private Label orderLabel;
		[Widget] private ScrolledWindow summaryScroll;
		[Widget] private TextView summaryView;
		[Widget] private Button resolveButton;
		#endregion
		
		// The liststore that each combatant will use for targeting.
		public static ListStore Order { get; private set; }
		
		// Convenience properties.
		private string Summary
		{
			get { return summaryView.Buffer.Text; }
			set { summaryView.Buffer.Text = value; }
		}
		private int Round
		{
			get { return (int)roundSpin.Value; }
			set { roundSpin.Value = value; }
		}
		
		public Brawl (List<string> order, Sport sport, float hp,
		              short mp, bool sd, bool fullFancy, bool gainMod) : base()
		{
			// Save a few parameters.
			this.duelNum = ++Arbiter.NumDuels;
			this.fullFancy = fullFancy;
			this.gainMod = gainMod;
			this.sport = sport;
			
			// Create widgets.
			XML xml = new XML("Brawl.glade", "brawlWidget");
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
				Summary += order[i];
				if (i < order.Count - 1)
				{
					Summary += " .vs. ";
					orderLabel.Text += ", ";
				}
			}
			
			// More preliminary summary stuff.
			if (sport.ShortName == "DoS") Summary += n + "Duel of Swords";
			if (sport.ShortName == "DoF") Summary += n + "Duel of Fists";
			if (sport.ShortName == "DoM") Summary += n + "Duel of Magic";
			Summary += n + "Initial HP: " + hp.ToString(sport.ScoreFormat);
			if (sport.Fancies || sport.Feints)
				Summary += n + "Initial Mods: " + mp.ToString();
			if (sd) Summary += n +"Static Defenses enabled";
			if (fullFancy) Summary += n + "Full Fancy Defenses enabled";
			if (gainMod) Summary += n + "Knockout Mod Bonus enabled";
			
			// For automatic scrolling.
			summaryView.Buffer.CreateMark("scroll", summaryView.Buffer.EndIter, true);
			
			// Set round number.
			Round = 1;
			
			// Store present instance.
			instance = this;
			
			// Participate in size negotiation.
			SizeRequested += delegate (object sender, SizeRequestedArgs args) 
				{ args.Requisition = brawlWidget.SizeRequest(); };
			SizeAllocated += delegate (object sender, SizeAllocatedArgs args)
				{ brawlWidget.Allocation = args.Allocation; };
		}
		
		// Enables the resolve button if all the combatants' choices are valid.
		public static void CheckResolve ()
		{
			bool resolve = true;
			foreach (Combatant c in instance.order)
				resolve = resolve && c.Valid;
			instance.resolveButton.Sensitive = resolve;
		}
		
		// Cancel the brawl in progress.
		private void CancelBrawl (object sender, EventArgs args)
		{
			// Just in case of mis-clicks...
			Dialog dialog = new Dialog("Cancel brawl?", null,
			                        DialogFlags.NoSeparator | DialogFlags.Modal,
			                        new object[] {Stock.No, ResponseType.No,
									Stock.Yes, ResponseType.Yes});
			dialog.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			Label label = new Label("If you cancel this brawl, all current progress\n" +
			                        "in the brawl will be lost. Are you sure you\n" +
			                        "want to cancel?");
			dialog.VBox.PackStart(label);
			dialog.Default = dialog.ActionArea.Children[0];
			dialog.VBox.ShowAll();
			bool cancel = dialog.Run() == (int)ResponseType.Yes;
			dialog.Destroy();
			
			// Exit the method if the user said anything other than yes.
			if (!cancel) return;
			
			// Otherwise, nullify the saved instance and
			// reset to the main widget.
			instance = null;
			MainWindow.ReturnToDuels();
		}
		
		// Resolves the current round.
		private void ResolveRound (object sender, EventArgs args)
		{
			// Line breaks for padding.
			Summary += n + n;
			
			// Print the round header into the summary.
			Summary += "  ROUND " + Round.ToString() + n;
			Summary += "===========================" + n;
			
			float resultA = 0;
			float resultB = 0;
			
			// First, check for full fancy defenders.
			if (fullFancy)
			{
				// Loop through all combatants.
				for (int c = 0; c < order.Count; c++)
				{
					// Don't consider the combatant if he's not fancying.
					if (order[c].SecFancy)
					{
						// Track the number of people that will be
						// defended.
						int defended = 0;
						
						// Now loop through everyone some more.
						for (int t = 0; t < order.Count; t++)
						{
							if (order[t].Target == c &&
							    order[c].Target != t && !order[t].SD)
							{
								sport.Resolve(order[c].Secondary, order[c].SecFancy, order[c].SecFeint,
								              order[t].Primary, order[t].PriFancy, order[t].PriFeint,
								              out resultA, out resultB);
								if (resultA == 1) defended++;
							}
						}
						
						order[c].FullFancy = defended > 1;
					}
				}
			}
			
			// Then, check for anyone exiting the ring,
			// and also anyone trying to SD themselves.
			foreach (Combatant c in order)
			{
				if (c.Eliminate || c.HP <= 0)
				{
					// Set HP to zero, mark as having acted and defended
					c.HP = 0;
					c.Acted = true;
					c.Defended = true;
					
					// Print what happened.
					Summary += c.CName + " exits the ring." + n;
				}
				else if (order[c.Target].CName == c.CName && c.SD)
				{
					// Mark them as having acted and defended.
					// They don't get to do squat.
					c.Acted = true;
					c.Defended = true;
					
					// Make fun of them.
					Summary += c.CName + " stands there with a finger " +
						"lodged in a nostril. Too bad." + n;
				}
			}
			
			// Loop through each combatant.
			for (int c = 0; c < order.Count; c++)
			{
				// Reset result variables.
				resultA = 0;
				resultB = 0;
				
				// Decrement MP.
				order[c].MP -= Convert.ToInt16(order[c].PriFancy);
				order[c].MP -= Convert.ToInt16(order[c].PriFeint);
				order[c].MP -= Convert.ToInt16(order[c].SecFancy);
				order[c].MP -= Convert.ToInt16(order[c].SecFeint);
				
				// Store target in a variable.
				int t = order[c].Target;
				
				// If the combatant has not yet acted and is not SDing...
				if (!order[c].Acted && !order[c].SD)
				{
					// If the target is KO'd, first of all...
					if (order[t].HP <= 0)
					{
						// Nothing happens, pretty much.
						Summary += order[c].CName + " attacks " + order[t].CName + " with " +
							(order[c].PriFancy ? "Fancy " : "") +
							(order[c].PriFeint ? "Feint " : "") +
							sport.Moves[order[c].Primary] + ", which is wasted on " +
							"the KO'd combatant." + n;
					}
					// The combatant is attacking him/herself...
					// ...I'm going to enjoy this far more than I should.
					else if (order[c].Target == c)
					{
						// Loop through each candidate in attacks and see if
						// the combatant is using any of them.
						string attacks = "TH,HC,LC,SL,JA,CH,UC,HO,SN,JK,SP,FL," +
						                 "MB,MW,WB,FT,FF,MS,AB,RF,NR,IM,EF";
						foreach (string a in attacks.Split(','))
							if (sport.Abbrev[order[c].Primary] == a &&
							    !order[c].PriFeint)
								resultA = 1;
						
							// Inflict damange.
						order[c].HP -= resultA;
						
						// Print what just happened.
						Summary += order[c].CName + " attacks self with " +
							(order[c].PriFancy ? "Fancy " : "") +
							(order[c].PriFeint ? "Feint " : "") +
							sport.Moves[order[c].Primary] + ". That wasn't very smart.  ( " +
							resultA.ToString(sport.ScoreFormat) + " )" + n;
					}
					// If the combatant and their target are targeting each other...
					else if (order[t].Target == c && !order[t].Acted && !order[t].SD)
					{
						// Evaluate primary vs. primary.
						sport.Resolve(order[c].Primary, order[c].PriFancy, order[c].PriFeint,
						              order[t].Primary, order[t].PriFancy, order[t].PriFeint,
						              out resultA, out resultB);
						
						// Inflict damage.
						order[c].HP -= resultB;
						order[t].HP -= resultA;
						
						// Print what just happened.
						Summary += order[c].CName + " attacks " + order[t].CName + " with " +
							(order[c].PriFancy ? "Fancy " : "") +
							(order[c].PriFeint ? "Feint " : "") +
							sport.Moves[order[c].Primary] + ", who counterattacks with " +
							(order[t].PriFancy ? "Fancy " : "") +
							(order[t].PriFeint ? "Feint " : "") +
							sport.Moves[order[t].Primary] + ".  ( " +
							resultA.ToString(sport.ScoreFormat) + " / " +
							resultB.ToString(sport.ScoreFormat) + " )" + n;
						
						// Mark the target as having acted.
						order[t].Acted = true;
						
						// Check for eliminations and mod bonuses.
						if (order[t].HP <= 0)
						{
							order[t].HP = 0;
							order[t].Defended = true;
							if (gainMod) order[c].MP++;
							Summary += order[c].CName + " eliminates " +
								order[t].CName + (gainMod ?
								                  " and gains one mod!" :
								                  "!") + n;
						}
						if (order[c].HP <= 0)
						{
							order[c].HP = 0;
							order[c].Defended = true;
							if (gainMod) order[t].MP++;
							Summary += order[t].CName + " eliminates " +
								order[c].CName + (gainMod ?
								                  " and gains one mod!" :
								                  "!") + n;
						}
					}
					else
					{
						// Check for anyone SDing the target.
						int sder = -1;
						int d = 0;
						while (sder == -1 && d < order.Count)
						{
							// Stop at the first combatant SDing the target.
							if (order[d].SD && !order[d].Acted &&
							    order[d].Target == order[c].Target)
								sder = d;
							d++;
						}
						if (sder > -1)
						{
							// Just looks better this way.
							d = sder;
							
							// Evaluate primary vs. primary.
							sport.Resolve(order[c].Primary, order[c].PriFancy, order[c].PriFeint,
							              order[d].Primary, order[d].PriFancy, order[d].PriFeint,
							              out resultA, out resultB);
							
							// Inflict damage.
							order[c].HP -= resultB;
							order[d].HP -= resultA;
							
							// Print what just happened.
							Summary += order[c].CName + " attacks " + order[t].CName + " with " +
								(order[c].PriFancy ? "Fancy " : "") +
								(order[c].PriFeint ? "Feint " : "") +
								sport.Moves[order[c].Primary] + ", who is protected by " +
								order[d].CName + ", who uses " +
								(order[d].PriFancy ? "Fancy " : "") +
								(order[d].PriFeint ? "Feint " : "") +
								sport.Moves[order[d].Primary] + ".  ( " +
								resultA.ToString(sport.ScoreFormat) + " / " +
								resultB.ToString(sport.ScoreFormat) + " )" + n;
							
							// Mark the defender as having acted.
							order[d].Acted = true;
							
							// Check for eliminations and mod bonuses.
							if (order[d].HP <= 0)
							{
								order[d].HP = 0;
								order[d].Defended = true;
								if (gainMod) order[c].MP++;
								Summary += order[c].CName + " eliminates " +
									order[d].CName + (gainMod ?
									                  " and gains one mod!" :
									                  "!") + n;
							}
							if (order[c].HP <= 0)
							{
								order[c].HP = 0;
								order[c].Defended = true;
								if (gainMod) order[d].MP++;
								Summary += order[d].CName + " eliminates " +
									order[c].CName + (gainMod ?
									                  " and gains one mod!" :
									                  "!") + n;
							}
						}
						else if (!order[t].Defended)
						{
							// Evaluate primary vs. secondary.
							sport.Resolve(order[c].Primary, order[c].PriFancy, order[c].PriFeint,
							              order[t].Secondary, order[t].SecFancy, order[t].SecFeint,
							              out resultA, out resultB);
							
							// Nullify any damage inflicted by a full fancy defender.
							if (order[t].FullFancy) resultB = 0;
							
							// Inflict damage.
							order[c].HP -= resultB;
							order[t].HP -= resultA;
							
							// Print what just happened.
							Summary += order[c].CName + " attacks " + order[t].CName + " with " +
								(order[c].PriFancy ? "Fancy " : "") +
								(order[c].PriFeint ? "Feint " : "") +
								sport.Moves[order[c].Primary] + ", who defends with " +
								(order[t].FullFancy ? "Full " : "") +
								(order[t].SecFancy ? "Fancy " : "") +
								(order[t].SecFeint ? "Feint " : "") +
								sport.Moves[order[t].Secondary] + ".  ( " +
								resultA.ToString(sport.ScoreFormat) + " / " +
								resultB.ToString(sport.ScoreFormat) + " )" + n;
							
							// Mark the defender as having defended if
							// a full fancy defense wasn't used.
							order[t].Defended = true && !order[t].FullFancy;
							
							// Check for eliminations and mod bonuses.
							if (order[t].HP <= 0)
							{
								order[t].HP = 0;
								order[t].Acted = true;
								order[t].Defended = true;
								if (gainMod) order[c].MP++;
								Summary += order[c].CName + " eliminates " +
									order[t].CName + (gainMod ?
									                  " and gains one mod!" :
									                  "!") + n;
							}
							if (order[c].HP <= 0)
							{
								order[c].HP = 0;
								order[c].Defended = true;
								if (gainMod) order[t].MP++;
								Summary += order[t].CName + " eliminates " +
									order[c].CName + (gainMod ?
									                  " and gains one mod!" :
									                  "!") + n;
							}
						}
						// The defender has already defended.
						else
						{
							// Loop through each candidate in attacks and see if
							// the combatant is using any of them.
							string attacks = "TH,HC,LC,SL,JA,CH,UC,HO,SN,JK,SP,FL," +
							                 "MB,MW,WB,FT,FF,MS,AB,RF,NR,IM,EF";
							foreach (string a in attacks.Split(','))
								if (sport.Abbrev[order[c].Primary] == a &&
								    !order[c].PriFeint)
									resultA = 1;
							
							// Inflict damange.
							order[t].HP -= resultA;
							
							// Print what just happened.
							Summary += order[c].CName + " attacks " + order[t].CName + " with " +
								(order[c].PriFancy ? "Fancy " : "") +
								(order[c].PriFeint ? "Feint " : "") +
								sport.Moves[order[c].Primary] + ", which goes unhindered.  ( " +
								resultA.ToString(sport.ScoreFormat) + " / " +
								resultB.ToString(sport.ScoreFormat) + " )" + n;
							
							// Check for eliminations and mod bonuses.
							if (gainMod && order[t].HP <= 0)
							{
								order[t].HP = 0;
								order[t].Acted = true;
								if (gainMod) order[c].MP++;
								Summary += order[c].CName + " eliminates " +
									order[t].CName + (gainMod ?
									                  " and gains one mod!" :
									                  "!") + n;
							}
						}
					}
					
					// Mark the current combatant as having acted.
					order[c].Acted = true;
				}
			}
			
			// Check for any SDers that didn't get to defend.
			foreach (Combatant c in order)
				if (!c.Acted && c.SD)
					Summary += c.CName + " was poised to protect " +
						order[c.Target].CName + " with " +
						sport.Moves[c.Primary] +
						", but no attack came that way." + n;
			
			// Print sub-header.
			Summary += "------------------------" + n;
			Summary += "REMAINING HP" +
				((sport.Fancies || sport.Feints) ? " / MODS" : "") + n;
			
			// Print out each HP value.
			for (int i = 0; i < order.Count; i++)
			{
				Summary += order[i].CName + ": " +
					(order[i].HP > 0 ?
					 order[i].HP.ToString(sport.ScoreFormat) :
					 "KO");
				if (sport.Fancies || sport.Feints)
					Summary += " / " + order[i].MP.ToString();
				if (i < order.Count - 1) Summary += n;
			}
			
			// Check for KO'd combatants and desensitize(?) them.
			foreach (Combatant c in order)
			{
				// First, reset them.
				c.Reset();
				if (c.HP <= 0) c.Sensitive = false;
			}
			
			// Create a list to hold the new order.
			List<Combatant> newOrder = new List<Combatant>();
			
			// Add each combatant, from #2 on, into the new list.
			for (int i = 1; i < order.Count; i++)
				if (order[i].HP > 0) newOrder.Add(order[i]);
			
			// And then put the old #1 duelist at the end.
			if (order[0].HP > 0) newOrder.Add(order[0]);
			
			// Save the new order.
			order = newOrder;
			
			// Update the order label and list store.
			Order.Clear();
			orderLabel.Text = "";
			for (int i = 0; i < order.Count; i++)
			{
				Order.AppendValues(order[i].CName);
				orderLabel.Text += order[i].CName;
				if (i < order.Count - 1) orderLabel.Text += ", ";
			}
			
			// Check to see if we need to enter final two mode.
			if (order.Count == 2)
			{
				order[0].Target = 1;
				order[0].FinalTwo = true;
				order[1].Target = 0;
				order[1].FinalTwo = true;
			}
			
			// Check to see if the brawl's over.
			if (order.Count == 1)
			{
				// Desensitize the final combatant.
				order[0].Sensitive = false;
				
				// Print victory message!
				Summary += n + n + order[0].CName +
					" has won the brawl!" + n + n +
					"(( " + order[0].CName + " .def. ";
				
				// Check to see if the victor is the last combatant
				// in the original order.
				int wLength = combatantBox.Children.Length;
				if (((Combatant)combatantBox.Children[wLength - 1]).CName ==
				    order[0].CName) wLength--;
				
				// Print all the duelists except the winner out into
				// the final line.
				for (int w = 0; w < wLength; w++)
				{
					Combatant c = (Combatant)combatantBox.Children[w];
					if (c.CName != order[0].CName)
						Summary += (w == wLength - 1 ? "and " : "") +
							c.CName + (w < wLength - 1 ? ", " : "");
				}
				
				Summary += " with " + order[0].HP.ToString(sport.ScoreFormat) +
					" HP remaining, in " + Round.ToString() + " rounds ))";
			}
			
			// Scroll the summary to the bottom.
			summaryView.Buffer.MoveMark("scroll", summaryView.Buffer.EndIter);
			summaryView.ScrollMarkOnscreen(summaryView.Buffer.GetMark("scroll"));
			
			// Save the brawl thus far to a file.
			string fileName = duelNum.ToString("00") + ". Brawl";
			if (Arbiter.FightNight) fileName += " (" + sport.ShortName + ")";
			fileName += ".txt";
			string path = System.IO.Path.Combine(Arbiter.CurrentDir, fileName);
			
			// Open the file and write the contents of the buffer to it.
			StreamWriter sw = new StreamWriter(path, false);
			sw.Write(Summary);
			sw.Close();
			
			// Advance round number.
			Round++;
			
			// Make the resolver insensitive.
			resolveButton.Sensitive = false;
		}
	}
}

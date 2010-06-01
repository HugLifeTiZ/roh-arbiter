// 
// EditDialog.cs
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
using Gtk;
using Glade;

namespace Arbiter
{
	// This is a dialog for editing the lists of
	// Duelists and Rings. It's actually pretty simple.
	public class EditDialog
	{
		[Widget] private Dialog editDialog;
		[Widget] private ScrolledWindow listScroll;
		private TreeView listView;
		private ListStore list;
		
		// Constructor.
		public EditDialog (string title, ListStore list, bool reorderable)
		{
			// Load the GUI.
			XML xml = new XML("GUI.glade", "editDialog");
			xml.Autoconnect(this);
			editDialog.Title = title;
			editDialog.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			
			// Create the TreeView.
			listView = new TreeView();
			listScroll.Add(listView);
			listView.Reorderable = reorderable;
			listView.HeadersVisible = false;
			listView.SearchColumn = 0;
			
			CellRendererText cell = new CellRendererText();
			cell.Editable = true;
			cell.Edited += CellEdited;
			listView.AppendColumn("Name", cell, "text", 0);
			
			listView.Selection.Mode = SelectionMode.Browse;
			
			this.list = list;
			listView.Model = list;
			
			// Appear!
			editDialog.ShowAll();
		}
		
		// Convenience.
		public void Run ()
			{ editDialog.Run(); }
		
		// Closes the dialog.
		private void OkClicked (object sender, System.EventArgs e)
			{ editDialog.Destroy(); }
		
		// Adds a new item to the list.
		private void AddToList (object sender, System.EventArgs e)
		{
			Dialog dialog = new Dialog("Enter name:", editDialog,
			                           DialogFlags.NoSeparator | DialogFlags.Modal,
			                           new object[] {Stock.Cancel, ResponseType.Cancel,
										Stock.Ok, ResponseType.Ok});
			dialog.Icon = Gdk.Pixbuf.LoadFromResource("RoH.png");
			dialog.WindowPosition = WindowPosition.Center;
			Label label = new Label("Enter a name:");
			label.SetAlignment(0.0f, 0.5f);
			dialog.VBox.PackStart(label);
			Entry entry = new Entry();
			entry.ActivatesDefault = true;
			dialog.VBox.PackStart(entry);
			dialog.Default = dialog.ActionArea.Children[0];
			dialog.VBox.ShowAll();
			int res = dialog.Run();
			
			if (res == (int)ResponseType.Ok)
			{
				TreeIter iter = list.InsertWithValues(0, entry.Text);
				listView.ScrollToCell(list.GetPath(iter), listView.Columns[0],
				                      true, 0.5f, 0.5f);
			}
			dialog.Destroy();
		}
		
		// Deletes an item from the list.
		private void DeleteFromList (object sender, System.EventArgs e)
		{
			TreeIter iter;
			listView.Selection.GetSelected(out iter);
			list.Remove(ref iter);
		}
		
		// Updates the list when a cell is edited.
		private void CellEdited (object o, EditedArgs args)
		{
			TreeIter iter;
			list.GetIter(out iter, new TreePath(args.Path));
			list.SetValues(iter, args.NewText);
		}
	}
}

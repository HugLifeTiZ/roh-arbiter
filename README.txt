Arbiter v0.3
by Trent McPheron (twilightinzero@gmail.com)
=======================

== Changelog ==
-v0.3-
 *Switched to MIT/X11 license. It's more permissive.
  Besides, I don't like GNU's fanaticism. :P
 *Made the program look less foreign on Windows with
  a special gtkrc that will be used only on Windows.
 *Separated settings file into three files: One for
  basic settings, one for the duelist list, and one
  for the ring list.
 *Created a shift report viewer in the new duel tab.
 *Rearranged the swords and fists moves a little.
 *Changed the swords icons.
 *This isn't anything an end user will notice, but
  it's important none the less: the icons for the
  duel tabs were formerly embedded pre-scaled. Now
  the icons are embedded at original size and scaled
  by the program. This is probably safer from a legal
  standpoint, 'cos the icon pack says "free to use",
  but not "free to modify". There is no visual
  difference between the two methods.
-v0.2.1-
 *First release with a proper name. I was incredibly
  indecisive with this. First, it was Cedran, named
  after Brig. Then it was Sparky, the name that Dris
  came up with for Jaycy's autocaller. I finally
  settled with Arbiter. It's more or less a synonym
  for judge or referee. It also refers to someone
  who oversees some kind of interaction.
 *Corrected some errors in the DoM matrix, and one
  in the DoS Matrix.
 *Added a check in the venue loader that makes sure
  the matrix is at least consistent. E.g.: MW-IM and
  IM-MW won't give .5 to B no matter which way it's
  put in. This is actually the very error that Tass
  reported to me, and in so doing, it revealed more
  errors.
 *Updated the default duelist list. We've broken 100!
  The default list has 107 duelists! :D For the time
  being, though, in order to get in on that, you'll
  have to nuke your settings file. v0.3 will rectify
  this.
 *This is mostly a bug-fix release. No real features
  have been added. It's an important bug-fix release,
  though.
-v0.2-
 *GUI source switched from Stetic to Glade. This
  makes the source code more accessible to developers
  without MonoDevelop. Plus, it's easier to use and
  maintain; Stetic was rather unstable.
 *Along with that, a huge GUI revamp.
 *The undo feature! Reverse any round, even RFx2, and
  and even un-end duels.
 *A list of duelists and rings, as well as a simple
  dialog to edit them. You can use the drop down box
  to pick duelists and rings, or suggestions will be
  made as you type.
 *If an existing shift report is found in the day's
  folder, the program will offer to load it.
 *Fixed the newline bug.
 *If it's earlier than 3 AM and the program is
  started, the correct day will still be used.
 *Now loads the close icon from the theme and resizes
  it, so that one does not need to be included anymore.
 *Big code refactoring, more for my benefit.
-v0.1-
 *Initial release.

== Intro ==
This is an application to assist Rings of Honor officials in
calling and logging duels, mainly intended to be used in the RDI,
or other settings where the flash chat caller tool is unavailable.
It is written in C# and uses GTK# to draw the GUI, thus it
requires GTK# and the .NET Framework (or Mono) to run. As long
as you have those, it will run on Windows, Mac OS, and Linux.
Recent versions of Windows already have the .NET Framework
installed.

If you're on Windows, install GTK# from the included installer,
or download the GTK# runtime here:
http://ftp.novell.com/pub/mono/gtk-sharp/gtk-sharp-2.12.9-2.win32.msi

If you're on Mac OS, download Mono and the GTK# runtime here:
(Not included because it's pretty big.)
http://ftp.novell.com/pub/mono/archive/2.4.2.3/macos-10-universal/6/MonoFramework-2.4.2.3_6.macos10.novell.universal.dmg

If you're on Linux, use your package manager to get the
required software.

== Features ==
This program can call for all three venues, and supports calling
normal, challenge, and madness duels. Each tab has a special icon
for each venue/type combo, even for DoF and DoM Madness, which
don't actually exist yet. ;) Like the tool in the flash chat, it
will resolve the moves for you and update the scores. The interface
resembles that of the tool in the flash chat. It has no way of
knowing what moves each opponent is picking, though, so you'll have
to input them into the tool yourself, by way of the provided
comboboxes. Thus its use is more pronounced if you need to (or want
to) log the duels, as described below.

== Logging ==
An additional feature it has over the flash chat tool is that it
will create shift reports for you, and it will log each duel. The
logs will be stored in a subdirectory of a directory to be chosen
by the user. So if your log directory is C:/Users/Blah/Documents/
Duels, the duels that are called on October 31, 2009 will be in
C:/Users/Documents/Duels/2009-10-31. It will create a bare-bones
shift report as well, consisting of the date and the final lines
of each duel fought while the program is open. If you edit the
shift report while the program is open, to insert comments for
example, those changes will be lost once another duel ends.

== Custom short names ==
You should put each duelist's name into the entry boxes with their
official name, as opposed to a nickname or just their first name.
The program normally determines the short name of a duelist by
splitting by spaces and taking the first word. In some cases, this
is not sufficient. You can explicitly set the short name of a
duelist with a slash. Do not include a space before or after the
slash. For examples, look at the included list of duelists (see
below). If the program determines a short name by itself and finds
it to be longer than ten characters, it will be truncated. This is
a fail-safe for if the slash is accidentally omitted. This behavior
can be overriden in case of intentionally long names, by explicitly
setting the short name.

== Duelist/Ring Lists ==
The program will maintain a list of duelists and rings that can be
picked from a drop-down box, or be used to make suggestions as you
type. When you start a duel with a duelist not already on the list,
that duelist will be added. This doesn't apply to the rings; you
will have to use the included list editor. To edit one of the lists,
click on the appropriate button next to the duelist or ring entries.
A dialog will appear with the desired list, and you can add, delete,
or edit an item. Use the buttons on the right to add or delete, and
to edit, double click on the desired item. The list of duelists is
always sorted alphabetically, and cannot be re-arranged. The list of
rings, however, can be re-arranged by clicking on an item and
dragging it wherever you want it. The program includes a list of
duelists constructed from all duelists that have fought since
October, and my own ring set is included as an example. You should
change it if you're not a Tales geek. ;) If you call for Fists, make
sure to keep the rings Styx, Pit, Fern, Can, and Pool, and if you
call for Magic, make sure to keep Poseidon, Tartarus, Anemoi, Titan,
and Olympus.

== Other Stuff ==
If you have any trouble with this program, use the e-mail address
at the top of this readme to let me know. This includes program
bugs, matrix errors, etc.

This program is licensed under the MIT/X11 License, which means that
it is free and open source. The source code can be downloaded from the
same place you downloaded the program itself, and you can do, for the
most part, whatever you want with it.

I'm not a professional programmer, so I'm not promising the code
will actually be of professional quality. ;P Any changes for the
good of the program will be welcomed as long as it doesn't stray
too far from its original purpose or vision. That is to say, I won't
put it into my version of the tool if I don't want it. Nothing's
stopping you from releasing your own version. ;)

The icons used for each duel type are resized from the icons in
the pack here by Henrique Lazarini: http://www.pixeljoint.com/pixelart/37532.htm
The icon for the new duel tab and the window icons is © 2004-2009
Rings of Honor.

With all that out of the way, enjoy using the Arbiter! :)
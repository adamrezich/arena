arena
=====

arena is an awesome, vectory, minimalist MOBA-in-progress.


Overview
--------

arena is a game wherein two teams of five players each compete to destroy the others' base.


Installation notes
------------------

Requires [GTK+](http://www.gtk.org/download/), as well as [this font](http://www.dsg4.com/04/extra/bitmap/stuff/04b_19.zip).

If you just wanna try the game out, the latest "stable" build is available [here](http://rezich.com/Arena.zip) (complete with the font and all the GTK DLLs, so you don't need to install GTK+; just extract and go), and a server should be running at nukle.us. Just type "nukle.us" in for the server address!


Controls
--------

The following are the default controls. I say "default" but really there's no way to change them yet. Oops!

- Mouse0 - click on ground to move, click on enemy to autoattack
- Mouse1 - click on unit to select, drag to select multiple units*
- Q - ability 1
- W - ability 2
- E - ability 3
- R - ability 4 (ultimate)
- T - chatwheel**
- A - click to attack unit (including allied units, when possible)*
- D - display stats panel*
- F - level up ability*
- S - stop moving*
- F1 - unlock cursor
- F2 - toggle anti-aliasing
- F3 - toggle double-buffering
- Space - center camera on currently-selected unit
- Enter - chat to your team
- Shift+Enter - chat to everyone on the server
- Esc - cancel chat, quit game
- Tab - show scoreboard

\* not implemented
\** half-assedly implemented


Positions
---------

### Grappler ###

#### Abilities ####

- **Grab** - Reach out with a hook to grab an enemy unit and pull him in.
- **Hookshot** - Fire a hook that grabs onto pillars and pulls you to them, dealing damage to enemies along the way.
- **Tackle** - Fire a hook in the direction you're facing, and, if it hits an enemy, pulls you to them.
- **Grapple** - Disable target enemy unit and yourself.


### Runner ###

#### Abilities ####

- **Sprint** - Temporarily gain a move speed bonus.
- **Longshot** - Passively gain attack range. This is just an example spell.
- **Agility Aura** - Passively gain move speed, turn speed, and attack speed.
- **???** - ???


### Pusher ###


### Nuker ###
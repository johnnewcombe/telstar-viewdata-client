# Telstar Viewdata Client

Experimental cross platform Viewdata Client written using Avalonia on .Net 9.

## TODO:

* Configuration files etc.
* Serial Port access.
* Full Screen
* Logging needs to be sorted.
* Alt+Q needs to work from all screens, maybe help too?


## Viewdata Nuances

If a character on the screen is updated, this may very well affect
the following characters up until the end of the row. There are
some basic rules to be followed i.e.

Rules:
  
* A foreground colour change affects all following characters in the row up
  until another colour change is found.
* A new background control character affects all following characters in the
  row until either another new background is found or if s a black
  background control character is found.
* A new background control character is applied to the cell containing the NB
  control code.
* A black background control character affects all following characters in the
  row until either another black background is found or if a new
  background control character is found.
* A black background control character is applied to the cell containing the NB
  control code.
* A double height control character affects all following characters in the row
  until a normal height control character is found.
* Any row containing a double height character will cause the row below to be
  read only.
* Flash affects all following characters in the row until a steady control
  character is found.
* Separated graphics control character affects all following characters in the row until a
  Contiguous graphics control character is found.
* Invalid escape codes e.g. ESC/0E or ESC/0F are treated the same as valid ones
* in terms of displaying either a blank or a graphics hold.

## Graphics Hold Rules
GH gets the Graphic char, note that a blank in graphics mode is a graphic char so a control that follows a blank has
the graphic blank i.e a blank space. Note also that following a graphic hold, even an Alpha foreground will be displayed as the GH
char but everything after that will not until a Graphic foreground is shown.

The GH character displayed for an alpha or graphic foreground control takes previous cell colour not new one.

The GH character is set to blank for an Alpha char and for each new row.


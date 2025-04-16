# Telstar Viewdata Client

Experimental cross platform Viewdata Client written using Avalonia on .Net 9.

## TODO:

* Add Double Height.
* Sort display (rest of row) when insert point is selected by cursor row/col.
* Keyboard Input
* Configuration files etc.
* Serial Port access.
* Full Screen
* Cursor display and on/off control
* Make sure ProcessReceiveBuffer can't be called before the previous call has completed. Perhaps use Long Running Task so that the app is not relient on working at a slow baud rate.


## Rest of Row Rules

* A foreground colour change affects all following until another colour change.
* A new background affects all following until black background.
* New background is applied to the cell containing the NB control code.
* Double height affects all following until normal height
* Flash affects all until steady.
* Separated affects all that follow until Contiguous. ???? DOES THE BACGROUND FLASH?
* 



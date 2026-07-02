# Telstar Viewdata Client

Experimental cross platform Viewdata Client.

Platforms:

* Linux (x64/arm64)
* MacOS (intel/Apple Silicon)
* Windows (x64/arm64)


![alt text](./Assets/ScreenshotAll.png)

## Outstanding Development:

* Support for serial ports.

## Installation

### MacOS Installation

Download the appropriate zip from releases:
 
- `TelstarClient-macos-arm64-<version>.zip` — Apple Silicon (M1/M2/M3/M4)
- `TelstarClient-macos-x64-<version>.zip` — Intel

Extract the zip and move `TelstarClient.app` to your Applications folder.

#### First Run

As TelstarClient is not notarized with Apple, macOS may report it as damaged
or block it from running. To bypass this:

**Option 1 — Right-click method:**
Right-click `TelstarClient.app` → Open → click Open in the dialog that appears.
This only needs to be done once.

**Option 2 — Terminal method:**
```bash
xattr -cr /Applications/TelstarClient.app
```
Then double-click as normal.

**Option 3 — System Settings:**
System Settings → Privacy & Security → scroll down → click Open Anyway.

**Option 4 — Ad-hoc signing:**
If you have Xcode command line tools installed:
```bash
codesign --force --deep --sign - /Applications/TelstarClient.app
```
Then double-click as normal.

### Linux

Download the appropriate zip from releases:

- `TelstarClient-linux-arm64-<version>.zip` — arm64
- `TelstarClient-linux-x64-<version>.zip` — x64

```bash
$ ./install.sh
```
You will be presented with an option to install for all users or for the current user.

### Windows

Download the appropriate executable from releases:

- `TelstarClient-Win-arm64-Setup-<version>.zip` — ARM 64
- `TelstarClient-Win-x64-Setup-<version>.zip` — x64

Run the downloaded setup program.

## Log Files

* Windows → %APPDATA%
* macOS → ~/Library/Application Support
* Linux → ~/.config/TelstarClient/logs

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

## Development Notes

The build process is automated using Github Actions. See the .github/workflows/ReleaseBuilder.yml file.

This is triggered by a new tag being pushed to the repository.
```text
git tag v0.0.15
git push origin v0.0.15
```
This will trigger the build process and create a new release for Linux, MacOS and Windows within 
GitHub Releases. Releases can be seen on the right hand side of the Github Code tab.

If the label exists already it can be deleted first with the following command.

```text
git tag -d v0.0.15
git push origin :refs/tags/v0.0.15
```

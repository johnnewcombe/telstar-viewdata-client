#!/bin/bash

APPNAME="TelstarClient"
INSTALL_DIR_SYSTEM="/usr/local/bin/$APPNAME"
INSTALL_DIR_USER="$HOME/.local/bin/$APPNAME"
DESKTOP_DIR_SYSTEM="/usr/share/applications"
DESKTOP_DIR_USER="$HOME/.local/share/applications"
ICON_DIR_SYSTEM="/usr/share/icons/hicolor/256x256/apps"
ICON_DIR_USER="$HOME/.local/share/icons/hicolor/256x256/apps"

echo "==============================="
echo " $APPNAME Installer"
echo "==============================="
echo ""
echo "Where would you like to install $APPNAME?"
echo "  1) System-wide (/usr/local/bin) - requires sudo"
echo "  2) Current user only (~/.local/bin) - no sudo needed"
echo ""
read -p "Enter choice [1/2]: " CHOICE

case $CHOICE in
    1)
        INSTALL_DIR=$INSTALL_DIR_SYSTEM
        DESKTOP_DIR=$DESKTOP_DIR_SYSTEM
        ICON_DIR=$ICON_DIR_SYSTEM
        USE_SUDO=true
        ;;
    2)
        INSTALL_DIR=$INSTALL_DIR_USER
        DESKTOP_DIR=$DESKTOP_DIR_USER
        ICON_DIR=$ICON_DIR_USER
        USE_SUDO=false
        ;;
    *)
        echo "Invalid choice, exiting."
        exit 1
        ;;
esac

# Helper to run commands with or without sudo
run() {
    if [ "$USE_SUDO" = true ]; then
        sudo "$@"
    else
        "$@"
    fi
}

echo ""
echo "Installing $APPNAME to $INSTALL_DIR..."

# Create install directory and copy files
run mkdir -p "$INSTALL_DIR"
run cp -r ./* "$INSTALL_DIR/"
run chmod +x "$INSTALL_DIR/$APPNAME"

# Copy icon
run mkdir -p "$ICON_DIR"
run cp "./icon.png" "$ICON_DIR/telstarclient.png"

# Create .desktop file
DESKTOP_FILE="[Desktop Entry]
Name=Telstar Viewdata Terminal
Comment=Telstar Viewdata Terminal
Exec=$INSTALL_DIR/$APPNAME
Icon=telstarclient
Terminal=false
Type=Application
Categories=Network;
"

run mkdir -p "$DESKTOP_DIR"
echo "$DESKTOP_FILE" | run tee "$DESKTOP_DIR/telstarclient.desktop" > /dev/null
run chmod +x "$DESKTOP_DIR/telstarclient.desktop"

# Update desktop database
if command -v update-desktop-database &> /dev/null; then
    run update-desktop-database "$DESKTOP_DIR"
fi

# Update icon cache
if command -v gtk-update-icon-cache &> /dev/null; then
    run gtk-update-icon-cache -f -t "$ICON_DIR/../.."
fi

# Write uninstaller
UNINSTALL_SCRIPT="#!/bin/bash
echo \"Uninstalling $APPNAME...\"
$([ "$USE_SUDO" = true ] && echo 'sudo rm -rf' || echo 'rm -rf') \"$INSTALL_DIR\"
$([ "$USE_SUDO" = true ] && echo 'sudo rm -f' || echo 'rm -f') \"$DESKTOP_DIR/telstarclient.desktop\"
$([ "$USE_SUDO" = true ] && echo 'sudo rm -f' || echo 'rm -f') \"$ICON_DIR/telstarclient.png\"
echo \"$APPNAME uninstalled successfully.\"
"

run mkdir -p "$INSTALL_DIR"
echo "$UNINSTALL_SCRIPT" | run tee "$INSTALL_DIR/uninstall.sh" > /dev/null
run chmod +x "$INSTALL_DIR/uninstall.sh"

echo ""
echo "$APPNAME installed successfully to $INSTALL_DIR"
echo "To uninstall, run: $INSTALL_DIR/uninstall.sh"
echo ""
/*
    Copyright (c) 2025 John Newcombe

    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

*/

using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TelstarClient.ViewModels;

namespace TelstarClient.Views;

/// <summary>
/// Interaction logic for MainWindow.axaml.
/// Handles the main window setup, user input, and the rendering of the Viewdata display.
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger =
        App.Host.Services.GetRequiredService<ILogger<MainWindow>>();

    private MainWindowViewModel ViewModel { get; set; }

    private bool KioskMode { get; set; }

    // Created once on first display update and reused for every subsequent
    // frame (see UpdateDisplay) - a WriteableBitmap's pixel content is
    // mutable in place via Lock(), so there's no need to allocate a new
    // native bitmap surface on every single character/frame update.
    private WriteableBitmap _displayBitmap;

    public MainWindow()
    {
        InitializeComponent();

        // Subscribe to DataContext changes to link the Viewmodel's property changes to the display handler.
        DataContextChanged += (_, _) =>
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                ViewModel = viewModel;
                ViewModel.PropertyChanged += PropertyChangedEventHandler;
            }
        };

        // Initialize the display grid with character labels.
        //Display.Children.Clear();

        // Create an extra row of labels to accommodate the status line.
        //for (int i = 0; i < ViewdataDisplay.Display.Cols * (ViewdataDisplay.Display.Rows + 1); i++)
        //{
        //    var g = InitCharacterLabel(ViewdataDisplay.Display.Spc);
        //    Display.Children.Add(g);
        //}
    }

    /// <summary>
    /// Handles changes in the ViewModel properties, specifically triggering display updates.
    /// </summary>
    private void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            
            case nameof(ViewModel.Bitmap):
                try
                {
                    // Execute display updates on the UI thread to avoid cross-thread exceptions.
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update the display");
                }

                break;

            case nameof(ViewModel.Cursor):
                try
                {
                    UpdateCursor();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to update the cursor");
                }

                break;
        }
    }

    /// <summary>
    /// Toggles the window between normal mode and full-screen kiosk mode (no chrome, topmost).
    /// </summary>
    public void ToggleKioskMode()
    {
        if (KioskMode)
        {
            // Turn kiosk mode off: restore standard window decorations and state.
            ExtendClientAreaToDecorationsHint = false;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.Default;
            WindowState = WindowState.Normal;
            Topmost = false;
        }
        else
        {
            // Turn kiosk mode on: hide window chrome and set to full-screen.
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
            WindowState = WindowState.FullScreen;
            Topmost = true;
        }

        // Toggle the kiosk mode flag.
        KioskMode = !KioskMode;
    }

    /// <summary>
    /// Handles key press events, passing them to the ViewModel for processing.
    /// </summary>
    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        _logger.LogDebug("Keypress:{Key}, Physical Key:{PhysicalKey}, Modifiers:{Modifiers}", e.Key,
            e.PhysicalKey, e.KeyModifiers);
        try
        {
            ViewModel.KeyHandler(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
        }
    }

    /// <summary>
    /// Handles text input events, passing them to the ViewModel for processing.
    /// </summary>
    private void Window_TextInput(object sender, TextInputEventArgs e)
    {
        _logger.LogDebug("TextInput:{Text}", e.Text);
        try
        {
            ViewModel.TextHandler(e);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Updates the cursor visual representation on the display.
    /// </summary>
    private void UpdateCursor()
    {
        _logger.LogDebug("Updating cursor");

        var cursor = ViewModel.Cursor;
        if (cursor is not null &&
            cursor.Visible)
        {
            //var label = (Label)((Viewbox)Display.Children[cursor.GetCursorIndex()]).Child;
            //if (label != null) label.Content = "_";
        }
    }

    /// <summary>
    /// Updates the display grid based on the data received from the ViewModel.
    /// </summary>
    private void UpdateDisplay()
    {
        //_logger.LogDebug("Updating display");

        if (_displayBitmap is null)
        {
            _displayBitmap = BitmapConverter.CreateWriteableBitmap(width: 480, height: 500);
            BitmapDisplay.Source = _displayBitmap;
        }

        BitmapConverter.UpdatePixels(_displayBitmap, ViewModel.Bitmap);
        BitmapDisplay.InvalidateVisual();

    }
}
using TelstarClient.Models;

namespace TelstarClient.DisplayManager;

/// <summary>
/// This class manages the Display model. Methods here update the Display model
/// and provide all the Viewdata decoding etc., 
/// </summary>
public class DisplayManager
{
    private Display _display;
    private readonly Cursor _cursor;
    private ViewdataUtils _viewdataUtils;

    /// <summary>
    /// Contructor which creates a new Display Model.
    /// </summary>
    public DisplayManager()
    {
        _display = new Display();
        _cursor = new();
        _viewdataUtils = new (_display,_cursor);
    }
    
    /// <summary>
    /// Places a single Unicode 16 character in the Display model
    /// the current cursor position.
    /// </summary>
    /// <param name="character"></param>
    /// <returns></returns>
    public (int, char)? PrintChar(char character)
    {
        
        // process for viewdata
        character = _viewdataUtils.ConvertChar(character);
        if (character == '\x00')
        {
            // a control code was received and actioned so ignore
            return null;
        }

        // get the position index e.g. 0-959
        var index = _cursor.Row * _cursor.Col + _cursor.Col;
        _display.Rows[_cursor.Row].Cells[_cursor.Col].Character = character;

        // move cursor for next character
        _cursor.HorizontalTab();

        // return position index and character as a tuple
        return (index, character);
    }

    /// <summary>
    /// Clear the screen by creating a new one.
    /// </summary>
    private void ClearScreen()
    {
        // TODO: How does this propagate to the display! Event?
        _display = new Display();
    }
}
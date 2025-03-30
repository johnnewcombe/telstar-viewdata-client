using System.IO.Pipelines;

namespace TelstarClient.DisplayManager;

public class ViewdataUtils
{
    // character constants
    private const char NULL = '\x00';
    private const char BS = '\x08';
    private const char HT = '\x09';
    private const char LF = '\x0a';
    private const char VT = '\x0b';
    private const char HOME = '\x0c';
    private const char HOMECLR = '\x1e';
    private const char CR = '\x0d';
    private const char ESC = '\x1b';

    
    private bool _escapedMode;
    private bool _doubleHeight;
    private Cursor _cursor;

    public ViewdataUtils(Cursor cursor)
    {
        _cursor = cursor;
    }

    public char ConvertChar(char character)
    {
        // process control codes
        // null character will be returned if a control
        if (ProcessControls(character))
            return NULL;
        
        
        return character;
    }

    private bool ProcessControls(char character)
    {
        // assume we are going to process a control
        var result = true;
        
        // if any of these get detected then NULL character is returned otherwise
        // the passed character is returned unaltered.
        switch (character)
        {
            case '\x08':
                _cursor.Backspace();
                break;           
            case '\x09':
                _cursor.HorizontalTab();
                break;
            case '\x0a':
                _cursor.LineFeed();
                break;
            case '\x0b':
                _cursor.VerticalTab();
                break;
            case '\x0c':
            case '\x1e':
                _cursor.Home();
                break;
            case '\x0d':
                _cursor.CarriageReturn();
                break;
            case '\x11':
                _cursor.Visible = true;
                break;
            case '\x14':
                _cursor.Visible = false;
                break;
            case '\x1b':
                _escapedMode = true;
                break;  
            default:
                // didn't process a control so indicate as such
                result = false;
                break;
        }

        return result;

    }
}

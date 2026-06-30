using System.Collections.Generic;
using System.Linq;
using TelstarClient.Configuration;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public abstract class FormBase : IForm
{
    private int _currentField = 0;
    protected DisplayManager _displayManager;

    protected FormBase(DisplayManager displayManager, Configuration.Connection connection)
    {
        _displayManager = displayManager;
        Connection = connection;
    }

    protected List<Field> Fields { get; set; } = new List<Field>();

    public bool ProcessFormKey(int asciiValue)
    {
        //var currentField = _currentForm.GetCurrentField();
        if (asciiValue == 0x1B) // escape 
        {
            //_currentForm = null;
            return false;
        }

        if (asciiValue == 0x08) // backspace
        {
            if (GetCurrentField().Value.Length > 0)
            {
                // remove the char from the display by setting the cursor to the current position
                // and then writing a space character
                _displayManager.SetCursorPosition(GetCurrentField().Value.Length - 1
                                                  + GetCurrentField().StartIndex);
                _displayManager.Write((char)0x20);


                // remove the char from the value property of the field this reduces he value
                // length which is used when calculating where to place the cursor
                GetCurrentField().Value =
                    GetCurrentField().Value.Substring(0,
                        GetCurrentField().Value.Length - 1);

                //set cursor and update display
                SetCursor();

                return true;
            }
        }

        // shift tab
        if (asciiValue is 0x89)
        {
            if (Previous())
            {
                //set cursor and update display
                SetCursor();
            }

            return true;
        }

        // are we terminating the field?
        if (asciiValue is 0x0d or 0x09 ||
            GetCurrentField().Value.Length >= GetCurrentField().Length)
        {
            if (Next())
            {
                //set cursor and update display
                SetCursor();
                return true;
            }

            // all done
            return false;
        }

        if (asciiValue >= 0x20 && asciiValue < 0x80)
        {
            if (GetCurrentField().Type is FieldType.Alpha && !char.IsAsciiLetterOrDigit((char)asciiValue))
                return true;
            if (GetCurrentField().Type == FieldType.Numeric && !char.IsAsciiDigit((char)asciiValue))
                return true;

            GetCurrentField().Value += (char)asciiValue;
            _displayManager.Write((char)asciiValue);

            //set cursor and update display
            SetCursor();
        }

        return true;
    }

    public Connection Connection { get; }

    public Field GetCurrentField()
    {
        return Fields[_currentField];
    }

    /// <summary>
    /// Get the fiels based on the 'id' property.
    /// </summary>
    /// <param id="name"></param>
    public Field GetFieldById(string id)
    {
        return Fields.FirstOrDefault(f => f.ID == id);
    }

    private bool Next()
    {
        if (_currentField < Fields.Count - 1)
        {
            _currentField++;
            return true;
        }

        return false;
    }

    private bool Previous()
    {
        if (_currentField > 0)
        {
            _currentField--;
            return true;
        }

        return false;
    }

    public bool IsValid()
    {
        // TODO: must check the values in each field to ensure they are valid for the field type
        //   and that all required fields are present.
        foreach (var field in Fields)
        {
            if (!field.IsValid())
            {
                return false;
            }
        }

        return true;
    }

    // Forces derived classes to implement their own ToString
    public override string ToString()
    {
        foreach (var field in Fields)
        {
            return $"{field.ID}:{field.Value}";
        }

        return string.Empty;
    }
    
    public int GetCursor()
    {
        return GetCurrentField().StartIndex + GetCurrentField().Value.Length;
    }

    private void SetCursor()
    {
        //set cursor and update display
        _displayManager.SetCursorPosition(GetCurrentField().Value.Length +
                                          GetCurrentField().StartIndex);
    }
}
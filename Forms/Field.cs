using System;

namespace TelstarClient.Forms;

public enum FieldType
{
    AlphaNumeric = 0,
    Alpha = 1,
    Numeric = 2,
}

public class Field
{
    public Field(string id, int startIndex, int length, string value, FieldType type, bool required)
    {
        ID = id;
        StartIndex = startIndex;
        Length = length;
        Value = value;
        Type = type;
    }

    public Field(string id, int col, int row, int length, string value, FieldType type, bool required)
    {
        ID = id;
        StartIndex = col*Models.Display.COLS + row;
        Length = length;
        Value = value;
        Type = type;
    }

    public string ID;
    public int StartIndex = 0; // cell position on the display
    public int Length = 0; // number of cells
    public string Value = String.Empty;
    public FieldType Type = FieldType.AlphaNumeric;
    public bool IsRequired =false;
    
    //private int currentIndex = 0; // keeps track of where the cursor is for this field

    /*

    public int CurrentIndexInField
    {
        get { return currentIndex; }
    }

    /// <summary>
    /// Moves the cursor to the next element in the field. Returns false if the field is full, true otherwise.
    /// </summary>
    /// <returns></returns>
    public bool HT()
    {
        if (currentIndex < StartIndex+Length)
        {
            currentIndex++;
            return true;
        }
        return false;
    }
*/
    
}
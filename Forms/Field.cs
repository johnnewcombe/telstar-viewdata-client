using System;
using System.Linq;

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
    
    // TODO turn this into a property so that Value if null is returned as an empty string
    //  that will preven .Length from failing.
    public string Value = String.Empty;
    public FieldType Type = FieldType.AlphaNumeric;
    public bool IsRequired =false;

    public bool IsValid()
    {
        // Check required fields have a value
        if (IsRequired && string.IsNullOrWhiteSpace(Value))
            return false;

        // If not required and empty, no further checks needed
        if (string.IsNullOrWhiteSpace(Value))
            return true;

        // Check length
        if (Value.Length > Length)
            return false;

        // Check type
        switch (Type)
        {
            case FieldType.Alpha:
                return Value.All(c => char.IsBetween(c,(char)0x20,(char)0x7f));
            case FieldType.AlphaNumeric:
                return Value.All(c => char.IsLetterOrDigit(c) || c == '.');
            case FieldType.Numeric:
                return Value.All(char.IsDigit);
            default:
                return false;
        }
    }
}
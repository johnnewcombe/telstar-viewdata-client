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
        Id = id;
        StartIndex = startIndex;
        Length = length;
        Value = value;
        Type = type;
        IsRequired = required;
    }

    public Field(string id, int row, int col, int length, string value, FieldType type, bool required)
    {
        Id = id;
        StartIndex = row*Models.Display.Cols + col;
        Length = length;
        Value = value;
        Type = type;
        IsRequired = required;
    }

    public readonly string Id;
    public readonly int StartIndex; // cell position on the display
    public readonly int Length; // number of cells
    public readonly FieldType Type;
    public readonly bool IsRequired;

    private string _value = String.Empty;

    public string Value
    {
        get => _value ?? String.Empty;
        set => _value = value;
    }
    
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
            // TODO add different types for host, etc. so that validation can be better
            //  add a better way to indicate that a field is invalid rather than not
            //  reporting it etc.
            case FieldType.Alpha:
                return Value.All(c => char.IsBetween(c,(char)0x20,(char)0x7f));
            case FieldType.AlphaNumeric:
                return Value.All(c => c > 0x20 && c <=0x7f);
//                return Value.All(c => char.IsLetterOrDigit(c) || c == '.' || c == ' ');
            case FieldType.Numeric:
                return Value.All(char.IsDigit);
            default:
                return false;
        }
    }
}
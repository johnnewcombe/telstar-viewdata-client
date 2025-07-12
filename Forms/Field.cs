using System;

namespace TelstarClient.Forms;

public enum FieldType {
    AlphaNumeric = 0,
    Alpha = 1,
    Numeric = 2,
}

public class Field {
    public int StartIndex=0;
    public int Length=0;
    public string Value=String.Empty;
    public FieldType Type = FieldType.AlphaNumeric;
}
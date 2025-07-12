using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Edit : IForm {

    private int _currentField;

    public Edit() {

        Field f;

        // create fields
        Fields = new List<Field>();

        f = new Field {
            StartIndex = 247,
            Length = 20,
        };
        Fields.Add(f);

        f = new Field {
            StartIndex = 327,
            Length = 20,
        };
        Fields.Add(f);

        f = new Field {
            StartIndex = 407,
            Length = 20,
            Type = FieldType.Numeric
        };
        Fields.Add(f);

        f = new Field {
            StartIndex = 503,
            Length = 1,
            Type = FieldType.Numeric
        };
        Fields.Add(f);

    }

    public List<Field> Fields { get; set; }

    public Field GetCurrentField() {
        return Fields[_currentField];
    }

    public bool Next() {
        _currentField++;
        return _currentField < Fields.Count;
    }

    public string ToString() {

        var menu = new StringBuilder();
        menu.Append("\r\n");
        menu.Append(Converters.ConvertFromMarkup("[17][D]EDIT\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]NAME:\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]HOST:\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]PORT:\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]SAVE TO MEMORY (0-9):\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[3]Press Escape to Return to Terminal"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }

}
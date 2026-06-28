using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using TelstarClient.Display;
using Tmds.DBus.Protocol;

namespace TelstarClient.Forms;

public class EditConnection : FormBase
{
    private int _currentField;
private Configuration.Connection _connection;

    public EditConnection(Configuration.Connection connection)
    {
        _connection = connection;

        // create fields
        Fields = new List<Field>();

        Fields.Add(new Field(6, 7, 20, string.Empty, FieldType.AlphaNumeric, false));
        Fields.Add(new Field(8, 7, 20, string.Empty, FieldType.AlphaNumeric, true));
        Fields.Add(new Field(10, 7, 20, string.Empty, FieldType.Numeric, true));

        if (connection is not null)
        {
            Fields.Add(new Field(12, 31, 1, string.Empty, FieldType.Numeric, false));
        }
    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append("\r\n");
        menu.Append(Converters.ConvertFromMarkup("[17][D]EDIT\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]NAME:\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]HOST:\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]PORT:\r\n\n"));
        if (_connection is not null)
        {
            menu.Append(Converters.ConvertFromMarkup("[C]SAVE TO MEMORY? (0-9 or RTN):\r\n\n"));
        }
        menu.Append(Converters.ConvertFromMarkup("[3]Press Escape to Return to Terminal"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }
}
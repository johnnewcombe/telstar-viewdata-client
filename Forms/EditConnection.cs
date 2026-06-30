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

    public EditConnection(DisplayManager displayManager, Configuration.Connection connection) : base(displayManager,
        connection)
    {
        // create fields
        Fields = new List<Field>();

        // if connection is null then
        Fields.Add(new Field("dirName", 6, 7, 20, connection.Name, FieldType.AlphaNumeric, false));
        Fields.Add(new Field("host", 8, 7, 20, connection.Host, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("port", 10, 7, 20, connection.Port.ToString(), FieldType.Numeric, true));
    }

    public override string ToString()
    {
        // TODO Values of fileds will need to be included here

        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[17][D]EDIT\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]NAME:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[0].Value));
        menu.Append(Converters.ConvertFromMarkup("[C]HOST:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[1].Value));
        menu.Append(Converters.ConvertFromMarkup("[C]PORT:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[2].Value));
        menu.Append(Converters.ConvertFromMarkup("\n[9][C]Press[W]Escape[C]to Return"));

        return menu.ToString();
    }
}
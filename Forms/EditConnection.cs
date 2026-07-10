using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using TelstarClient.Configuration;
using ViewdataDisplay;

namespace TelstarClient.Forms;

public class EditConnection : FormBase
{

    public EditConnection(DisplayManager displayManager, IConnection connection) : base(displayManager,
        connection)
    {
        var name=string.Empty;
        var host = string.Empty;
        var port = 0;
        var parity = string.Empty;
        
        if (connection is TcpConnection tcp)
        {
            name = tcp.Name;
            host = tcp.Host ;
            port = tcp.Port;
            parity = tcp.Parity ? "Y" : "N";
        }
        
        // create fields
        Fields = new List<Field>();

        // if connection is null then
        Fields.Add(new Field("dirName", 6, 9, 20, name, FieldType.AlphaNumeric, false));
        Fields.Add(new Field("host", 8, 9, 32, host, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("port", 10, 9, 20, port.ToString(), FieldType.Numeric, true));
        Fields.Add(new Field("parity", 12, 9, 1, parity, FieldType.Bool, true));
    }

    public override string ToString()
    {

        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[17][D]EDIT\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]  NAME:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[0].Value));
        menu.Append(Converters.ConvertFromMarkup("[C]  HOST:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[1].Value));
        menu.Append(Converters.ConvertFromMarkup("[C]  PORT:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[2].Value));
        menu.Append(Converters.ConvertFromMarkup("[C]PARITY:[W][PLACEHOLDER]\r\n\n"))
            .Replace("[PLACEHOLDER]", Fields[3].Value);
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[10][C]Press[W]Alt-S[C]to Save\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Alt-D[C]to Delete\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Escape[C]to Return"));

        return menu.ToString();
    }
}
using System.Collections.Generic;
using System.Text;
using TelstarClient.Configuration;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class EditConnection : FormBase
{

    public EditConnection(DisplayManager displayManager, IConnection connection) : base(displayManager,
        connection)
    {
        var name=string.Empty;
        var host = string.Empty;
        var port = 0;
        
        if (connection is TcpConnection tcp)
        {
            name = tcp.Name;
            host = tcp.Host ;
            port = tcp.Port;
        }
        
        // create fields
        Fields = new List<Field>();

        // if connection is null then
        Fields.Add(new Field("dirName", 6, 7, 20, name, FieldType.AlphaNumeric, false));
        Fields.Add(new Field("host", 8, 7, 20, host, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("port", 10, 7, 20, port.ToString(), FieldType.Numeric, true));
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
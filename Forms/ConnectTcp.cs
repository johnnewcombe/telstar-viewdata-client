using TelstarClient.Configuration;

namespace TelstarClient.Forms;

using System.Collections.Generic;
using System.Text;
using ViewdataDisplay;

public class ConnectTcp : FormBase
{

    public ConnectTcp(DisplayManager displayManager, IConnection connection) : base(displayManager,
        connection)
    {
   
        // create fields
        Fields = new List<Field>();
        Fields.Add(new Field("host", 6, 9, 20, string.Empty, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("port", 8, 9, 20, string.Empty, FieldType.Numeric, true));
        Fields.Add(new Field("parity", 10, 9, 1, string.Empty, FieldType.Bool, true));
    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[12][D]CONNECT TCP\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]  HOST:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]  PORT:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]PARITY:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Alt-C[C]to Connect\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Escape[C]to Return"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }
}
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
        menu.Append("\r\n[_+]"); // cursor on
        menu.Append("[12][D]CONNECT TCP\r\n\n");
        menu.Append("[c][l-]\r\n\n");
        menu.Append("[C]  HOST:[W]\r\n\n");
        menu.Append("[C]  PORT:[W]\r\n\n");
        menu.Append("[C]PARITY:[W]\r\n\n");
        menu.Append("[9][C]Press[W]Alt-C[C]to Connect\r\n\n");
        menu.Append("[9][C]Press[W]Escape[C]to Return");
        return menu.ToString();
    }
}
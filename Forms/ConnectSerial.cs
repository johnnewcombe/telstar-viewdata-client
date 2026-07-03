namespace TelstarClient.Forms;

using System.Collections.Generic;
using System.Text;
using Display;

public class ConnectSerial : FormBase
{

    public ConnectSerial(DisplayManager displayManager, Configuration.Connection connection) : base(displayManager,
        connection)
    {
        
        // create fields
        Fields = new List<Field>();
        Fields.Add(new Field("device", 6, 16, 20, string.Empty, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("baud", 8, 16, 20, string.Empty, FieldType.Numeric, true));

    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[9][D]CONNECT MODEM\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]SERIAL DEVICE:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]    BAUD RATE:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Escape[C]to Return"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }
}
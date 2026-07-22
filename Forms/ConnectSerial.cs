using System.IO.Ports;
using TelstarClient.Configuration;

namespace TelstarClient.Forms;

using System.Collections.Generic;
using System.Text;
using ViewdataDisplay;


public class ConnectSerial : FormBase
{
    public ConnectSerial(DisplayManager displayManager, IConnection connection) : base(displayManager,
        connection)
    {
        var device = string.Empty;
        var baudRate = 0;
        var parity = string.Empty;
        var init = string.Empty;

        if (connection is SerialConnection ser)
        {
            device = ser.Device;
            baudRate = ser.BaudRate;
            parity = ser.Parity ? "Y" : "N";
            init = ser.InitString;
        }

        // create fields
        Fields = new List<Field>();
        Fields.Add(new Field("device", 6, 16, 20, device, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("baud", 8, 16, 20, baudRate.ToString(), FieldType.Numeric, true));
        Fields.Add(new Field("parity", 10, 16, 1, parity, FieldType.Bool, true));
        Fields.Add(new Field("init", 13, 1, 38, init, FieldType.AlphaNumeric, false));
    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[12][D]CONNECT MODEM\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]SERIAL DEVICE:[W][PLACEHOLDER]\r\n\n"))
            .Replace("[PLACEHOLDER]", Fields[0].Value);
        menu.Append(Converters.ConvertFromMarkup("[C]    BAUD RATE:[W][PLACEHOLDER]\r\n\n"))
            .Replace("[PLACEHOLDER]", Fields[1].Value);
        menu.Append(Converters.ConvertFromMarkup("[C]       PARITY:[W][PLACEHOLDER]\r\n\n"))
            .Replace("[PLACEHOLDER]", Fields[2].Value);
        menu.Append(Converters.ConvertFromMarkup("[C]INIT STRING:\r\n"));
        menu.Append(Converters.ConvertFromMarkup("[W][PLACEHOLDER]\r\n\n"))
            .Replace("[PLACEHOLDER]", Fields[3].Value);
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Alt-C[C]to Connect\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Escape[C]to Return"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }
}
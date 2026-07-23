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
        Fields.Add(new Field("init", 14, 2, 36, init, FieldType.AlphaNumeric, false));
    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append("\r\n[_+]"); // cursor on
        menu.Append("[12][D]CONNECT MODEM\r\n\n");
        menu.Append("[c][l-]\r\n\n");
        menu.Append("[C]SERIAL DEVICE:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[0].Value);
        menu.Append("[C]    BAUD RATE:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[1].Value);
        menu.Append("[C]       PARITY:[W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[2].Value);
        menu.Append("[C]INIT STRING:\r\n\n");
        menu.Append(" [W][PLACEHOLDER]\r\n\n")
            .Replace("[PLACEHOLDER]", Fields[3].Value);
        menu.Append("[c][l-]\r\n\n");
        menu.Append("[9][C]Press[W]Alt-C[C]to Connect\r\n\n");
        menu.Append("[9][C]Press[W]Escape[C]to Return");
        return menu.ToString();
    }
}
namespace TelstarClient.Forms;

using System.Collections.Generic;
using System.Text;
using Display;

public class Connect : FormBase
{

    public Connect(DisplayManager displayManager, Configuration.Connection connection) : base(displayManager,
        connection)
    {
        
        // create fields
        Fields = new List<Field>();
        Fields.Add(new Field("host", 6, 7, 20, string.Empty, FieldType.AlphaNumeric, true));
        Fields.Add(new Field("port", 8, 7, 20, string.Empty, FieldType.Numeric, true));

    }

    public override string ToString()
    {
        var menu = new StringBuilder();
        menu.Append(Converters.ConvertFromMarkup("\r\n[_+]")); // cursor on
        menu.Append(Converters.ConvertFromMarkup("[14][D]CONNECT\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]HOST:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[C]PORT:[W]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9][C]Press[W]Escape[C]to Return"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }
}
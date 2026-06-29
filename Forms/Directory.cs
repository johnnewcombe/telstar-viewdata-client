using System.Collections.Generic;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Directory : FormBase {

    public Directory(DisplayManager displayManager, Configuration.Connection connection) : base(displayManager,
        connection)
    {
        
    }
    
    public override string ToString() {

        var menu = new StringBuilder();
        //menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
        menu.Append(Converters.ConvertFromMarkup("\r\n[_-]")); // cursor off
        menu.Append(Converters.ConvertFromMarkup("[14][C][D]DIRECTORY\r\n\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[2][C]DIR NAME\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[PLACEHOLDER]"));
        menu.Append(Converters.ConvertFromMarkup("\r\n\n[4][W]Select 0 for Manual Dialling"));
        menu.Append(Converters.ConvertFromMarkup("\r\n\n[7][W]Select Alt 1-9 to Edit"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n\n[0][M]Alt 1-9 to Edit"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n[0][W]Alt-H for to view Help"));
        return menu.ToString();
    }

    public List<Field> Fields { get; set; }


}
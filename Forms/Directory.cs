using System.Text;
using ViewdataDisplay;

namespace TelstarClient.Forms;

public class Directory : FormBase {

    public Directory(DisplayManager displayManager, Configuration.IConnection connection) : base(displayManager,
        connection)
    {
        
    }
    
    public override string ToString() {

        var menu = new StringBuilder();
        //menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
        menu.Append(Converters.ConvertFromMarkup("\r\n[_-]")); // cursor off
        menu.Append(Converters.ConvertFromMarkup("[14][W][D]DIRECTORY\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n"));
        menu.Append(Converters.ConvertFromMarkup("[PLACEHOLDER]"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[8]0[C]for Manual Connection\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[8]S[C]for Serial Connection\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[4]Alt 1-9[C]to Edit  [W]Alt-H[C]for Help"));
        return menu.ToString();
    }

    //public List<Field> Fields { get; set; }


}
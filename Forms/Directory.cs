using System.Collections.Generic;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Directory : IForm {
    public string ToString() {

        var menu = new StringBuilder();
        //menu.Append(Converters.ConvertFromMarkup("0123456789012345678901234567890123456789"));
        menu.Append("\r\n");
        menu.Append(Converters.ConvertFromMarkup("[14][C][D]DIRECTORY\r\n\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[2][C]DIR NAME\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[PLACEHOLDER]"));
        menu.Append(Converters.ConvertFromMarkup("\r\n\n[4][C]Select '0' for Manual Dialling"));
        menu.Append(Converters.ConvertFromMarkup("\r\n\n[7][W]Alt-H for to view Help"));
        return menu.ToString();
    }

    public List<Field> Fields { get; set; }

    public Field GetCurrentField() {
        throw new System.NotImplementedException();
    }

    public bool Next() {
        throw new System.NotImplementedException();
    }
}
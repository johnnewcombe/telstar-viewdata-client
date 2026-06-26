using System.Collections.Generic;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Help : IForm {

    public string ToString() {

        var menu = new StringBuilder();

        menu.Append("\r\n");
        menu.Append(Converters.ConvertFromMarkup("[17][D]HELP\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt C[C]Conceal\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt R[C]Reveal\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt H[C]Help\r\n\n"));
        //menu.Append(Converters.ConvertFromMarkup(""));
        //menu.Append(Converters.ConvertFromMarkup(""));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt X[C]Disconnect\r\n\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9]Press any key to Return\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[3]Version 0.1 (c) John Newcombe 2025\r\n\n"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }

    public List<Field> Fields { get; set; }

    public Field GetCurrentField() {
        throw new System.NotImplementedException();
    }

    public bool Next() {
        throw new System.NotImplementedException();
    }
    public bool Previous() {
        throw new System.NotImplementedException();
    }
}
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using TelstarClient.Display;

namespace TelstarClient.Forms;

public class Help : FormBase {
    
    public Help(DisplayManager displayManager, Configuration.Connection connection):base(displayManager, connection)
    {
    }
    
    public override string ToString() {

        // the version is set in the file VERSION and is baked in during the build using make
        // it is also picked up from the file VERSION by the .csproj file but only if a full
        // build is done 
        var version = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;
        
        var menu = new StringBuilder();

        menu.Append("\r\n");
        menu.Append(Converters.ConvertFromMarkup("[17][D]HELP\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt C[C]Conceal\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt R[C]Reveal\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt H[C]Help\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt X[C]Disconnect\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[M]Alt Q[C]Quit\r\n\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9]Press any key to Return\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[13]Version " + version + "\r\n"));
        menu.Append(Converters.ConvertFromMarkup("[9](c) John Newcombe 2026\r\n\n"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }

}
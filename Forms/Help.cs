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
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt C[C]Conceal     "));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt R[C]Reveal\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt H[C]Help        "));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt F[C]Full Screen\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt X[C]Disconnect  "));
        menu.Append(Converters.ConvertFromMarkup("[W]Alt Q[C]Quit\r\n\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[9]Press any key to Return\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[c][l-]\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[13]Version " + version + "\r\n\n"));
        menu.Append(Converters.ConvertFromMarkup("[8][C](c) John Newcombe 2026\r\n\n"));
        //menu.Append(Converters.ConvertFromMarkup("\r\n0123456789012345678901234567890123456789"));
        return menu.ToString();
    }

}
using System.Reflection;
using System.Text;
using ViewdataDisplay;


namespace TelstarClient.Forms;

public class Help : FormBase {
    
    public Help(DisplayManager displayManager, Configuration.IConnection connection):base(displayManager, connection)
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
        menu.Append("[17][D]HELP\r\n\n");
        menu.Append("[c][l-]\r\n");
        menu.Append(" [W]Alt C[C]Conceal     ");
        menu.Append(" [W]Alt R[C]Reveal\r\n");
        menu.Append(" [W]Alt H[C]Help        ");
        menu.Append(" [W]Alt F[C]Full Screen\r\n");
        menu.Append(" [W]Alt X[C]Disconnect  ");
        menu.Append(" [W]Alt Q[C]Quit\r\n\n");
        menu.Append("[9]Press escape to Return\r\n");
        menu.Append("[c][l-]\r\n");
        menu.Append(" [W]*<page>_[C]Load a Specific Page\r\n");
        menu.Append(" [W]*_      [C]Load Previous Frame\r\n");
        menu.Append(" [W]*00     [C]Refresh Current Frame\r\n");
        menu.Append(" [W]*09     [C]Update Current Frame\r\n");
        menu.Append(" [W]__      [C]Correct Keying Error\r\n");
        menu.Append(" [W]_       [C]Load Continuation Frame\r\n");
        menu.Append("[c][l-]\r\n");
        menu.Append("[13]Version " + version + "\r\n");
        menu.Append("[8][C](c) John Newcombe 2026\r\n\n");
        return menu.ToString();
    }

}
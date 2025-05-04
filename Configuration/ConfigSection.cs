using System.Collections.Generic;

namespace TelstarClient.Configuration;

public class ConfigSections {

    public List<Conection> Connections { get; set; } = new List<Conection>();

    public ConfigSections() {
        Connections = new List<Conection>();
        for (var i = 0; i < 9; i++) {
            Connections.Add(new Conection());
        }
    }
}

public class Conection {

    public string Name { get; set; }
    public string Address { get; set; }
    public int Port { get; set; }
}
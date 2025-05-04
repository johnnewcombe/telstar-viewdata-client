using System.Collections.Generic;

namespace TelstarClient.Configuration;

public class ConfigSections() {
        public List<ConfigSection> tcp { get; set; }
}
public class ConfigSection {
        
        public string Name { get; set; }
        public string Address { get; set; }
        public int Port { get; set; }
}

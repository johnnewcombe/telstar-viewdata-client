using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace TelstarClient.Configuration;

public class Settings {

    private ConfigSections config;
    
    public  Settings(string jsonFilename) {
        
        var jsonString = File.ReadAllText(jsonFilename);
        config = JsonSerializer.Deserialize<ConfigSections>(jsonString)!;

        foreach (var section in config.Connections) {
            Trace.WriteLine($"Name: {section.Name}");
            Trace.WriteLine($"Address: {section.Address}");
            Trace.WriteLine($"Port: {section.Port}");
        }
    }

    public void Save(string jsonFilename) {
        var jsonString = JsonSerializer.Serialize(config);
    }
}

// output:
//Date: 8/1/2019 12:00:00 AM -07:00
//TemperatureCelsius: 25
//Summary: Hot
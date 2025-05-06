using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Avalonia.Platform;
using Tmds.DBus.Protocol;

namespace TelstarClient.Configuration;

public class Settings {
    
    public ConfigSections config;

    public Settings(string jsonFilename) {

        string jsonString;
        
        // read the config from the application support file if it exists
        if (File.Exists(jsonFilename)) {
            jsonString = File.ReadAllText(jsonFilename);
            config = JsonSerializer.Deserialize<ConfigSections>(jsonString)!;
        }
        // config doesn't exist so load the default from Assets and save
        else {
            var fs = new StreamReader(AssetLoader
                .Open(new Uri($"avares://{AppDomain.CurrentDomain.FriendlyName}/Assets/defaultConfig.json")));
            jsonString = fs.ReadToEnd();
            File.WriteAllText(jsonFilename, jsonString);
        }
        config = JsonSerializer.Deserialize<ConfigSections>(jsonString)!;
    }

    public void Save(string jsonFilename) {
        var jsonString = JsonSerializer.Serialize(config);
        File.WriteAllText(jsonFilename, jsonString);
    }
}

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
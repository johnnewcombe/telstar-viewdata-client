/*
    Copyright (c) 2025 John Newcombe
   
    This file is part of the Software known as GlassTTY Viewdata Client.

    GlassTTY Viewdata Client is free software: you can redistribute
    it and/or modify it under the terms of the GNU General Public
    License as published by the Free Software Foundation, either
    version 3 of the License, or (at your option) any later version.
    GlassTTY Viewdata Client is distributed in the hope that it will
    be useful, but WITHOUT ANY WARRANTY; without even the implied
    warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
    See the GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with Foobar. If not, see <https://www.gnu.org/licenses/>.

*/

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
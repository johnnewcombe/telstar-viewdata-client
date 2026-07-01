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
    along with the product. If not, see <https://www.gnu.org/licenses/>.

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
    
    public ConfigSections Config;
    private string _jsonFilename;
    
    public Settings(string jsonFilename) {

        _jsonFilename = jsonFilename;
        string jsonString;
        
        // read the config from the application support file if it exists
        if (File.Exists(jsonFilename)) {
            jsonString = File.ReadAllText(jsonFilename);
            Config = JsonSerializer.Deserialize<ConfigSections>(jsonString)!;
        }
        // config doesn't exist so load the default from Assets and save
        else {
            var fs = new StreamReader(AssetLoader
                .Open(new Uri($"avares://{AppDomain.CurrentDomain.FriendlyName}/Assets/defaultConfig.json")));
            jsonString = fs.ReadToEnd();
            File.WriteAllText(jsonFilename, jsonString);
        }
        Config = JsonSerializer.Deserialize<ConfigSections>(jsonString)!;
    }

    public void Save() {
        var jsonString = JsonSerializer.Serialize(Config);
        File.WriteAllText(_jsonFilename, jsonString);
    }
}

public class ConfigSections {

    public List<Connection> Connections { get; set; } = new List<Connection>();

    public ConfigSections() {
        Connections = new List<Connection>();
        for (var i = 0; i < 9; i++) {
            Connections.Add(new Connection());
        }
    }
}

public class Connection {

    public string Name { get; set; }
    public string Host { get; set; }
    public int Port { get; set; }
}
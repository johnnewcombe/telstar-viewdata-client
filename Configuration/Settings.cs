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
using System.IO;
using System.Text.Json;
using Avalonia.Platform;

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

// TODO find a way for the Connection to work for both Tcp and Serial as this is
//  passed into connectTcp and ConnectSerial and is used to populate the Connection fields.
//  Ber in mind that this object is purely a data object used for seiings.
//  could we have...
//   List of args?
//   A dictionary of fields?
//   Additional Fields for Serial

public class ConfigSections {

    public List<TcpConnection> Connections { get; set; }
    public SerialConnection SerialConnection { get; set; } = new SerialConnection();

    public ConfigSections() {
        Connections = new List<TcpConnection>();
        for (var i = 0; i < 9; i++) {
            Connections.Add(new TcpConnection());
        }
        
    }
}

public class TcpConnection : IConnection{

    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool Parity { get; set; } = false;
    public string InitString { get; set; } = "";

}

public class SerialConnection : IConnection
{
    public string Device { get; set; } = "";
    public int BaudRate { get; set; } = 9600;
    public bool Parity { get; set; } = false;
    public string InitString { get; set; }
}

public interface IConnection
{
    bool Parity { get; set; }
    string InitString { get; set; }
}
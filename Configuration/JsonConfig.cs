using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Tmds.DBus.Protocol;

namespace TelstarClient.Configuration;

public class JsonConfig {

    private IConfigurationRoot root;
    public  JsonConfig(string jsonFilename) {
        root = new ConfigurationBuilder()
            .AddJsonFile(jsonFilename)
            .Build();
        
        //var ConfigSection = builder.GetRequiredSection("AppSettings").Get<ConfigSection>();
    }

    public ConfigSection GetConnection(string key) {
        var config = new ConfigSection();
        
        var connectionRoot = root.GetSection("connections");
        var connection = connectionRoot.GetSection(key);
        
        config.Name = (string)connection["name"];
        config.Address = connection["tcp:address"];
        
        if (!int.TryParse(connection["tcp:port"], out var port)) {
            port = 6502;
        }
        config.Port = port;
        return config;
    }
}

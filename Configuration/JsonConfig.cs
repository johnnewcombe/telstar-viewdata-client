using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace TelstarClient.Configuration;

public class JsonConfig {

    public IConfigurationRoot Load(string jsonFilename) {
        return new ConfigurationBuilder()
            .AddJsonFile(jsonFilename)
            .Build();
    }
}
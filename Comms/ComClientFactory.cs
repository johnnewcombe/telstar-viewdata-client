using System;
using Microsoft.Extensions.Logging;

namespace TelstarClient.Comms;

public class CommsClientFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public CommsClientFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public ICommsClient Create(CommsClientType type)
    {
        return type switch
        {
            CommsClientType.Tcp => new TcpClient(_loggerFactory.CreateLogger<TcpClient>()),
            CommsClientType.Serial => new SerialClient(_loggerFactory.CreateLogger<SerialClient>()),
            _ => throw new ArgumentOutOfRangeException(nameof(type))
        };
    }
}

public enum CommsClientType { Tcp, Serial }
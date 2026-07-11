using System;
using Microsoft.Extensions.Logging;

namespace TelstarClient.Comms;

/// <summary>
/// Factory class for creating communication clients.
/// </summary>
public class CommsClientFactory
{
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="CommsClientFactory"/> class.
    /// </summary>
    /// <param name="loggerFactory">The logger factory.</param>
    public CommsClientFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates a communication client of the specified type.
    /// </summary>
    /// <param name="type">The type of communication client to create.</param>
    /// <returns>An instance of <see cref="ICommsClient"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the type is not recognized.</exception>
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

/// <summary>
/// Defines the types of communication clients.
/// </summary>
public enum CommsClientType { Tcp, Serial }
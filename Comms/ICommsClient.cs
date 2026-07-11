using System.IO.Ports;

namespace TelstarClient.Comms;

public delegate void DataReceivedEventHandler(string data);
public delegate void OnConnectEventHandler(bool status);

/// <summary>
/// Defines the interface for a communication client, providing methods to handle data transmission and connection management.
/// </summary>
public interface ICommsClient
{
    /// <summary>
    /// Event raised when data is received from the communication source.
    /// </summary>
    event DataReceivedEventHandler OnDataReceivedEvent;
    
    /// <summary>
    /// Event raised when the connection status changes.
    /// </summary>
    event OnConnectEventHandler OnConnectEvent;

    /// <summary>
    /// Establishes a connection to the specified target.
    /// </summary>
    /// <param name="arg1">The connection argument (e.g., host or port).</param>
    /// <param name="arg2">The secondary connection argument (e.g., port number or baud rate).</param>
    /// <param name="arg3">A flag indicating a specific connection setting.</param>
    void Connect(string arg1, int arg2, bool arg3);
    
    /// <summary>
    /// Checks if the client is currently connected.
    /// </summary>
    /// <returns>True if connected, false otherwise.</returns>
    bool IsConnected();
    
    /// <summary>
    /// Writes a string to the connection.
    /// </summary>
    /// <param name="data">The string to write.</param>
    /// <returns>True if the write operation was successful.</returns>
    bool Write(string data);
    
    /// <summary>
    /// Writes a single byte to the connection.
    /// </summary>
    /// <param name="data">The byte to write.</param>
    /// <returns>True if the write operation was successful.</returns>
    bool Write(byte data);
    
    /// <summary>
    /// Writes a single character to the connection.
    /// </summary>
    /// <param name="data">The character to write.</param>
    /// <returns>True if the write operation was successful.</returns>
    bool Write(char data);
    
    /// <summary>
    /// Writes an array of bytes to the connection.
    /// </summary>
    /// <param name="data">The byte array to write.</param>
    /// <returns>True if the write operation was successful.</returns>
    bool Write(byte[] data);
    
    /// <summary>
    /// Disconnects from the current source.
    /// </summary>
    void Disconnect();
    
    /// <summary>
    /// Disposes of the client and releases resources.
    /// </summary>
    void Dispose();
}

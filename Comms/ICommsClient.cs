using System.IO.Ports;

namespace TelstarClient.Comms;

public delegate void DataReceivedEventHandler(string data);
public delegate void OnConnectEventHandler(bool status);

public interface ICommsClient
{
    event DataReceivedEventHandler OnDataReceivedEvent;
    event OnConnectEventHandler OnConnectEvent;

    Parity Parity { get; set; }

    void Connect(string arg1, int arg2);
    bool IsConnected();
    bool Write(string data);
    bool Write(byte data);
    bool Write(char data);
    bool Write(byte[] data);
    void Disconnect();
    void Dispose();
}

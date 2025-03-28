using System;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text;
using System.Threading;

namespace AvaloniaApplication1.Comms;

public class NetClient
{
    private IPAddress _hostIPAddress;
    private IPEndPoint _hostIPEndPoint;
    
    public NetClient()
    {
    }
    
    /*
     Read and write operations can be performed simultaneously on an instance 
     of the NetworkStream class without the need for synchronization. 
     As long as there is one unique thread for the write operations and one 
     unique thread for the read operations, there will be no cross-interference 
     between read and write threads and no synchronization is required.
     */
    private Socket socket;
    private TcpClient client;
    
    public void Disconnect()
    {
        try
        {
            socket.Close();
        }
        catch (Exception ex)
        {
            Debug.Print(ex.Message);
        }
    }
    async public void Connect(string ipAddress, int port)
    {
        _hostIPAddress = (Dns.Resolve(ipAddress)).AddressList[0];
        _hostIPEndPoint = new IPEndPoint(_hostIPAddress,port);
        
        client = new TcpClient();
        await client.ConnectAsync(_hostIPEndPoint);
        await using NetworkStream stream = client.GetStream();
        
        var buffer = new byte[1_024];
        var received = 0;

        while (true)
        {
            try
            {
                received = await stream.ReadAsync(buffer);
            }
            catch (Exception ex)
            {
                Debug.Print(ex.Message);
                break;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, received);
            Debug.Print($"Message received: \"{message}\" \n");
        }
        client.Close();
        Debug.Print("Connection closed.");
    }
}
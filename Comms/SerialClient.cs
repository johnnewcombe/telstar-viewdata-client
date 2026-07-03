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


namespace TelstarClient.Comms;

public class SerialClient : ICommsClient
{
    public event DataReceivedEventHandler OnDataReceivedEvent;
    public event OnConnectEventHandler OnConnectEvent;

    public void Connect(string connectionString, int port)
    {
        // Placeholder implementation
        OnConnectEvent?.Invoke(true);
    }

    public bool IsConnected()
    {
        return false;
    }

    public bool Write(string data)
    {
        return true;
    }

    public bool Write(byte data)
    {
        return true;
    }

    public bool Write(char data)
    {
        return true;
    }

    public bool Write(byte[] data)
    {
        return true;
    }

    public void Disconnect()
    {
        OnConnectEvent?.Invoke(false);
    }
}
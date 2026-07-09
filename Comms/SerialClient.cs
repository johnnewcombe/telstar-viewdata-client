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
using System.IO.Ports;
using Microsoft.Extensions.Logging;

namespace TelstarClient.Comms;

public class SerialClient : ICommsClient
{
    private bool _disposed;
    
    private SerialPort _serialPort;

    public Parity Parity { get; set; } = Parity.None;
    
    private readonly ILogger<SerialClient> _logger;
    public event DataReceivedEventHandler OnDataReceivedEvent;
    public event OnConnectEventHandler OnConnectEvent;

    public SerialClient(ILogger<SerialClient> logger)
    { 
        _logger = logger;
    }
    
    public void Connect(string deviceName, int baudRate)
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }

            _serialPort = new SerialPort(deviceName, baudRate);
            _serialPort.Parity = this.Parity;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            OnConnectEvent?.Invoke(true);
        }
        catch (Exception ex)
        {
            OnConnectEvent?.Invoke(false);
            _logger.LogError("Error connecting to Serial device:{Error}", ex.Message);

        }
    }

    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        if (OnDataReceivedEvent != null)
        {
            // Read available data
            string data = _serialPort.ReadExisting();
            OnDataReceivedEvent.Invoke(data);
        }
    }

    public bool IsConnected()
    {
        return _serialPort != null && _serialPort.IsOpen;
    }

    public bool Write(string data)
    {
        try
        {
            if (IsConnected())
            {
                _serialPort.Write(data);
                return true;
            }
        }
        catch (Exception)
        {
            _logger.LogError("Error writing to device:{Data}", data);
            Dispose();
            throw;
        }
        
        return false;
    }

    public bool Write(byte data)
    {
        if (IsConnected())
        {
            _serialPort.Write(new[] { data }, 0, 1);
            return true;
        }
        return false;
    }

    public bool Write(char data)
    {
        if (IsConnected())
        {
            try
            {
                _serialPort.Write(data.ToString());
                return true;
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }
        
        return false;
    }

    public bool Write(byte[] data)
    {
        if (IsConnected())
        {
            _serialPort.Write(data, 0, data.Length);
            return true;
        }
        return false;
    }

    public void Disconnect()
    {
        if (_serialPort != null)
        {
            _serialPort.DataReceived -= SerialPort_DataReceived;
            if (_serialPort.IsOpen)
            {
                _serialPort.Close();
            }
            _serialPort.Dispose();
            _serialPort = null;
            OnConnectEvent?.Invoke(false);
        }

        _logger.LogInformation("Disconnecting");
        Dispose();
    }

    public void Dispose()
    {
        _logger.LogInformation("Connection closing");
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_serialPort != null)
                {
                    _serialPort.Close();
                    _serialPort.Dispose();
                    _serialPort = null;
                    OnConnectEvent?.Invoke(false);
                }
            }

            _disposed = true;
        }
    }
}
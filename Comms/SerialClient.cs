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
using System.Text;
using Microsoft.Extensions.Logging;

namespace TelstarClient.Comms;

public class SerialClient : ICommsClient
{

    private bool _disposed;
    private readonly StringBuilder _receiveBuffer = new();
    
    private SerialPort _serialPort;

    public Parity Parity { get; set; } = Parity.None;
    
    private readonly ILogger<SerialClient> _logger;
    public event DataReceivedEventHandler OnDataReceivedEvent;
    public event Action<bool, string?> OnConnectEvent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SerialClient"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public SerialClient(ILogger<SerialClient> logger)
    { 
        _logger = logger;
    }
    
    /// <summary>
    /// Connects to the serial device.
    /// </summary>
    /// <param name="deviceName">The name of the serial device (e.g., COM1).</param>
    /// <param name="baudRate">The baud rate.</param>
    /// <param name="parity">A flag indicating if parity should be used.</param>
    public void Connect(string deviceName, int baudRate, bool parity)
    {
        try
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
                _serialPort.Dispose();
            }
            _logger.LogInformation("Connecting to Device:{arg1}, Daud:{arg2}, Parity:{Parity}", deviceName, baudRate, Parity);

            _serialPort = new SerialPort(deviceName, baudRate);
            _serialPort.Parity = parity ? Parity.Even : Parity.None;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            OnConnectEvent?.Invoke(true,"");
        }
        catch (Exception ex)
        {
            OnConnectEvent?.Invoke(false,"");
            _logger.LogError("Error connecting to Serial device:{Error}", ex.Message);
            OnConnectEvent?.Invoke(false, CommsConstants.UNABLE_TO_CONNECT);
        }
    }

    /// <summary>
    /// Handles the DataReceived event from the serial port.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>


    private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
    {
        string data = _serialPort.ReadExisting();

        _receiveBuffer.Append(data);
        OnDataReceivedEvent?.Invoke(data);

        if (_receiveBuffer.ToString().Contains(CommsConstants.NoCarrierText))
        {
            Disconnect();
            _receiveBuffer.Clear();
        }
    }

    public bool IsConnected()
    {
        return _serialPort != null && _serialPort.IsOpen;
    }

    /// <summary>
    /// Write Data to Port
    /// </summary>
    /// <param name="data">Data to be written</param>
    /// <returns>Success status as Boolean Value</returns>
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
            return false;
        }
        
        return false;
    }
    
    /// <summary>
    /// Write Data to Port
    /// </summary>
    /// <param name="data">Data to be written</param>
    /// <returns>Success status as Boolean Value</returns>
    public bool Write(byte data)
    {
        byte[] array = [data];
        return Write(array);
    }

    /// <summary>
    /// Write Data to Port
    /// </summary>
    /// <param name="data">Data to be written</param>
    /// <returns>Success status as Boolean Value</returns>
    public bool Write(char data)
    {
        byte[] array = [(byte)data];
        return Write(array);
    }
    
    
    /// <summary>
    /// Write Data to Port
    /// </summary>
    /// <param name="data">Data to be written</param>
    /// <returns>Success status as Boolean Value</returns>
    public bool Write(byte[] data)
    {
        if (IsConnected())
        {
            try
            {
                
            _serialPort.Write(data, 0, data.Length);
            return true;
            }
            catch (Exception)
            {
                _logger.LogError("Error writing to device:{Data}", data);
                Dispose();
                return false; 
            }
        }
        return false;
    }

    /// <summary>
    /// Disconnects from the serial device.
    /// </summary>
    public void Disconnect()
    {
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
                    if (_serialPort.IsOpen)
                    {
                        _serialPort.Close();
                    }
                    _serialPort.Dispose();
                    _serialPort = null;
                    _receiveBuffer.Clear();
                    OnConnectEvent?.Invoke(false,"");
                }
            }

            _disposed = true;
        }
    }
}
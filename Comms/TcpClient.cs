using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;
using System.Threading.Tasks;

/*
   MIT License

   Copyright (c) 2019 Softink Lab

   Permission is hereby granted, free of charge, to any person obtaining a copy
   of this software and associated documentation files (the "Software"), to deal
   in the Software without restriction, including without limitation the rights
   to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
   copies of the Software, and to permit persons to whom the Software is
   furnished to do so, subject to the following conditions:

   The above copyright notice and this permission notice shall be included in all
   copies or substantial portions of the Software.

   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.

 */

namespace TelstarClient.Comms
{
    public class TcpClient : ICommsClient, IDisposable
    {
        private bool _disposed;
        // *** Event Handlers *** //

        #region Delegates

        public event DataReceivedEventHandler OnDataReceivedEvent;
        public event OnConnectEventHandler OnConnectEvent;

        #endregion

        #region Properties

        public Parity Parity
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        // Connection Parameters
        private IPAddress _ipAddress;
        private int _port;


        // Socket Parameters
        private System.Net.Sockets.TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly byte[] _readerBuffer = new byte[1024];

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a TCP asynchronous client. This client is connect to the server and port with passed parameters.
        /// </summary>
        public TcpClient()
        {
        }

        public IPAddress IpAddrss => _ipAddress;

        public int Port
        {
            get { return _port; }
        }

        /// <summary>
        /// Connect to the server
        /// </summary>
        /// <param name="ip">Server IP</param>
        /// <param name="port">Server Port</param>
        public void Connect(string ip, int port)
        {
            Task.Run(async () => await ConnectAsync(ip, port));
        }

        private async Task ConnectAsync(string ip, int port)
        {
            try
            {
                if (IPAddress.TryParse(ip, out IPAddress parsedAddress))
                {
                    _ipAddress = parsedAddress;
                }
                else
                {
                    var addresses = await Dns.GetHostAddressesAsync(ip);
                    _ipAddress = addresses[0];
                }

                // Close the client if open
                if (_tcpClient != null)
                {
                    _stream?.Dispose();
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                    _tcpClient = null;
                }

                // Create the client object
                _tcpClient = new System.Net.Sockets.TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, port);
                _stream = _tcpClient.GetStream();
                _port = port;

                OnConnectEvent?.Invoke(true);

                // Start receiving
                _ = ReceiveLoopAsync();
            }
            catch (Exception)
            {
                OnConnectEvent?.Invoke(false);
                throw;
            }
        }

        /// <summary>
        /// Check connection status of the socket
        /// </summary>
        /// <returns>True or False based on status</returns>
        public bool IsConnected()
        {
            return _tcpClient?.Client?.Connected ?? false;
        }

        /// <summary>
        /// Write Data to Socket
        /// </summary>
        /// <param name="data">Data to be written</param>
        /// <returns>Success status as Boolean Value</returns>
        public bool Write(string data)
        {
            if (IsConnected())
            {
                try
                {
                    byte[] byteDateLine = Encoding.ASCII.GetBytes(data);
                    _stream.Write(byteDateLine, 0, byteDateLine.Length);
                    return true;
                }
                catch (Exception)
                {
                    Dispose();
                    throw;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Write Data to Socket
        /// </summary>
        /// <param name="data">Data to be written</param>
        /// <returns>Success status as Boolean Value</returns>
        public bool Write(byte data)
        {
            byte[] array = [data];
            return Write(array);
        }

        /// <summary>
        /// Write Data to Socket
        /// </summary>
        /// <param name="data">Data to be written</param>
        /// <returns>Success status as Boolean Value</returns>
        public bool Write(char data)
        {
            byte[] array = [(byte)data];
            return Write(array);
        }

        /// <summary>
        /// Write Data to Socket
        /// </summary>
        /// <param name="data">Data to be written</param>
        /// <returns>Success status as Boolean Value</returns>
        public bool Write(byte[] data)
        {
            if (IsConnected())
            {
                try
                {
                    _stream.Write(data, 0, data.Length);
                    return true;
                }
                catch (Exception)
                {
                    Dispose();
                    throw;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Close the Socket Connection
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_tcpClient != null)
                    {
                        _stream?.Dispose();
                        _tcpClient.Close();
                        _tcpClient.Dispose();
                        _tcpClient = null;
                        OnConnectEvent?.Invoke(false);
                    }
                }
                _disposed = true;
            }
        }

        /// <summary>
        /// Disconnect the socket
        /// </summary>
        public void Disconnect()
        {
            _ipAddress = null;
            _port = 0;
            Dispose();
        }

        #endregion

        #region Private Methods

        private async Task ReceiveLoopAsync()
        {
            try
            {
                while (_tcpClient != null && _tcpClient.Client != null && _tcpClient.Client.Connected && _stream != null)
                {
                    int nBytesRec = await _stream.ReadAsync(_readerBuffer, 0, _readerBuffer.Length);
                    if (nBytesRec > 0)
                    {
                        string sRecieved = Encoding.ASCII.GetString(_readerBuffer, 0, nBytesRec);
                        OnDataReceivedEvent?.Invoke(sRecieved);
                    }
                    else
                    {
                        // Connection closed
                        Dispose();
                        break;
                    }
                }
            }
            catch (Exception)
            {
                Dispose();
            }
        }

        #endregion
    }
}
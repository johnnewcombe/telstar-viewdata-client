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

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TelstarClient.Comms
{
    public class TcpClient : ICommsClient, IDisposable
    {
        private bool _disposed;

        // *** Event Handlers *** //
        private readonly ILogger<TcpClient> _logger;

        #region Delegates

        public event DataReceivedEventHandler OnDataReceivedEvent;
        public event OnConnectEventHandler OnConnectEvent;

        #endregion

        #region Properties
        
        // Connection Parameters
        private IPAddress _ipAddress;
        private int _port;
        private bool _parity;
        
        // Socket Parameters
        private System.Net.Sockets.TcpClient _tcpClient;
        private NetworkStream _stream;
        private readonly byte[] _readerBuffer = new byte[1024];

        #endregion

        #region Public Methods

        /// <summary>
        /// Create a TCP asynchronous client. This client is connect to the server and port with passed parameters.
        /// </summary>
        public TcpClient(ILogger<TcpClient> logger)
        {
            _logger = logger;
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
        /// <param name="parity"></param>
        public void Connect(string ip, int port, bool parity)
        {
            _parity = parity;
            Task.Run(async () => await ConnectAsync(ip, port, parity));
        }

        private async Task ConnectAsync(string ip, int port, bool parity)
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

                _logger.LogInformation("Connecting to Host:{arg1}, Port:{arg2}, Parity:{arg3}", ip, port, parity);

                // Create the client object
                _tcpClient = new System.Net.Sockets.TcpClient();
                await _tcpClient.ConnectAsync(_ipAddress, port);
                _stream = _tcpClient.GetStream();
                _port = port;

                OnConnectEvent?.Invoke(true);

                // Start receiving
                _ = ReceiveLoopAsync();
            }
            catch (Exception ex)
            {
                OnConnectEvent?.Invoke(false);
                _logger.LogError("Error connecting to TCP server:{Error}", ex.Message
                );
                throw;
            }
        }

        /// <summary>
        /// Check connection status of the socket
        /// </summary>
        /// <returns>True or False based on status</returns>
        public bool IsConnected()
        {
            return _tcpClient?.Client.Connected ?? false;
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
                    byte[] byteDate = Encoding.ASCII.GetBytes(data);
                    return Write(byteDate);

                }
                catch (Exception)
                {
                    _logger.LogError("Error writing to socket:{Data}", data);
                    Dispose();
                    throw;
                }
            }

            return false;
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
                    if (_parity)
                    {
                        data=ApplyEvenParity(data);
                    }
                    
                    _stream.Write(data, 0, data.Length);
                    return true;
                }
                catch (Exception)
                {
                    _logger.LogError("Error writing to socket:{Data}", data);
                    Dispose();
                    throw;

                }
            }

            return false;
        }


        /// <summary>
        /// Disconnect the socket
        /// </summary>
        public void Disconnect()
        {
            _ipAddress = null;
            _port = 0;
            _logger.LogInformation("Disconnecting");
            Dispose();
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Applies even parity to the data.
        /// </summary>
        /// <param name="data">The byte array to process.</param>
        /// <returns>A new byte array with parity bits applied.</returns>
        public static byte[] ApplyEvenParity(byte[] data)
        {
            var result = new byte[data.Length];

            for (int i = 0; i < data.Length; i++)
            {
                byte b = (byte)(data[i] & 0x7F); // ensure only 7 bits of data
                int bitCount = CountSetBits(b);

                // if odd number of 1s, set the 8th bit to make it even
                if ((bitCount & 1) != 0)
                {
                    b |= 0x80;
                }

                result[i] = b;
            }

            return result;
        }
        /// <summary>
        /// Counts the number of set bits in a byte.
        /// </summary>
        /// <param name="b">The byte to check.</param>
        /// <returns>The number of set bits.</returns>
        private static int CountSetBits(byte b)
        {
            int count = 0;
            while (b != 0)
            {
                count += b & 1;
                b >>= 1;
            }
            return count;
        }
        
        /// <summary>
        /// Asynchronously receives data from the TCP stream.
        /// </summary>
        /// <returns>A task representing the asynchronous receive loop.</returns>
        private async Task ReceiveLoopAsync()
        {
            try
            {
                while (_tcpClient != null && _tcpClient.Client.Connected && _stream != null)
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
            catch (Exception ex)
            {
                _logger.LogError("Error in receive loop:{Error}", ex.Message);
                Dispose();
            }
        }

        #endregion

        /// <summary>
        /// Close the Socket Connection
        /// </summary>
        public void Dispose()
        {
            _logger.LogInformation("Connection closing");
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of the resources used by the TCP client.
        /// </summary>
        /// <param name="disposing">True if called from Dispose, false if called from finalizer.</param>
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
    }
}
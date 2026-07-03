using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

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
    public class TcpClient : ICommsClient
    {
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
        private Socket _socket;
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
            try
            {
                _ipAddress = (Dns.GetHostEntry(ip)).AddressList[0];

                // Close the socket if open
                if (_socket != null && _socket.Connected)
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    System.Threading.Thread.Sleep(10);
                    _socket.Close();
                }

                // Create the socket object
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                // Define the Server address and port
                IPEndPoint epServer = new IPEndPoint(_ipAddress, port);

                // Connect to server non-Blocking method
                _socket.Blocking = false;

                // create and pass a callback for when it connects.
                AsyncCallback onconnect = OnConnect;
                _socket.BeginConnect(epServer, onconnect, _socket);
            }
            catch (Exception)
            {
                OnConnectEvent(false);
                //throw new Exception($"Socket Connection Failed. Message : {ex}");
            }
        }

        /// <summary>
        /// Check connection status of the socket
        /// </summary>
        /// <returns>True or False based on status</returns>
        public bool IsConnected()
        {
            if (_socket != null)
            {
                return _socket.Connected;
            }

            return false;
        }

        /// <summary>
        /// Write Data to Socket
        /// </summary>
        /// <param name="data">Data to be written</param>
        /// <returns>Success status as Boolean Value</returns>
        public bool Write(String data)
        {
            // TODO: Implement Parity in code, based on the Parity Property
            
            // Check Connection
            if (IsConnected())
            {
                try
                {
                    Byte[] byteDateLine = Encoding.ASCII.GetBytes(data.ToCharArray());
                    _socket.Send(byteDateLine, byteDateLine.Length, 0);
                    return true;
                }
                catch (Exception ex)
                {
                    Dispose();
                    throw new Exception($"Data Writing Operation Failed: {ex}");
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
            // Check Connection
            if (IsConnected())
            {
                try
                {
                    _socket.Send(data, data.Length, 0);
                    return true;
                }
                catch (Exception)
                {
                    Dispose();
                    throw new Exception("Data Writing Operation Failed.");
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
            if (_socket != null && _socket.Connected)
            {
                OnConnectEvent(false);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        /// <summary>
        /// Disconnect the socket
        /// </summary>
        public void Disconnect()
        {
            _ipAddress = null;
            _port = 0;
            
            if (_socket != null && _socket.Connected)
            {
                OnConnectEvent(false);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        #endregion

        #region Private Methods

        // Setup Callbacks if Socket is Connected
        private void OnConnect(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            try
            {
                if (socket != null && socket.Connected)
                {
                    SetupRecieveCallback(socket);
                    OnConnectEvent(true);
                }
                else
                {
                    OnConnectEvent(false);
                }
            } 
            catch (Exception)
            {
                OnConnectEvent(false);
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
            }
        }

        // Setup Receive Callback for Async Listening
        private void SetupRecieveCallback(Socket socket)
        {
            try
            {
                AsyncCallback receiveData = OnReceivedData;
                socket.BeginReceive(_readerBuffer, 0, _readerBuffer.Length, SocketFlags.None, receiveData, socket);
            }
            catch (Exception ex)
            {
                Dispose();
                throw new Exception($"Receive Callback Setup Failed: {ex}");
            }
        }

        // Receive data from TCP
        private void OnReceivedData(IAsyncResult ar)
        {
            Socket socket = (Socket)ar.AsyncState;

            if (IsConnected())
            {
                try
                {
                    // Check data is available
                    if (socket != null)
                    {
                        int nBytesRec = socket.EndReceive(ar);
                        if (nBytesRec > 0)
                        {
                            string sRecieved = "";
                            for (int i = 0; i < nBytesRec; i++)
                            {
                                sRecieved += (char)_readerBuffer[i];
                            }

                            // Fire Data Recieved Event
                            OnDataReceivedEvent(sRecieved);

                            // If the Connection is Still Usable Re-establish the Callback
                            SetupRecieveCallback(socket);
                        }
                        else
                        {
                            Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Dispose();
                    throw new Exception($"Receive Operation Failed: {ex}");
                }
            }
        }

        #endregion
    }
}
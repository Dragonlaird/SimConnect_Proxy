using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

internal class ProxyConnector:IDisposable
{
    internal EventHandler<byte[]> DataReceived; // Event to notify consuming application of data received by this connector
    internal EventHandler<bool> Connected; // Event to notify consuming application when this connector accepts, establishes or drops a connection
    internal EventHandler<ProxyMessage> Notifications; // Event to notify consuming application of any progress messages or errors

    private ManualResetEvent connectDone = new ManualResetEvent(false); // Waits for connection to establish before allowing code to proceed
    private ManualResetEvent sendDone = new ManualResetEvent(false); // Waits for data to be sent before allowing allowing code to proceed

    private bool IsConnected = false; // Remember last connection status, so we can notify consuming application if this changes
    private IPEndPoint _socketEP = null; // EndPoint to connect to or listen on
    private Socket _socket; // Socket used for initiating connections
    private Socket _handler; // Worker Socket generated after establishing a connection
    private byte[] buffer = null; // A temporary buffer to hold any send data for sending, when a connection is established

    /// <summary>
    /// Start listening to EndPoint for incoming connection request
    /// </summary>
    /// <param name="ep">EndPoint to listen on</param>
    internal void Listen(EndPoint ep)
    {
        _socketEP = (IPEndPoint)ep;
        StartListener();
    }

    /// <summary>
    /// Start listening to Local Address/Port for incoming connection request
    /// </summary>
    /// <param name="address">Name or IP Address of this computer to listen on</param>
    /// <param name="port">Port Number to listen on</param>
    internal void Listen(string address, int port)
    {
        var ipAddress = Dns.GetHostAddresses(address).First();
        Listen(new IPEndPoint(ipAddress, port));
    }

    /// <summary>
    /// Connect to Remote EndPoint and wait to send data
    /// </summary>
    /// <param name="ep">EndPoint to connect to</param>
    internal void Connect(EndPoint ep)
    {
        _socketEP = (IPEndPoint)ep;
        StartConnector();
    }

    /// <summary>
    /// Connect to Remote EndPoint and wait to send data
    /// </summary>
    /// <param name="address">IP Address of Remote computer to connect to</param>
    /// <param name="port">Port Number of Remote computer to connect to</param>
    internal void Connect(string address, int port)
    {
        var ipAddress = Dns.GetHostAddresses(address).First();
        Connect(new IPEndPoint(ipAddress, port));
    }

    /// <summary>
    /// Disconnect any currently connected Sockets and dispose of them
    /// </summary>
    internal void Disconnect()
    {
        try
        {
            SendNotification($"Disconnecting {_socketEP?.Address ?? IPAddress.None}:{_socketEP?.Port ?? 0}");
            if (_handler != null)
            {
                if (_handler.Connected)
                    _handler.Shutdown(SocketShutdown.Both);
                _handler.Dispose();
            }
            if (_socket != null)
            {
                if (_socket.Connected)
                    _socket.Shutdown(SocketShutdown.Both);
                _socket.Dispose();
            }
            _socket = null;
        }
        catch { }
        if (Connected != null)
            Task.Run(() => Connected.DynamicInvoke(this, false));
        SendNotification($"Disconnected {_socketEP?.Address ?? IPAddress.None}:{_socketEP?.Port ?? 0}");
    }

    /// <summary>
    /// Send data to client or remote computer
    /// </summary>
    /// <param name="data">Data to send</param>
    internal void Send(byte[] data)
    {
        try
        {
            var socket = _handler ?? _socket;
            if (socket != null && socket.Connected)
            {
                SendNotification($"Sending to {_socketEP.Address}:{_socketEP.Port} ({data.Length} Bytes)");
                socket.Send(data);
                // Send scompleted Synchronously - switch to Listen mode
                SendNotification($"Sent to {_socketEP.Address}:{_socketEP.Port} ({data.Length} Bytes)");
                StateObject state = new StateObject();
                state.WorkSocket = socket;
                if (socket.Connected)
                {
                    // Begin receiving the data from the remote device.
                    SendNotification($"Start Listening for remote data on {_socketEP.Address}:{_socketEP.Port}");
                    socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadDataCallback), state);
                }
                else
                {
                    SendNotification(new IOException($"Socket {_socketEP.Address}:{_socketEP.Port} was closed after sending data"));
                }
            }
            else
            {
                if (buffer == null)
                {
                    buffer = new byte[data.Length];
                    data.CopyTo(buffer, 0);
                }
                else
                {
                    List<byte> tempBuffer = new List<byte>(buffer);
                    tempBuffer.AddRange(data);
                    buffer = tempBuffer.ToArray();
                }
                SendNotification(new IOException($"Cannot send to {_socketEP?.Address ?? IPAddress.None}:{_socketEP?.Port ?? 0} when not connected, data buffered"));
            }
        }
        catch (SocketException ex)
        {
            SendNotification(ex);
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    /// <summary>
    /// Called when data has been sent to connection
    /// </summary>
    /// <param name="sender">Unused</param>
    /// <param name="e">Data sent to connection for review</param>
    private void SendCallback(object sender, SocketAsyncEventArgs e)
    {
        SendNotification($"Sent to {_socketEP.Address.ToString()}:{_socketEP.Port} ({e.Buffer.Length} Bytes)");
        // Sent something to the remote EndPoint - listen for any response
        StateObject state = new StateObject();
        state.WorkSocket = _socket;
        // If socket is in Connect mode, the following will likely fail
        _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            new AsyncCallback(ReadDataCallback), state);
        sendDone.Set();
    }

    /// <summary>
    /// Start connection to stored Remote EndPoint
    /// </summary>
    private void StartConnector()
    {
        if (_socketEP != null)
        {
            SendNotification($"Connecting to {_socketEP.Address.ToString()}:{_socketEP.Port}");
            _socket = new Socket(_socketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = false;
            _socket.BeginConnect(_socketEP, new AsyncCallback(AcceptConnectionCallback), _socket);
            connectDone.WaitOne();
        }
    }

    /// <summary>
    /// Start listening for incoming data on stored EndPoint
    /// </summary>
    private void StartListener()
    {
        try
        {
            _socket = new Socket(_socketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _socket.Blocking = false;
            _socket.Bind(_socketEP);
            _socket.Listen(1);
            _socket.BeginAccept(new AsyncCallback(AcceptListenerCallback), _socket);
            SendNotification($"Listening on {_socketEP.Address.ToString()}:{_socketEP.Port}");
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    /// <summary>
    /// Called when a connection is established with the remote EndPoint
    /// </summary>
    /// <param name="ar">Connection result details</param>
    private void AcceptConnectionCallback(IAsyncResult ar)
    {
        try
        {
            var socket = (Socket)ar.AsyncState;
            socket.EndConnect(ar);
            SendNotification($"Connection Established: {_socketEP.Address.ToString()}:{_socketEP.Port}");

            IsConnected = socket.Connected;

            if (Connected != null)
                Task.Run(() => Connected.DynamicInvoke(this, IsConnected));
            if (buffer != null)
            {
                SendNotification($"Sending buffered data: {buffer.Length} bytes");
                Send(buffer);
            }
            buffer = null;
        }
        catch(Exception ex)
        {
            SendNotification(ex);
        }
        connectDone.Set();
    }

    /// <summary>
    /// Called when a client connects to our listening EndPoint
    /// </summary>
    /// <param name="ar">Details of connection request</param>
    private void AcceptListenerCallback(IAsyncResult ar)
    {
        try
        {
            var socket = (Socket)ar.AsyncState;
            _handler = socket.EndAccept(ar);
            // Create the state object.  
            StateObject state = new StateObject();
            state.WorkSocket = _handler;
            _handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadDataCallback), state);

            IsConnected = _handler.Connected;

            if (Connected != null)
                Task.Run(() => Connected.DynamicInvoke(this, IsConnected));
            // Client has connected to listener, initiate connection to MSFS
            SendNotification($"Connection Established: {_socketEP.Address.ToString()}:{_socketEP.Port}");
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    /// <summary>
    /// Called when data has been received via our worker socket
    /// </summary>
    /// <param name="ar">Details on received data</param>
    private void ReadDataCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            _handler = state.WorkSocket;
            if (_handler != null)
            {
                if (_handler.Connected)
                {
                    // Read data from the socket.  
                    SocketError errorCode = SocketError.NotConnected;
                    int read = _handler.EndReceive(ar, out errorCode);

                    // Data was read from the client socket.  
                    if (errorCode == SocketError.Success && read > 0)
                    {
                        if (DataReceived != null)
                        {
                            //Array.Copy(state.buffer, dataReceived, read);
                            var dataReceived = state.buffer.ToList().Take(read).ToArray();
                            Task.Run(() => DataReceived.DynamicInvoke(this, dataReceived));
                        }
                        _handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadDataCallback), state);
                    }
                    if(errorCode != SocketError.Success)
                    {
                        // Need to reconnect the socket
                        SendNotification(new ProxyMessage { Message = $"Socket Error: {errorCode}", Severity = 2 });
                        if(!_handler.Connected)
                            Task.Run(() => Connected.DynamicInvoke(this, false));
                    }
                }
                else
                {
                    if (Connected != null)
                        Task.Run(() => Connected.DynamicInvoke(this, false));
                }
            }
            else
            {
                _handler?.Close();
                _handler?.Dispose();
                _handler = null;
            }
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    /// <summary>
    /// Notify consuming application of an exception
    /// </summary>
    /// <param name="ex">Exception to notify</param>
    private void SendNotification(Exception ex)
    {
        if (Notifications == null)
            return;
        string msg = string.Empty;
        msg += ex.Message;
        while (ex.InnerException != null)
        {
            ex = ex.InnerException;
            msg += "\r\n" + ex.Message;
        }
        SendNotification(msg, 1);
        if (_socket?.Connected != IsConnected)
        {
            IsConnected = _socket?.Connected ?? false;
            if (Connected != null)
            {
                Task.Run(() => Connected.DynamicInvoke(this, IsConnected));
            }
        }
    }

    /// <summary>
    /// Notify consuming application of activity
    /// </summary>
    /// <param name="msg">Message to send to client</param>
    /// <param name="severity">Severity of message (1 = Exception, 5 = Debug)</param>
    private void SendNotification(string msg, int severity = 5)
    {
        if (Notifications == null)
            return;
        ProxyMessage notification = new ProxyMessage { Severity = severity, Message = msg };
        SendNotification(notification);
    }

    /// <summary>
    /// Notify consuming application of activity
    /// </summary>
    /// <param name="notification">Message and associated severity</param>
    private void SendNotification(ProxyMessage notification)
    {
        if (Notifications == null)
            return;
        Task.Run(() => Notifications.DynamicInvoke(this, notification));
    }

    /// <summary>
    /// Dispose of any existing sockets and related objects
    /// </summary>
    public void Dispose()
    {
        Disconnect();
        _socketEP = null;
    }
}

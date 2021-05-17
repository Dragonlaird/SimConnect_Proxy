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
    internal EventHandler<byte[]> DataReceived;
    internal EventHandler<bool> Connected;
    internal EventHandler<ProxyMessage> Notifications;

    private ManualResetEvent connectDone = new ManualResetEvent(false);
    private ManualResetEvent sendDone = new ManualResetEvent(false);

    private bool IsConnected = false;
    private IPEndPoint _socketEP = null;
    private Socket _socket;
    /// <summary>
    /// Allow connection to EndPoint and listen for incoming data stream
    /// </summary>
    /// <param name="ep">EndPoint to connect to</param>
    internal void Listen(EndPoint ep)
    {
        _socketEP = (IPEndPoint)ep;
        StartListener();
    }

    /// <summary>
    /// Allow connection to Local Address/Port and listen for incoming data stream
    /// </summary>
    /// <param name="address">IP Address of this computer to listen on</param>
    /// <param name="port">Port Number of this computer to listen on</param>
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
    /// Send data to connection
    /// </summary>
    /// <param name="data">Data to send</param>
    internal void Send(byte[] data)
    {
        try
        {
            if (_socket != null && _socket.Connected)
            {
                SendNotification($"Sending to {_socketEP.Address.ToString()}:{_socketEP.Port} ({data.Length} Bytes)");
                _socket.Send(data);
                // Send scompleted Synchronously - switch to Listen mode
                SendNotification($"Sent to {_socketEP.Address.ToString()}:{_socketEP.Port} ({data.Length} Bytes)");
                StateObject state = new StateObject();
                state.WorkSocket = _socket;
                if (_socket.Connected)
                {
                    // Begin receiving the data from the remote device.  
                    _socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReadDataCallback), state);
                }
                else
                {
                    SendNotification(new IOException("Socket was closed after sending data"));
                }
            }
            else
            {
                throw new IOException("Cannot send data when not connected");
            }
        }
        catch(SocketException ex) {
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
    /// <param name="sender"></param>
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
    /// Start connection to Remote endpoint/address
    /// </summary>
    private void StartConnector()
    {
        SendNotification($"Connecting to {_socketEP.Address.ToString()}:{_socketEP.Port} (Send Mode)");
        _socket = new Socket(_socketEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        _socket.Blocking = false;
        _socket.BeginConnect(_socketEP, new AsyncCallback(AcceptConnectionCallback), _socket);
        //connectDone.WaitOne();
    }

    /// <summary>
    /// Start listening for incoming data
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
            SendNotification($"Connecting to {_socketEP.Address.ToString()}:{_socketEP.Port} (Listen Mode)");
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
            //StateObject state = new StateObject();
            //state.WorkSocket = socket;
            //socket.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            //    new AsyncCallback(ReadDataCallback), state);

            IsConnected = socket.Connected;

            if (Connected != null)
                Task.Run(() => Connected.DynamicInvoke(this, true));
            //connectDone.Set();
        }
        catch(Exception ex)
        {
            SendNotification(ex);
        }
    }

    private void AcceptListenerCallback(IAsyncResult ar)
    {
        try
        {
            var socket = (Socket)ar.AsyncState;
            Socket handler = socket.EndAccept(ar);
            // Create the state object.  
            StateObject state = new StateObject();
            state.WorkSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadDataCallback), state);

            IsConnected = socket.Connected;

            if (Connected != null)
                Task.Run(() => Connected.DynamicInvoke(this, true));
            // Client has connected to listener, initiate connection to MSFS
            SendNotification($"Connection Established: {_socketEP.Address.ToString()}:{_socketEP.Port}");
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    private void ReadDataCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.WorkSocket;
            if (handler != null)
            {
                if (handler.Connected)
                {
                    // Read data from the socket.  
                    SocketError errorCode = SocketError.NotConnected;
                    int read = handler.EndReceive(ar, out errorCode);

                    // Data was read from the client socket.  
                    if (errorCode == SocketError.Success && read > 0)
                    {
                        if (DataReceived != null)
                        {
                            //Array.Copy(state.buffer, dataReceived, read);
                            var dataReceived = state.buffer.ToList().Take(read).ToArray();
                            Task.Run(() => DataReceived.DynamicInvoke(this, dataReceived));
                        }
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(ReadDataCallback), state);
                    }
                    if(errorCode != SocketError.Success)
                    {
                        // Need to reconnect the socket
                        SendNotification(new ProxyMessage { Message = $"Socket Error: {errorCode}", Severity = 2 });
                        if(!handler.Connected)
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
                handler?.Close();
                handler?.Dispose();
                handler = null;
            }
        }
        catch (Exception ex)
        {
            SendNotification(ex);
        }
    }

    /// <summary>
    /// Notify client of an exception
    /// </summary>
    /// <param name="ex">Exception to notify</param>
    /// <param name="isListener"></param>
    private void SendNotification(Exception ex, bool? isListener = null)
    {
        if (Notifications == null)
            return;
        string msg = string.Empty;
        if (isListener != null)
            msg = isListener == true ? "[Listener] " : "[Sender] ";
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
    /// Notify client of activity
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
    /// Notify client of activity
    /// </summary>
    /// <param name="notification">Message and associated severity</param>
    private void SendNotification(ProxyMessage notification)
    {
        if (Notifications == null)
            return;
        Task.Run(() => Notifications.DynamicInvoke(this, notification));
    }

    /// <summary>
    /// Dispose of any existing socket and related objects
    /// </summary>
    public void Dispose()
    {
        if (_socket.Connected)
            try
            {
                _socket?.Shutdown(SocketShutdown.Both);
            }
            catch { }
        _socket?.Dispose();
        _socketEP = null;
    }
}

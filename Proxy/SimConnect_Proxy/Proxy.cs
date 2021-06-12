using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Proxy:IDisposable
{
    private ProxyConnector _sender; // Remote connection, initiated after a local connection is established
    private ProxyConnector _listener; // Local listening port, waits for a connection
    private EndPoint _listenerEP; // EndPoint for Local listening port
    private EndPoint _senderEP; // EndPoint for Remote connection

    public EventHandler<byte[]> RemoteDataReceived; // Event to transfer data received from remote to client application
    public EventHandler<bool> RemoteConnected; // Event to notify client application when remote connection is established or dropped
    public EventHandler<byte[]> LocalDataReceived; // Event to transfer data received from local to client application
    public EventHandler<bool> LocalConnected; // Event to notify client application when local connection is established or dropped
    public EventHandler<ProxyMessage> Notifications; // Event to send information and error messages to client application

    /// <summary>
    /// A single-socket Proxy Server to intercept communication between a local application and a remote endpoint
    /// </summary>
    public Proxy()
    {
        Initialise();
    }

    /// <summary>
    /// Prepare both Listener and Sender connections, their events to methods below
    /// </summary>
    private void Initialise()
    {
        _listener = new ProxyConnector();
        _listener.Notifications += SendNotification;
        _listener.Connected += ProxyConnected;
        _listener.DataReceived += DataReceived;

        _sender = new ProxyConnector();
        _sender.Notifications += SendNotification;
        _sender.Connected += ProxyConnected;
        _sender.DataReceived += DataReceived;

    }

    /// <summary>
    /// Start listening for a local application to connected to a specific local port
    /// </summary>
    /// <param name="localAddress">IP Address or name of local computer</param>
    /// <param name="localPort">Port number to listen to</param>
    public void StartListener(string localAddress, int localPort)
    {
        var ipAddress = Dns.GetHostAddresses(localAddress).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        if (ipAddress == null)
        {
            SendNotification(this, new ProxyMessage { Message = $"Supplied Address: {localAddress} does not resolve to a valid IPv4 address", Severity = 1 });
            return;
        }
        _listenerEP = new IPEndPoint(ipAddress, localPort);
        StartListener(_listenerEP);
    }

    /// <summary>
    /// Start listening for a local application to connected to a specific local port
    /// </summary>
    /// <param name="endPoint">Local EndPoint to listen on</param>
    public void StartListener(EndPoint endPoint)
    {
        _listenerEP = endPoint;
        StartListener();
    }

    /// <summary>
    /// Start listening for a local application to connected to saved EndPoint
    /// </summary>
    private void StartListener()
    {
        // Confirm supplied address is local to this computer
        var ipAddress = ((IPEndPoint)_listenerEP).Address;
        if(Dns.GetHostByAddress(ipAddress).HostName != Dns.GetHostByAddress("127.0.0.1").HostName)
        {
            SendNotification(this, new ProxyMessage { Message = $"Supplied Listener Address: {ipAddress.ToString()} is not local to this computer", Severity = 1 });
            return;
        }

        _listener.Listen(_listenerEP);
    }

    /// <summary>
    /// Stop Listening for data or a connection on local EndPoint, disconnect and dispose of listening connector
    /// </summary>
    public void StopListener()
    {
        if (_listener != null)
            _listener.Disconnect();
    }

    /// <summary>
    /// Innitiate a connection to a local or remote computer, with the specified port
    /// </summary>
    /// <param name="remoteAddress">Computer name or IP Address to connect to</param>
    /// <param name="remotePort">Port number to connect to</param>
    public void StartSender(string remoteAddress, int remotePort)
    {
        var ipAddress = Dns.GetHostAddresses(remoteAddress).FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
        if (ipAddress == null)
        {
            SendNotification(this, new ProxyMessage { Message = $"Supplied Address: {remoteAddress} does not resolve to a valid IPv4 address", Severity = 1 });
            return;
        }
        var senderEP = new IPEndPoint(ipAddress, remotePort);
        StartSender(senderEP);
    }

    /// <summary>
    /// Innitiate a connection to a local or remote computer, with the specified port
    /// </summary>
    /// <param name="endPoint">Local or Remote EndPoint to connect to</param>
    private void StartSender(EndPoint endPoint)
    {
        _senderEP = endPoint;
        StartSender();
    }

    /// <summary>
    /// Initiate a connection to the saved remote EndPoint
    /// </summary>
    public void StartSender()
    {
        _sender.Connect(_senderEP);
    }

    /// <summary>
    /// Stop Listening for data and disconnect any established remote EndPoint connection, dsipose of sender connector
    /// </summary>
    public void StopSender()
    {
        if (_sender != null)
            _sender.Disconnect();
    }

    /// <summary>
    /// Data has been received from either the local or remote EndPoint
    /// </summary>
    /// <param name="sender">ProxyConnector receiving the data</param>
    /// <param name="e">Data received</param>
    private void DataReceived(object sender, byte[] e)
    {
        if (sender == _sender && RemoteDataReceived != null)
        {
            Task.Run(() => RemoteDataReceived.DynamicInvoke(this, e));
            if (_listener != null)
                _listener.Send(e);
        }
        if (sender == _listener && LocalDataReceived != null)
        {
            Task.Run(() => LocalDataReceived.DynamicInvoke(this, e));
            if (_sender != null)
                _sender.Send(e);
        }
    }

    /// <summary>
    /// ProxyConnector has either connected or disconnected
    /// </summary>
    /// <param name="sender">ProxyConnector notifying us</param>
    /// <param name="e">True = Connected; False = Disconnected</param>
    private void ProxyConnected(object sender, bool e)
    {
        if (sender == _sender && RemoteConnected != null)
            Task.Run(() => RemoteConnected.DynamicInvoke(this, e));
        if (sender == _listener)
        {
            if (LocalConnected != null)
                Task.Run(() => LocalConnected.DynamicInvoke(this, e));
            if (!e && _sender != null)
                _sender.Disconnect();
        }
    }


    /// <summary>
    /// Inform client application of a status change
    /// </summary>
    /// <param name="sender">Object notifying us</param>
    /// <param name="notification">Message and Severity</param>
    private void SendNotification(object sender, ProxyMessage notification)
    {
        if (Notifications == null)
            return;
        Task.Run(() => Notifications.DynamicInvoke(this, notification));
    }

    /// <summary>
    /// Dispose of both Local Listener and Sender Connectors, freeing any local resources used
    /// </summary>
    public void Dispose()
    {
        _sender?.Disconnect();
        _sender?.Dispose();
        _senderEP = null;
        _listener?.Disconnect();
        _listener?.Dispose();
        _listenerEP = null;
    }
}
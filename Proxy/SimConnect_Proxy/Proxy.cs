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
    private ProxyConnector _sender;
    private ProxyConnector _listener;
    private EndPoint _listenerEP;
    private EndPoint _senderEP;

    public EventHandler<byte[]> RemoteDataReceived;
    public EventHandler<bool> RemoteConnected;
    public EventHandler<byte[]> LocalDataReceived;
    public EventHandler<bool> LocalConnected;
    public EventHandler<ProxyMessage> Notifications;

    public Proxy()
    {
        Initialise();
    }

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

    public void StartListener(EndPoint endPoint)
    {
        _listenerEP = endPoint;
        StartListener();
    }

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

    private void StartSender(EndPoint endPoint)
    {
        _senderEP = endPoint;
        StartSender();
    }

    public void StartSender()
    {
        _sender.Connect(_senderEP);
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
        if(sender == _sender && RemoteConnected != null)
            Task.Run(() => RemoteConnected.DynamicInvoke(this, e));
        if(sender == _listener && LocalConnected != null)
            Task.Run(() => LocalConnected.DynamicInvoke(this, e));
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
    /// Dispose of both Local Listener and Remote Connector
    /// </summary>
    public void Dispose()
    {
        _sender?.Dispose();
        _listener?.Dispose();
    }
}
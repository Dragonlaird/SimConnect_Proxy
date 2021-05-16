using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

static class SocketExtensions
{
    public static bool IsConnected(this Socket socket)
    {
        try
        {
            return !(socket.Poll(1, SelectMode.SelectRead) && socket.Available == 0);
        }
        catch (SocketException) { return false; }
    }
}

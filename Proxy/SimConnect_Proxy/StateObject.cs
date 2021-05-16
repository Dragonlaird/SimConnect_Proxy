using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class StateObject
{
    public Socket WorkSocket = null;
    public const int BufferSize = 1024;
    public byte[] buffer = new byte[BufferSize];
    //public StringBuilder sb = new StringBuilder();
}

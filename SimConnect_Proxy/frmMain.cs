using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimConnect_Proxy
{
    public partial class frmMain : Form
    {
        private Proxy _proxy;

        private const int bytesPerDisplayLine = 8;

        private TcpClient client = null;
        public frmMain()
        {
            InitializeComponent();
        }

        private void pbStartProxy_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text == "Start Proxy")
            {
                // Clear Inbound and Outbound data reports, ready for new connections
                txtDataReceived.Text = "";
                txtDataSent.Text = "";

                // Start Proxy to listen for local connections and connect to remote
                string listenerAddress = txtListenerAddress.Text;
                int listenerPort = (int)txtListenerPort.Value;
                string senderAddress = txtSenderAddress.Text;
                int senderPort = (int)txtSenderPort.Value;
                _proxy = new Proxy();
                _proxy.RemoteConnected += RemoteConnected;
                _proxy.RemoteDataReceived += RemoteDataReceived;
                _proxy.LocalConnected += LocalConnected;
                _proxy.LocalDataReceived += LocalDataReceived;
                _proxy.Notifications += NotificationReceived;
                _proxy.StartListener(listenerAddress, listenerPort);
                //_proxy.StartSender(senderAddress, senderPort);
                ((Button)sender).Text = "Stop Proxy";
            }
            else
            {
                _proxy.Dispose();
                ((Button)sender).Text = "Start Proxy";
            }
        }

        private void NotificationReceived(object sender, ProxyMessage e)
        {
            var message = $"{DateTime.Now:HH:mm:ss} [{e.Severity}]: {e.Message}\r\n";
            if (txtErrors.InvokeRequired)
            {
                txtErrors.Invoke(new Action(() => txtErrors.Text += message));
                return;
            }
            txtErrors.Text += message;
        }

        private void LocalDataReceived(object sender, byte[] e)
        {
            ShowData(e, txtDataSent);
        }

        private void RemoteDataReceived(object sender, byte[] e)
        {
            ShowData(e, txtDataReceived);
        }

        private void LocalConnected(object sender, bool e)
        {
            _proxy.StartSender(txtSenderAddress.Text, (int)txtSenderPort.Value);
            if (cbLocalConnected.InvokeRequired)
            {
                cbLocalConnected.Invoke(new Action(() => cbLocalConnected.Checked = e));
                return;
            }
            cbLocalConnected.Checked = e;
        }

        private void ShowData(byte[] data, TextBox txtBox)
        {
            if (txtBox.InvokeRequired)
            {
                txtBox.Invoke(new Action(() => ShowData(data, txtBox)));
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss}]");
            string displayChars = string.Empty;
            for(var i = 0; i < data.Length; i++)
            {
                var byt = data[i];
                sb.Append($"{byt:X2} ");
                displayChars += DisplayChar(byt) + " ";
                if (displayChars.Length > 0 && (displayChars.Length / 2) % bytesPerDisplayLine == 0)
                {
                    sb.Append(" " + displayChars + "\r\n");
                    displayChars = string.Empty;
                }
            }
            if (displayChars.Length != 0)
            {
                var result = sb.ToString();
                result = result.PadRight(1 + 3 * (data.Length % bytesPerDisplayLine), ' ');
                sb = new StringBuilder(result);
                sb.Append(displayChars + "\r\n");
            }

            txtBox.Text += sb.ToString();
            txtBox.SelectionLength = txtBox.Text.Length;
            txtBox.ScrollToCaret();
        }

        private char DisplayChar(byte byt)
        {
            if (byt < 32 || byt > 126)
                return '.';
            return (char)byt;
        }

        private void RemoteConnected(object sender, bool e)
        {
            if (cbRemoteConnected.InvokeRequired)
            {
                cbRemoteConnected.Invoke(new Action(() => cbRemoteConnected.Checked = e));
                return;
            }
            cbRemoteConnected.Checked = e;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimConnect_Proxy
{
    public partial class frmMain : Form
    {
        private Proxy _proxy; // Proxy Server component to handle local and remote connections and data transfer

        private const int bytesPerDisplayLine = 8; // Number of bytes to display per line in both Data Sent and Data Received text boxes
        private const int maxLinesToDisplay = 1000; // Maximum number of lines per TextBox, older lines beyond this count are removed

        private FileStream dataStream;
        private const string dataFilePath = "FS_Data_Stream.csv";

        public frmMain()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Start/Stop the Proxy Listener
        /// </summary>
        /// <param name="sender">This (unused)</param>
        /// <param name="e">Null/not required</param>
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
                NotificationReceived(this, new ProxyMessage { Message = "Starting Proxy" });
                _proxy.StartListener(listenerAddress, listenerPort);
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), dataFilePath);
                dataStream = CreateFile(filePath);
                ((Button)sender).Text = "Stop Proxy";
            }
            else
            {
                NotificationReceived(this, new ProxyMessage { Message = "Stopping Proxy" });
                _proxy?.StopListener();
                _proxy?.StopSender();
                _proxy?.Dispose();
                cbLocalConnected.Checked = false;
                cbRemoteConnected.Checked = false;
                ((Button)sender).Text = "Start Proxy";
                if (dataStream != null)
                    CloseFile();
            }
        }

        private FileStream CreateFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && !filePath.Any(x=> Path.InvalidPathChars.Contains(x)))
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                var fileStream = File.OpenWrite(filePath);
                return fileStream;
            }
            else
            {
                NotificationReceived(this, new ProxyMessage { Message = $"Cannot create file {filePath}", Severity = 2 });
                return null;
            }
        }

        private void CloseFile()
        {
            if (dataStream != null)
            {
                dataStream.Flush();
                dataStream.Close();
            }
        }

        private void WriteToFile(string text)
        {
            if (dataStream != null)
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                dataStream.Write(bytes, 0, bytes.Length);
            }
        }

        /// <summary>
        /// Display any messages received from Proxy in txtErrors box
        /// </summary>
        /// <param name="sender">Proxy (unused)</param>
        /// <param name="e">Proxy message with severity</param>
        private void NotificationReceived(object sender, ProxyMessage e)
        {
            var message = $"{DateTime.Now:HH:mm:ss} [{e.Severity}]: {e.Message}\r\n";
            if (txtErrors.InvokeRequired)
            {
                txtErrors.Invoke(new Action(() =>
                {
                    txtErrors.Text += message;
                    txtErrors.SelectionLength = txtErrors.Text.Length;
                    txtErrors.ScrollToCaret();
                }));
                return;
            }
            while (txtErrors.Lines.Count() > maxLinesToDisplay)
            {
                txtErrors.Lines = txtErrors.Lines.Skip(1).Take(txtErrors.Lines.Length - 1).ToArray();
            }

            txtErrors.Text += message;
            txtErrors.SelectionLength = txtErrors.Text.Length;
            txtErrors.ScrollToCaret();
        }

        /// <summary>
        /// Display any data received by Proxy Listener (local connection)
        /// Data is formatted into a hex display layout and added to txtDataSend
        /// </summary>
        /// <param name="sender">Proxy (unused)</param>
        /// <param name="e">Data received by Listener</param>
        private void LocalDataReceived(object sender, byte[] e)
        {
            ShowData(e, txtDataSent);
        }

        /// <summary>
        /// Display any data received by Proxy Sender (remote connection)
        /// Data is formatted into a hex display layout and added to txtDataReceived
        /// </summary>
        /// <param name="sender">Proxy (unused)</param>
        /// <param name="e">Data received by Sender</param>
        private void RemoteDataReceived(object sender, byte[] e)
        {
            ShowData(e, txtDataReceived);
        }

        /// <summary>
        /// Display data in supplied textbox, formatted into a hex display format
        /// </summary>
        /// <param name="data">data to display</param>
        /// <param name="txtBox">TextBox to populate</param>
        private void ShowData(byte[] data, TextBox txtBox)
        {
            if (txtBox.InvokeRequired)
            {
                txtBox.Invoke(new Action(() => ShowData(data, txtBox)));
                return;
            }
            StringBuilder sb = new StringBuilder();
            StringBuilder op = new StringBuilder(); // Used to generate the CSV file output
            op.Append(txtBox == txtDataSent ? "L," : "R,");
            op.Append("\"0000\",");
            sb.AppendLine($"[{DateTime.Now:HH:mm:ss}]");
            string displayChars = string.Empty;
            sb.Append("0000: ");
            for(var i = 0; i < data.Length; i++)
            {
                var byt = data[i];
                sb.Append($"{byt:X2} ");
                op.Append($"\"0x{byt:X2}\",");
                displayChars += DisplayChar(byt) + " ";
                if (displayChars.Length > 0 && (displayChars.Length / 2) % bytesPerDisplayLine == 0)
                {
                    sb.Append(" " + displayChars + "\r\n");
                    op.Append($"\"{displayChars}\"\r\n");
                    displayChars = string.Empty;
                    if (i < data.Length - 1)
                    {
                        sb.Append((i + 1).ToString("0000") + ": ");
                        op.Append(txtBox == txtDataSent ? "L," : "R,");
                        op.Append("\"" + (i + 1).ToString("0000") + "\",");
                    }
                }
            }
            if (displayChars.Length != 0)
            {
                var result = sb.ToString();
                result = result.PadRight(result.Length + 1 + 3 * (data.Length % bytesPerDisplayLine), ' ');
                sb = new StringBuilder(result);
                sb.Append(displayChars + "\r\n");
                result = op.ToString();
                result = result.PadRight(result.Length + data.Length % bytesPerDisplayLine, ',');
                op = new StringBuilder(result);
                op.Append($"\"{displayChars}\"\r\n");
            }
            txtBox.Text += sb.ToString();
            WriteToFile(op.ToString());
            while (txtBox.Lines.Count() > maxLinesToDisplay)
            {
                txtBox.Lines = txtBox.Lines.Skip(1).Take(txtBox.Lines.Length - 1).ToArray();
            }
            txtBox.SelectionLength = txtBox.Text.Length;
            txtBox.ScrollToCaret();
        }

        /// <summary>
        /// Convert a single byte into a dsiplayable character.
        /// If byte is outside the normal display range, returns a full stop character
        /// </summary>
        /// <param name="byt">Byte to convert</param>
        /// <returns>Character to display</returns>
        private char DisplayChar(byte byt)
        {
            if (byt < 32 || byt > 126)
                return '.';
            return (char)byt;
        }

        /// <summary>
        /// Change cbLocalConnected Checked property,
        /// when Proxy has successfully connected or disconnected to/from a local endpoint 
        /// </summary>
        /// <param name="sender">Proxy (unused)</param>
        /// <param name="e">Boolean value indicating if connected</param>
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

        /// <summary>
        /// Change cbRemoteConnected Checked property,
        /// when Proxy has successfully connected or disconnected to/from a remote endpoint 
        /// </summary>
        /// <param name="sender">Proxy (unused)</param>
        /// <param name="e">Boolean value indicating if connected</param>
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

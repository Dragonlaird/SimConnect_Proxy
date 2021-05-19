
namespace SimConnect_Proxy
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtListenerAddress = new System.Windows.Forms.TextBox();
            this.txtSenderAddress = new System.Windows.Forms.TextBox();
            this.txtListenerPort = new System.Windows.Forms.NumericUpDown();
            this.txtSenderPort = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.pbStartProxy = new System.Windows.Forms.Button();
            this.cbLocalConnected = new System.Windows.Forms.CheckBox();
            this.cbRemoteConnected = new System.Windows.Forms.CheckBox();
            this.txtErrors = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.txtDataSent = new System.Windows.Forms.TextBox();
            this.txtDataReceived = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.txtListenerPort)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSenderPort)).BeginInit();
            this.SuspendLayout();
            // 
            // txtListenerAddress
            // 
            this.txtListenerAddress.Location = new System.Drawing.Point(117, 16);
            this.txtListenerAddress.Name = "txtListenerAddress";
            this.txtListenerAddress.Size = new System.Drawing.Size(100, 20);
            this.txtListenerAddress.TabIndex = 0;
            this.txtListenerAddress.Text = "localhost";
            // 
            // txtSenderAddress
            // 
            this.txtSenderAddress.Location = new System.Drawing.Point(117, 79);
            this.txtSenderAddress.Name = "txtSenderAddress";
            this.txtSenderAddress.Size = new System.Drawing.Size(100, 20);
            this.txtSenderAddress.TabIndex = 1;
            this.txtSenderAddress.Text = "localhost";
            // 
            // txtListenerPort
            // 
            this.txtListenerPort.Location = new System.Drawing.Point(117, 42);
            this.txtListenerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtListenerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtListenerPort.Name = "txtListenerPort";
            this.txtListenerPort.Size = new System.Drawing.Size(120, 20);
            this.txtListenerPort.TabIndex = 2;
            this.txtListenerPort.Value = new decimal(new int[] {
            499,
            0,
            0,
            0});
            // 
            // txtSenderPort
            // 
            this.txtSenderPort.Location = new System.Drawing.Point(117, 106);
            this.txtSenderPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
            this.txtSenderPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.txtSenderPort.Name = "txtSenderPort";
            this.txtSenderPort.Size = new System.Drawing.Size(120, 20);
            this.txtSenderPort.TabIndex = 3;
            this.txtSenderPort.Value = new decimal(new int[] {
            500,
            0,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(37, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Local Address:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(56, 44);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Local Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 82);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Remote Address:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(45, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Remote Port:";
            // 
            // pbStartProxy
            // 
            this.pbStartProxy.Location = new System.Drawing.Point(244, 132);
            this.pbStartProxy.Name = "pbStartProxy";
            this.pbStartProxy.Size = new System.Drawing.Size(75, 23);
            this.pbStartProxy.TabIndex = 8;
            this.pbStartProxy.Text = "Start Proxy";
            this.pbStartProxy.UseVisualStyleBackColor = true;
            this.pbStartProxy.Click += new System.EventHandler(this.pbStartProxy_Click);
            // 
            // cbLocalConnected
            // 
            this.cbLocalConnected.AutoSize = true;
            this.cbLocalConnected.Enabled = false;
            this.cbLocalConnected.Location = new System.Drawing.Point(241, 20);
            this.cbLocalConnected.Name = "cbLocalConnected";
            this.cbLocalConnected.Size = new System.Drawing.Size(78, 17);
            this.cbLocalConnected.TabIndex = 12;
            this.cbLocalConnected.Text = "Connected";
            this.cbLocalConnected.UseVisualStyleBackColor = true;
            // 
            // cbRemoteConnected
            // 
            this.cbRemoteConnected.AutoSize = true;
            this.cbRemoteConnected.Enabled = false;
            this.cbRemoteConnected.Location = new System.Drawing.Point(241, 81);
            this.cbRemoteConnected.Name = "cbRemoteConnected";
            this.cbRemoteConnected.Size = new System.Drawing.Size(78, 17);
            this.cbRemoteConnected.TabIndex = 13;
            this.cbRemoteConnected.Text = "Connected";
            this.cbRemoteConnected.UseVisualStyleBackColor = true;
            // 
            // txtErrors
            // 
            this.txtErrors.Location = new System.Drawing.Point(12, 172);
            this.txtErrors.Multiline = true;
            this.txtErrors.Name = "txtErrors";
            this.txtErrors.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtErrors.Size = new System.Drawing.Size(384, 303);
            this.txtErrors.TabIndex = 14;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 153);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(58, 13);
            this.label6.TabIndex = 15;
            this.label6.Text = "Messages:";
            // 
            // txtDataSent
            // 
            this.txtDataSent.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDataSent.Location = new System.Drawing.Point(409, 23);
            this.txtDataSent.Multiline = true;
            this.txtDataSent.Name = "txtDataSent";
            this.txtDataSent.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDataSent.Size = new System.Drawing.Size(379, 210);
            this.txtDataSent.TabIndex = 16;
            // 
            // txtDataReceived
            // 
            this.txtDataReceived.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtDataReceived.Location = new System.Drawing.Point(409, 265);
            this.txtDataReceived.Multiline = true;
            this.txtDataReceived.Name = "txtDataReceived";
            this.txtDataReceived.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDataReceived.Size = new System.Drawing.Size(379, 210);
            this.txtDataReceived.TabIndex = 17;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(406, 7);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(58, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Data Sent:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(406, 249);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(82, 13);
            this.label8.TabIndex = 19;
            this.label8.Text = "Data Received:";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 487);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtDataReceived);
            this.Controls.Add(this.txtDataSent);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtErrors);
            this.Controls.Add(this.cbRemoteConnected);
            this.Controls.Add(this.cbLocalConnected);
            this.Controls.Add(this.pbStartProxy);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtSenderPort);
            this.Controls.Add(this.txtListenerPort);
            this.Controls.Add(this.txtSenderAddress);
            this.Controls.Add(this.txtListenerAddress);
            this.Name = "frmMain";
            this.Text = "Proxy Launcher";
            ((System.ComponentModel.ISupportInitialize)(this.txtListenerPort)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSenderPort)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtListenerAddress;
        private System.Windows.Forms.TextBox txtSenderAddress;
        private System.Windows.Forms.NumericUpDown txtListenerPort;
        private System.Windows.Forms.NumericUpDown txtSenderPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button pbStartProxy;
        private System.Windows.Forms.CheckBox cbLocalConnected;
        private System.Windows.Forms.CheckBox cbRemoteConnected;
        private System.Windows.Forms.TextBox txtErrors;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtDataSent;
        private System.Windows.Forms.TextBox txtDataReceived;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}


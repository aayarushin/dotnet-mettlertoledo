namespace RICADO.MettlerToledo.WinFormsTest;

partial class Form1
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        grpConnection = new System.Windows.Forms.GroupBox();
        lblPort = new System.Windows.Forms.Label();
        cmbPort = new System.Windows.Forms.ComboBox();
        btnRefreshPorts = new System.Windows.Forms.Button();
        lblBaudRate = new System.Windows.Forms.Label();
        cmbBaudRate = new System.Windows.Forms.ComboBox();
        btnConnect = new System.Windows.Forms.Button();
        btnDisconnect = new System.Windows.Forms.Button();

        grpResults = new System.Windows.Forms.GroupBox();
        lblNetWeightLabel = new System.Windows.Forms.Label();
        lblNetWeight = new System.Windows.Forms.Label();
        lblTareWeightLabel = new System.Windows.Forms.Label();
        lblTareWeight = new System.Windows.Forms.Label();
        lblGrossWeightLabel = new System.Windows.Forms.Label();
        lblGrossWeight = new System.Windows.Forms.Label();
        lblUnitsLabel = new System.Windows.Forms.Label();
        lblUnits = new System.Windows.Forms.Label();
        lblStableLabel = new System.Windows.Forms.Label();
        lblStable = new System.Windows.Forms.Label();

        grpActive = new System.Windows.Forms.GroupBox();
        btnReadWeightStatus = new System.Windows.Forms.Button();
        btnReadNetWeight = new System.Windows.Forms.Button();
        btnReadTareWeight = new System.Windows.Forms.Button();
        btnReadSerial = new System.Windows.Forms.Button();
        btnReadFirmware = new System.Windows.Forms.Button();

        grpReactive = new System.Windows.Forms.GroupBox();
        chkAutoPoll = new System.Windows.Forms.CheckBox();
        lblPollInterval = new System.Windows.Forms.Label();
        numPollInterval = new System.Windows.Forms.NumericUpDown();
        lblPollMs = new System.Windows.Forms.Label();

        grpCommands = new System.Windows.Forms.GroupBox();
        btnZeroStable = new System.Windows.Forms.Button();
        btnZeroImmediate = new System.Windows.Forms.Button();
        btnTareStable = new System.Windows.Forms.Button();
        btnTareImmediate = new System.Windows.Forms.Button();
        btnClearTare = new System.Windows.Forms.Button();

        txtLog = new System.Windows.Forms.TextBox();
        lblLogHeader = new System.Windows.Forms.Label();
        btnClearLog = new System.Windows.Forms.Button();
        statusStrip = new System.Windows.Forms.StatusStrip();
        lblStatus = new System.Windows.Forms.ToolStripStatusLabel();

        grpConnection.SuspendLayout();
        grpResults.SuspendLayout();
        grpActive.SuspendLayout();
        grpReactive.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)numPollInterval).BeginInit();
        grpCommands.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        // grpConnection
        grpConnection.Text = "Connection Settings";
        grpConnection.Location = new System.Drawing.Point(12, 12);
        grpConnection.Size = new System.Drawing.Size(460, 110);
        grpConnection.Controls.Add(lblPort);
        grpConnection.Controls.Add(cmbPort);
        grpConnection.Controls.Add(btnRefreshPorts);
        grpConnection.Controls.Add(lblBaudRate);
        grpConnection.Controls.Add(cmbBaudRate);
        grpConnection.Controls.Add(btnConnect);
        grpConnection.Controls.Add(btnDisconnect);

        lblPort.Text = "COM Port:";
        lblPort.Location = new System.Drawing.Point(10, 28);
        lblPort.Size = new System.Drawing.Size(65, 23);
        lblPort.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        cmbPort.Location = new System.Drawing.Point(80, 28);
        cmbPort.Size = new System.Drawing.Size(110, 23);
        cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

        btnRefreshPorts.Text = "⟳";
        btnRefreshPorts.Location = new System.Drawing.Point(196, 28);
        btnRefreshPorts.Size = new System.Drawing.Size(30, 23);
        btnRefreshPorts.Click += new System.EventHandler(btnRefreshPorts_Click);

        lblBaudRate.Text = "Baud Rate:";
        lblBaudRate.Location = new System.Drawing.Point(238, 28);
        lblBaudRate.Size = new System.Drawing.Size(70, 23);
        lblBaudRate.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        cmbBaudRate.Location = new System.Drawing.Point(313, 28);
        cmbBaudRate.Size = new System.Drawing.Size(80, 23);
        cmbBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDown;
        cmbBaudRate.Items.AddRange(new object[] { "1200", "2400", "4800", "9600", "19200", "38400", "57600", "115200" });
        cmbBaudRate.Text = "9600";

        btnConnect.Text = "Connect";
        btnConnect.Location = new System.Drawing.Point(80, 68);
        btnConnect.Size = new System.Drawing.Size(90, 30);
        btnConnect.BackColor = System.Drawing.Color.FromArgb(0, 150, 60);
        btnConnect.ForeColor = System.Drawing.Color.White;
        btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnConnect.Click += new System.EventHandler(btnConnect_Click);

        btnDisconnect.Text = "Disconnect";
        btnDisconnect.Location = new System.Drawing.Point(180, 68);
        btnDisconnect.Size = new System.Drawing.Size(90, 30);
        btnDisconnect.BackColor = System.Drawing.Color.FromArgb(180, 40, 40);
        btnDisconnect.ForeColor = System.Drawing.Color.White;
        btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDisconnect.Enabled = false;
        btnDisconnect.Click += new System.EventHandler(btnDisconnect_Click);

        // grpResults
        grpResults.Text = "Live Measurements";
        grpResults.Location = new System.Drawing.Point(480, 12);
        grpResults.Size = new System.Drawing.Size(300, 140);
        grpResults.Controls.Add(lblNetWeightLabel);
        grpResults.Controls.Add(lblNetWeight);
        grpResults.Controls.Add(lblTareWeightLabel);
        grpResults.Controls.Add(lblTareWeight);
        grpResults.Controls.Add(lblGrossWeightLabel);
        grpResults.Controls.Add(lblGrossWeight);
        grpResults.Controls.Add(lblUnitsLabel);
        grpResults.Controls.Add(lblUnits);
        grpResults.Controls.Add(lblStableLabel);
        grpResults.Controls.Add(lblStable);

        lblNetWeightLabel.Text = "Net Weight:";
        lblNetWeightLabel.Location = new System.Drawing.Point(10, 26);
        lblNetWeightLabel.Size = new System.Drawing.Size(80, 20);
        lblNetWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        lblNetWeight.Text = "---";
        lblNetWeight.Location = new System.Drawing.Point(95, 26);
        lblNetWeight.Size = new System.Drawing.Size(100, 20);
        lblNetWeight.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);
        lblNetWeight.ForeColor = System.Drawing.Color.DarkBlue;

        lblTareWeightLabel.Text = "Tare Weight:";
        lblTareWeightLabel.Location = new System.Drawing.Point(10, 50);
        lblTareWeightLabel.Size = new System.Drawing.Size(80, 20);
        lblTareWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        lblTareWeight.Text = "---";
        lblTareWeight.Location = new System.Drawing.Point(95, 50);
        lblTareWeight.Size = new System.Drawing.Size(100, 20);
        lblTareWeight.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);
        lblTareWeight.ForeColor = System.Drawing.Color.DarkGreen;

        lblGrossWeightLabel.Text = "Gross Weight:";
        lblGrossWeightLabel.Location = new System.Drawing.Point(10, 74);
        lblGrossWeightLabel.Size = new System.Drawing.Size(80, 20);
        lblGrossWeightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        lblGrossWeight.Text = "---";
        lblGrossWeight.Location = new System.Drawing.Point(95, 74);
        lblGrossWeight.Size = new System.Drawing.Size(100, 20);
        lblGrossWeight.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);

        lblUnitsLabel.Text = "Units:";
        lblUnitsLabel.Location = new System.Drawing.Point(200, 26);
        lblUnitsLabel.Size = new System.Drawing.Size(45, 20);
        lblUnitsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        lblUnits.Text = "---";
        lblUnits.Location = new System.Drawing.Point(250, 26);
        lblUnits.Size = new System.Drawing.Size(40, 20);
        lblUnits.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);

        lblStableLabel.Text = "Stable:";
        lblStableLabel.Location = new System.Drawing.Point(200, 50);
        lblStableLabel.Size = new System.Drawing.Size(45, 20);
        lblStableLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        lblStable.Text = "---";
        lblStable.Location = new System.Drawing.Point(250, 50);
        lblStable.Size = new System.Drawing.Size(40, 20);
        lblStable.Font = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);

        // grpActive
        grpActive.Text = "Active Requests";
        grpActive.Location = new System.Drawing.Point(12, 130);
        grpActive.Size = new System.Drawing.Size(460, 105);
        grpActive.Controls.Add(btnReadWeightStatus);
        grpActive.Controls.Add(btnReadNetWeight);
        grpActive.Controls.Add(btnReadTareWeight);
        grpActive.Controls.Add(btnReadSerial);
        grpActive.Controls.Add(btnReadFirmware);

        btnReadWeightStatus.Text = "Weight && Status";
        btnReadWeightStatus.Location = new System.Drawing.Point(10, 28);
        btnReadWeightStatus.Size = new System.Drawing.Size(120, 30);
        btnReadWeightStatus.Enabled = false;
        btnReadWeightStatus.Click += new System.EventHandler(btnReadWeightStatus_Click);

        btnReadNetWeight.Text = "Net Weight";
        btnReadNetWeight.Location = new System.Drawing.Point(140, 28);
        btnReadNetWeight.Size = new System.Drawing.Size(100, 30);
        btnReadNetWeight.Enabled = false;
        btnReadNetWeight.Click += new System.EventHandler(btnReadNetWeight_Click);

        btnReadTareWeight.Text = "Tare Weight";
        btnReadTareWeight.Location = new System.Drawing.Point(250, 28);
        btnReadTareWeight.Size = new System.Drawing.Size(100, 30);
        btnReadTareWeight.Enabled = false;
        btnReadTareWeight.Click += new System.EventHandler(btnReadTareWeight_Click);

        btnReadSerial.Text = "Serial Number";
        btnReadSerial.Location = new System.Drawing.Point(10, 66);
        btnReadSerial.Size = new System.Drawing.Size(110, 27);
        btnReadSerial.Enabled = false;
        btnReadSerial.Click += new System.EventHandler(btnReadSerial_Click);

        btnReadFirmware.Text = "Firmware Rev";
        btnReadFirmware.Location = new System.Drawing.Point(130, 66);
        btnReadFirmware.Size = new System.Drawing.Size(110, 27);
        btnReadFirmware.Enabled = false;
        btnReadFirmware.Click += new System.EventHandler(btnReadFirmware_Click);

        // grpReactive
        grpReactive.Text = "Reactive (Auto-Poll)";
        grpReactive.Location = new System.Drawing.Point(12, 243);
        grpReactive.Size = new System.Drawing.Size(460, 60);
        grpReactive.Controls.Add(chkAutoPoll);
        grpReactive.Controls.Add(lblPollInterval);
        grpReactive.Controls.Add(numPollInterval);
        grpReactive.Controls.Add(lblPollMs);

        chkAutoPoll.Text = "Enable Auto-Poll";
        chkAutoPoll.Location = new System.Drawing.Point(10, 26);
        chkAutoPoll.Size = new System.Drawing.Size(130, 23);
        chkAutoPoll.Enabled = false;
        chkAutoPoll.CheckedChanged += new System.EventHandler(chkAutoPoll_CheckedChanged);

        lblPollInterval.Text = "Interval:";
        lblPollInterval.Location = new System.Drawing.Point(155, 26);
        lblPollInterval.Size = new System.Drawing.Size(55, 23);
        lblPollInterval.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

        numPollInterval.Location = new System.Drawing.Point(215, 26);
        numPollInterval.Size = new System.Drawing.Size(70, 23);
        numPollInterval.Minimum = 200;
        numPollInterval.Maximum = 60000;
        numPollInterval.Value = 500;
        numPollInterval.Increment = 100;
        numPollInterval.ValueChanged += new System.EventHandler(numPollInterval_ValueChanged);

        lblPollMs.Text = "ms";
        lblPollMs.Location = new System.Drawing.Point(291, 26);
        lblPollMs.Size = new System.Drawing.Size(25, 23);
        lblPollMs.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        // grpCommands
        grpCommands.Text = "Commands";
        grpCommands.Location = new System.Drawing.Point(12, 311);
        grpCommands.Size = new System.Drawing.Size(460, 65);
        grpCommands.Controls.Add(btnZeroStable);
        grpCommands.Controls.Add(btnZeroImmediate);
        grpCommands.Controls.Add(btnTareStable);
        grpCommands.Controls.Add(btnTareImmediate);
        grpCommands.Controls.Add(btnClearTare);

        btnZeroStable.Text = "Zero (Stable)";
        btnZeroStable.Location = new System.Drawing.Point(10, 26);
        btnZeroStable.Size = new System.Drawing.Size(90, 27);
        btnZeroStable.Enabled = false;
        btnZeroStable.Click += new System.EventHandler(btnZeroStable_Click);

        btnZeroImmediate.Text = "Zero (Immed.)";
        btnZeroImmediate.Location = new System.Drawing.Point(108, 26);
        btnZeroImmediate.Size = new System.Drawing.Size(95, 27);
        btnZeroImmediate.Enabled = false;
        btnZeroImmediate.Click += new System.EventHandler(btnZeroImmediate_Click);

        btnTareStable.Text = "Tare (Stable)";
        btnTareStable.Location = new System.Drawing.Point(211, 26);
        btnTareStable.Size = new System.Drawing.Size(90, 27);
        btnTareStable.Enabled = false;
        btnTareStable.Click += new System.EventHandler(btnTareStable_Click);

        btnTareImmediate.Text = "Tare (Immed.)";
        btnTareImmediate.Location = new System.Drawing.Point(309, 26);
        btnTareImmediate.Size = new System.Drawing.Size(95, 27);
        btnTareImmediate.Enabled = false;
        btnTareImmediate.Click += new System.EventHandler(btnTareImmediate_Click);

        btnClearTare.Text = "Clear Tare";
        btnClearTare.Location = new System.Drawing.Point(412, 26);
        btnClearTare.Size = new System.Drawing.Size(38, 27);
        btnClearTare.Enabled = false;
        btnClearTare.Click += new System.EventHandler(btnClearTare_Click);

        // Log area
        lblLogHeader.Text = "Activity Log:";
        lblLogHeader.Location = new System.Drawing.Point(12, 384);
        lblLogHeader.Size = new System.Drawing.Size(80, 20);
        lblLogHeader.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;

        btnClearLog.Text = "Clear";
        btnClearLog.Location = new System.Drawing.Point(100, 383);
        btnClearLog.Size = new System.Drawing.Size(55, 22);
        btnClearLog.Click += new System.EventHandler(btnClearLog_Click);

        txtLog.Location = new System.Drawing.Point(12, 408);
        txtLog.Size = new System.Drawing.Size(760, 220);
        txtLog.Multiline = true;
        txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.ReadOnly = true;
        txtLog.Font = new System.Drawing.Font("Consolas", 8.5f);
        txtLog.BackColor = System.Drawing.Color.Black;
        txtLog.ForeColor = System.Drawing.Color.Lime;

        // StatusStrip
        statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { lblStatus });
        statusStrip.Location = new System.Drawing.Point(0, 638);
        statusStrip.Size = new System.Drawing.Size(792, 22);

        lblStatus.Text = "Disconnected";
        lblStatus.ForeColor = System.Drawing.Color.Red;

        // Form
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(792, 660);
        Text = "Mettler Toledo MR304 - Serial Test Tool";
        MinimumSize = new System.Drawing.Size(808, 700);
        Controls.Add(grpConnection);
        Controls.Add(grpResults);
        Controls.Add(grpActive);
        Controls.Add(grpReactive);
        Controls.Add(grpCommands);
        Controls.Add(lblLogHeader);
        Controls.Add(btnClearLog);
        Controls.Add(txtLog);
        Controls.Add(statusStrip);
        FormClosing += new System.Windows.Forms.FormClosingEventHandler(Form1_FormClosing);

        grpConnection.ResumeLayout(false);
        grpResults.ResumeLayout(false);
        grpActive.ResumeLayout(false);
        grpReactive.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)numPollInterval).EndInit();
        grpCommands.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();


    }

    #endregion

    // Connection
    private System.Windows.Forms.GroupBox grpConnection;
    private System.Windows.Forms.Label lblPort;
    private System.Windows.Forms.ComboBox cmbPort;
    private System.Windows.Forms.Button btnRefreshPorts;
    private System.Windows.Forms.Label lblBaudRate;
    private System.Windows.Forms.ComboBox cmbBaudRate;
    private System.Windows.Forms.Button btnConnect;
    private System.Windows.Forms.Button btnDisconnect;

    // Results
    private System.Windows.Forms.GroupBox grpResults;
    private System.Windows.Forms.Label lblNetWeightLabel;
    private System.Windows.Forms.Label lblNetWeight;
    private System.Windows.Forms.Label lblTareWeightLabel;
    private System.Windows.Forms.Label lblTareWeight;
    private System.Windows.Forms.Label lblGrossWeightLabel;
    private System.Windows.Forms.Label lblGrossWeight;
    private System.Windows.Forms.Label lblUnitsLabel;
    private System.Windows.Forms.Label lblUnits;
    private System.Windows.Forms.Label lblStableLabel;
    private System.Windows.Forms.Label lblStable;

    // Active
    private System.Windows.Forms.GroupBox grpActive;
    private System.Windows.Forms.Button btnReadWeightStatus;
    private System.Windows.Forms.Button btnReadNetWeight;
    private System.Windows.Forms.Button btnReadTareWeight;
    private System.Windows.Forms.Button btnReadSerial;
    private System.Windows.Forms.Button btnReadFirmware;

    // Reactive
    private System.Windows.Forms.GroupBox grpReactive;
    private System.Windows.Forms.CheckBox chkAutoPoll;
    private System.Windows.Forms.Label lblPollInterval;
    private System.Windows.Forms.NumericUpDown numPollInterval;
    private System.Windows.Forms.Label lblPollMs;

    // Commands
    private System.Windows.Forms.GroupBox grpCommands;
    private System.Windows.Forms.Button btnZeroStable;
    private System.Windows.Forms.Button btnZeroImmediate;
    private System.Windows.Forms.Button btnTareStable;
    private System.Windows.Forms.Button btnTareImmediate;
    private System.Windows.Forms.Button btnClearTare;

    // Log
    private System.Windows.Forms.Label lblLogHeader;
    private System.Windows.Forms.Button btnClearLog;
    private System.Windows.Forms.TextBox txtLog;

    // Status
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel lblStatus;
}

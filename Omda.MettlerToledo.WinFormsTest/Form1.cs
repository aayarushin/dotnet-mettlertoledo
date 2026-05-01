using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Omda.MettlerToledo;
using Omda.MettlerToledo.Channels;

namespace Omda.MettlerToledo.WinFormsTest;

public partial class Form1 : Form
{
    private MettlerToledoDevice? _device;
    private readonly System.Windows.Forms.Timer _pollTimer;
    private CancellationTokenSource? _cts;
    private bool _pollBusy;

    public Form1()
    {
        InitializeComponent();

        _pollTimer = new System.Windows.Forms.Timer();
        _pollTimer.Tick += PollTimer_Tick;

        RefreshPorts();
    }

    // ?? helpers ????????????????????????????????????????????????????????????

    private void RefreshPorts()
    {
        string current = cmbPort.Text;
        cmbPort.Items.Clear();
        foreach (string port in SerialPort.GetPortNames())
            cmbPort.Items.Add(port);

        if (cmbPort.Items.Contains(current))
            cmbPort.Text = current;
        else if (cmbPort.Items.Count > 0)
            cmbPort.SelectedIndex = 0;
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            Invoke(() => AppendLog(message));
            return;
        }

        string line = $"[{DateTime.Now:HH:mm:ss.fff}] {message}";
        txtLog.AppendText(line + Environment.NewLine);
    }

    private void SetConnectedState(bool connected)
    {
        if (InvokeRequired)
        {
            Invoke(() => SetConnectedState(connected));
            return;
        }

        cmbPort.Enabled = !connected;
        cmbBaudRate.Enabled = !connected;
        btnRefreshPorts.Enabled = !connected;
        btnConnect.Enabled = !connected;
        btnDisconnect.Enabled = connected;

        btnReadWeightStatus.Enabled = connected;
        btnReadNetWeight.Enabled = connected;
        btnReadTareWeight.Enabled = connected;
        btnReadSerial.Enabled = connected;
        btnReadFirmware.Enabled = connected;

        chkAutoPoll.Enabled = connected;
        btnZeroStable.Enabled = connected;
        btnZeroImmediate.Enabled = connected;
        btnTareStable.Enabled = connected;
        btnTareImmediate.Enabled = connected;
        btnClearTare.Enabled = connected;

        lblStatus.Text = connected ? $"Connected – {cmbPort.Text}" : "Disconnected";
        lblStatus.ForeColor = connected ? System.Drawing.Color.Green : System.Drawing.Color.Red;

        if (!connected)
            ClearResults();
    }

    private void ClearResults()
    {
        lblNetWeight.Text = "---";
        lblTareWeight.Text = "---";
        lblGrossWeight.Text = "---";
        lblUnits.Text = "---";
        lblStable.Text = "---";
    }

    private void UpdateResults(ReadWeightAndStatusResult r)
    {
        if (InvokeRequired)
        {
            Invoke(() => UpdateResults(r));
            return;
        }

        lblNetWeight.Text = r.NetWeight.ToString("F4");
        lblTareWeight.Text = r.TareWeight.ToString("F4");
        lblGrossWeight.Text = r.GrossWeight.ToString("F4");
        lblUnits.Text = r.Units;

        lblStable.Text = r.StableStatus ? "Yes" : "No";
        lblStable.ForeColor = r.StableStatus ? System.Drawing.Color.DarkGreen : System.Drawing.Color.OrangeRed;
    }

    private async Task<bool> ExecuteDeviceActionAsync(Func<CancellationToken, Task> action, string description)
    {
        if (_device == null) return false;

        _cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        try
        {
            await action(_cts.Token);
            return true;
        }
        catch (OperationCanceledException)
        {
            AppendLog($"[TIMEOUT] {description} timed out.");
        }
        catch (MettlerToledoException ex)
        {
            string detail = ex.InnerException != null ? $" ? {ex.InnerException.GetType().Name}: {ex.InnerException.Message}" : string.Empty;
            AppendLog($"[ERROR] {description}: {ex.Message}{detail}");
        }
        catch (Exception ex)
        {
            AppendLog($"[ERROR] {description}: {ex.GetType().Name} – {ex.Message}");
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
        }
        return false;
    }

    // ?? connection ?????????????????????????????????????????????????????????

    private async void btnConnect_Click(object sender, EventArgs e)
    {
        if (cmbPort.SelectedItem == null)
        {
            MessageBox.Show("Please select a COM port.", "No Port Selected",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!int.TryParse(cmbBaudRate.Text, out int baudRate))
        {
            MessageBox.Show("Invalid baud rate.", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string portName = cmbPort.SelectedItem.ToString()!;
        btnConnect.Enabled = false;

        var factory = new MettlerToledoDeviceFactory(new ChannelFactory());
        _device = factory.CreateSerialDevice(
            ProtocolType.SICS,
            portName,
            baudRate,
            Parity.None,
            8,
            StopBits.One,
            Handshake.None,
            timeout: 3000,
            retries: 2);

        AppendLog($"Connecting to {portName} @ {baudRate} baud ...");

        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await _device.InitializeAsync(cts.Token);
            AppendLog("Connected successfully.");
            SetConnectedState(true);
        }
        catch (Exception ex)
        {
            AppendLog($"[ERROR] Connect failed: {ex.Message}");
            _device.Dispose();
            _device = null;
            btnConnect.Enabled = true;
        }
    }

    private void btnDisconnect_Click(object sender, EventArgs e)
    {
        _pollTimer.Stop();
        chkAutoPoll.Checked = false;

        _cts?.Cancel();
        _device?.Dispose();
        _device = null;

        AppendLog("Disconnected.");
        SetConnectedState(false);
    }

    private void btnRefreshPorts_Click(object sender, EventArgs e)
    {
        RefreshPorts();
    }

    // ?? active requests ????????????????????????????????????????????????????

    private async void btnReadWeightStatus_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.ReadWeightAndStatusAsync(ct);
            UpdateResults(r);
            AppendLog($"Weight & Status ? Net={r.NetWeight:F4} {r.Units}  Tare={r.TareWeight:F4}  Gross={r.GrossWeight:F4}  Stable={r.StableStatus}  CenterOfZero={r.CenterOfZero}");
        }, "Read Weight & Status");
    }

    private async void btnReadNetWeight_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.ReadNetWeightAsync(ct);
            lblNetWeight.Text = r.NetWeight.ToString("F4");
            lblUnits.Text = r.Units;
            AppendLog($"Net Weight ? {r.NetWeight:F4} {r.Units}");
        }, "Read Net Weight");
    }

    private async void btnReadTareWeight_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.ReadTareWeightAsync(ct);
            lblTareWeight.Text = r.TareWeight.ToString("F4");
            lblUnits.Text = r.Units;
            AppendLog($"Tare Weight ? {r.TareWeight:F4} {r.Units}");
        }, "Read Tare Weight");
    }

    private async void btnReadSerial_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.ReadSerialNumberAsync(ct);
            AppendLog($"Serial Number ? {r.SerialNumber}");
        }, "Read Serial Number");
    }

    private async void btnReadFirmware_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.ReadFirmwareRevisionAsync(ct);
            AppendLog($"Firmware Revision ? {r.Version}");
        }, "Read Firmware Revision");
    }

    // ?? reactive / auto-poll ???????????????????????????????????????????????

    private void chkAutoPoll_CheckedChanged(object sender, EventArgs e)
    {
        if (chkAutoPoll.Checked)
        {
            _pollTimer.Interval = (int)numPollInterval.Value;
            _pollTimer.Start();
            AppendLog($"Auto-poll started (interval={_pollTimer.Interval} ms).");
        }
        else
        {
            _pollTimer.Stop();
            AppendLog("Auto-poll stopped.");
        }
    }

    private void numPollInterval_ValueChanged(object sender, EventArgs e)
    {
        if (_pollTimer.Enabled)
            _pollTimer.Interval = (int)numPollInterval.Value;
    }

    private async void PollTimer_Tick(object sender, EventArgs e)
    {
        if (_pollBusy || _device == null) return;

        _pollBusy = true;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds((int)numPollInterval.Value - 50));
            var r = await _device.ReadWeightAndStatusAsync(cts.Token);
            UpdateResults(r);
            AppendLog($"[POLL] Net={r.NetWeight:F4} {r.Units}  Tare={r.TareWeight:F4}  Gross={r.GrossWeight:F4}  Stable={r.StableStatus}");
        }
        catch (OperationCanceledException)
        {
            // poll took too long – skip silently
        }
        catch (Exception ex)
        {
            AppendLog($"[POLL ERROR] {ex.Message}");
            _pollTimer.Stop();
            chkAutoPoll.Checked = false;
        }
        finally
        {
            _pollBusy = false;
        }
    }

    // ?? commands ???????????????????????????????????????????????????????????

    private async void btnZeroStable_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.SendCommandAsync(CommandType.ZeroStable, ct);
            AppendLog($"Zero (Stable) ? success");
        }, "Zero Stable");
    }

    private async void btnZeroImmediate_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.SendCommandAsync(CommandType.ZeroImmediately, ct);
            AppendLog($"Zero (Immediately) ? success");
        }, "Zero Immediately");
    }

    private async void btnTareStable_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.SendCommandAsync(CommandType.TareStable, ct);
            AppendLog($"Tare (Stable) ? success");
        }, "Tare Stable");
    }

    private async void btnTareImmediate_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.SendCommandAsync(CommandType.TareImmediately, ct);
            AppendLog($"Tare (Immediately) ? success");
        }, "Tare Immediately");
    }

    private async void btnClearTare_Click(object sender, EventArgs e)
    {
        await ExecuteDeviceActionAsync(async ct =>
        {
            var r = await _device!.SendCommandAsync(CommandType.ClearTare, ct);
            AppendLog($"Clear Tare ? success");
        }, "Clear Tare");
    }

    // ?? log ????????????????????????????????????????????????????????????????

    private void btnClearLog_Click(object sender, EventArgs e)
    {
        txtLog.Clear();
    }

    // ?? form closing ???????????????????????????????????????????????????????

    private void Form1_FormClosing(object sender, FormClosingEventArgs e)
    {
        _pollTimer.Stop();
        _cts?.Cancel();
        _device?.Dispose();
    }
}

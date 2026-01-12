using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace RICADO.MettlerToledo.Tests.Emulators
{
    /// <summary>
    /// Emulates a Mettler Toledo device's SICS protocol responses
    /// </summary>
    public class SICSResponseEmulator
    {
        private readonly Dictionary<string, Func<string, string>> _commandHandlers;
        private string _serialNumber = "12345678";
        private string _firmwareVersion = "1.0.0";
        private double _netWeight = 0.0;
        private double _tareWeight = 0.0;
        private string _units = "kg";
        private bool _isStable = true;
        private bool _isCenterOfZero = true;

        public SICSResponseEmulator()
        {
            _commandHandlers = new Dictionary<string, Func<string, string>>
            {
                { "I4", HandleReadSerialNumber },
                { "I3", HandleReadFirmwareRevision },
                { "SI", HandleReadNetWeight },
                { "TA", HandleReadTareWeight },
                { "SIX1", HandleReadWeightAndStatus },
                { "Z", HandleZeroStable },
                { "ZI", HandleZeroImmediately },
                { "T", HandleTareStable },
                { "TI", HandleTareImmediately },
                { "TAC", HandleClearTare }
            };
        }

        /// <summary>
        /// Set the serial number that will be returned by the I4 command
        /// </summary>
        public void SetSerialNumber(string serialNumber)
        {
            if (string.IsNullOrEmpty(serialNumber))
                throw new ArgumentException("Serial number cannot be null or empty");

            if (!Regex.IsMatch(serialNumber, "^[0-9A-Za-z]+$"))
                throw new ArgumentException("Serial number must contain only alphanumeric characters");

            _serialNumber = serialNumber;
        }

        /// <summary>
        /// Set the firmware version that will be returned by the I3 command
        /// </summary>
        public void SetFirmwareVersion(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException("Firmware version cannot be null or empty");

            _firmwareVersion = version;
        }

        /// <summary>
        /// Set the weight values for the device
        /// </summary>
        public void SetWeight(double netWeight, double tareWeight, string units = "kg", bool isStable = true)
        {
            _netWeight = netWeight;
            _tareWeight = tareWeight;
            _units = units;
            _isStable = isStable;
        }

        /// <summary>
        /// Process a SICS command and return the appropriate response
        /// </summary>
        /// <param name="command">The SICS command (e.g., "I4")</param>
        /// <returns>The SICS response with CRLF terminator</returns>
        public string ProcessCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return "ET\r\n"; // Protocol error

            // Remove any trailing CRLF from the command
            command = command.Replace("\r\n", "").Trim();

            // Find the handler for this command
            foreach (var handler in _commandHandlers)
            {
                if (command.Equals(handler.Key, StringComparison.InvariantCultureIgnoreCase))
                {
                    string response = handler.Value(command);
                    return response + "\r\n";
                }
            }

            // Command not supported
            return "ES\r\n";
        }

        /// <summary>
        /// Process a SICS command and return the response as bytes
        /// </summary>
        public byte[] ProcessCommandBytes(byte[] commandBytes)
        {
            string command = Encoding.ASCII.GetString(commandBytes);
            string response = ProcessCommand(command);
            return Encoding.ASCII.GetBytes(response);
        }

        private string HandleReadSerialNumber(string command)
        {
            return $"I4 A \"{_serialNumber}\"";
        }

        private string HandleReadFirmwareRevision(string command)
        {
            return $"I3 A \"{_firmwareVersion}\"";
        }

        private string HandleReadNetWeight(string command)
        {
            string stability = _isStable ? "S" : "D";
            // Format: "S" + stability + spaces + weight + space + units
            // Example: "S S  125.50 kg" or "S D  125.50 kg"
            // The regex expects: ^S [SD]\u0020+([0-9\u002E\\-]+) (.*)$
            // Use invariant culture to ensure dot decimal separator
            return
                $"S {stability}  {_netWeight.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} {_units}";
        }

        private string HandleReadTareWeight(string command)
        {
          return $"TA A  {_tareWeight.ToString("F2", System.Globalization.CultureInfo.InvariantCulture)} {_units}";
 }

        private string HandleReadWeightAndStatus(string command)
        {
            string stability = _isStable ? "S" : "D";
            string centerOfZero = _isCenterOfZero ? "Z" : "N";
            double grossWeight = _netWeight + _tareWeight;

            // Format weights with invariant culture (dot separator) and 9-character right-aligned padding
            string formattedGrossWeight =
                grossWeight.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).PadLeft(9);
            string formattedNetWeight = _netWeight.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                .PadLeft(9);
            string formattedTareWeight = _tareWeight.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
                .PadLeft(9);

            return
                $"SIX1 {stability} 0 {centerOfZero} R R 0 0 0 1 N {formattedGrossWeight} {formattedNetWeight} {formattedTareWeight} {_units}";
        }

        private string HandleZeroStable(string command)
        {
            _tareWeight = 0;
            return "Z A";
        }

        private string HandleZeroImmediately(string command)
        {
            _tareWeight = 0;
            return "ZI A";
        }

        private string HandleTareStable(string command)
        {
            _tareWeight = _netWeight;
            _netWeight = 0;
            return "T A";
        }

        private string HandleTareImmediately(string command)
        {
            _tareWeight = _netWeight;
            _netWeight = 0;
            return "TI A";
        }

        private string HandleClearTare(string command)
        {
            _netWeight = _netWeight + _tareWeight;
            _tareWeight = 0;
            return "TAC A";
        }
    }
}
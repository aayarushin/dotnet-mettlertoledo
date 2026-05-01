using System;
using System.IO.Ports;
using Omda.MettlerToledo.Channels;

namespace Omda.MettlerToledo
{
    /// <summary>
    /// Factory interface for creating MettlerToledoDevice instances
    /// Enables dependency injection for testing and production scenarios
    /// </summary>
    public interface IMettlerToledoDeviceFactory
    {
        /// <summary>
        /// Create an Ethernet-connected Mettler Toledo device
        /// </summary>
        /// <param name="protocolType">Protocol type (e.g., SICS)</param>
        /// <param name="remoteHost">Remote host address</param>
        /// <param name="port">TCP port number</param>
        /// <param name="timeout">Timeout in milliseconds (default: 2000)</param>
        /// <param name="retries">Number of retries (default: 1)</param>
        /// <returns>A configured MettlerToledoDevice instance</returns>
        MettlerToledoDevice CreateEthernetDevice(
            ProtocolType protocolType,
            string remoteHost,
            int port,
            int timeout = 2000,
            int retries = 1);

        /// <summary>
        /// Create a Serial-connected Mettler Toledo device
        /// </summary>
        /// <param name="protocolType">Protocol type (e.g., SICS)</param>
        /// <param name="portName">Serial port name (e.g., COM1, /dev/ttyUSB0)</param>
        /// <param name="baudRate">Baud rate (default: 9600)</param>
        /// <param name="parity">Parity setting (default: None)</param>
        /// <param name="dataBits">Data bits (default: 8)</param>
        /// <param name="stopBits">Stop bits (default: One)</param>
        /// <param name="handshake">Handshake protocol (default: None)</param>
        /// <param name="timeout">Timeout in milliseconds (default: 2000)</param>
        /// <param name="retries">Number of retries (default: 1)</param>
        /// <returns>A configured MettlerToledoDevice instance</returns>
        MettlerToledoDevice CreateSerialDevice(
            ProtocolType protocolType,
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            int timeout = 2000,
            int retries = 1);
    }
}

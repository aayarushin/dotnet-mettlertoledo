using System;
using System.IO.Ports;

namespace RICADO.MettlerToledo.Channels
{
    /// <summary>
    /// Factory interface for creating communication channels
    /// Enables dependency injection for testing and production scenarios
    /// </summary>
    public interface IChannelFactory
    {
        /// <summary>
        /// Create an Ethernet (TCP/IP) channel
        /// </summary>
        /// <param name="remoteHost">Remote host address</param>
        /// <param name="port">TCP port number</param>
        /// <returns>An initialized IChannel for Ethernet communication</returns>
        IChannel CreateEthernetChannel(string remoteHost, int port);

        /// <summary>
        /// Create a Serial (RS-232) channel
        /// </summary>
        /// <param name="portName">Serial port name (e.g., COM1, /dev/ttyUSB0)</param>
        /// <param name="baudRate">Baud rate (e.g., 9600, 115200)</param>
        /// <param name="parity">Parity setting</param>
        /// <param name="dataBits">Data bits (5-8)</param>
        /// <param name="stopBits">Stop bits</param>
        /// <param name="handshake">Handshake protocol</param>
        /// <returns>An initialized IChannel for Serial communication</returns>
        IChannel CreateSerialChannel(
          string portName,
       int baudRate,
            Parity parity,
           int dataBits,
                 StopBits stopBits,
         Handshake handshake);
    }
}

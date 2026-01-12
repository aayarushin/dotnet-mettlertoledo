using System;
using System.IO.Ports;

namespace RICADO.MettlerToledo.Channels
{
    /// <summary>
    /// Production channel factory that creates real Ethernet and Serial channels
    /// </summary>
    internal class ChannelFactory : IChannelFactory
    {
        /// <summary>
        /// Singleton instance for production use
        /// </summary>
        public static readonly ChannelFactory Instance = new ChannelFactory();

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private ChannelFactory()
        {
        }

        /// <summary>
        /// Create an Ethernet (TCP/IP) channel using TcpClient
        /// </summary>
        public IChannel CreateEthernetChannel(string remoteHost, int port)
        {
            return new EthernetChannel(remoteHost, port);
        }

        /// <summary>
        /// Create a Serial (RS-232) channel using SerialPort
        /// </summary>
        public IChannel CreateSerialChannel(
        string portName,
              int baudRate,
          Parity parity,
                 int dataBits,
              StopBits stopBits,
               Handshake handshake)
        {
            return new SerialChannel(portName, baudRate, parity, dataBits, stopBits, handshake);
        }
    }
}

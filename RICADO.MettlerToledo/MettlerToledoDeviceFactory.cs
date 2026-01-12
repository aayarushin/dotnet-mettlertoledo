using System;
using System.IO.Ports;
using RICADO.MettlerToledo.Channels;

namespace RICADO.MettlerToledo
{
    /// <summary>
    /// Production factory for creating MettlerToledoDevice instances
    /// Uses the production ChannelFactory to create real hardware connections
    /// </summary>
    public class MettlerToledoDeviceFactory : IMettlerToledoDeviceFactory
    {
        private readonly IChannelFactory _channelFactory;

        /// <summary>
        /// Singleton instance for production use
        /// Uses the production ChannelFactory internally
        /// </summary>
        public static readonly MettlerToledoDeviceFactory Instance = new MettlerToledoDeviceFactory(ChannelFactory.Instance);

        /// <summary>
        /// Default constructor for production use
        /// Uses the production ChannelFactory singleton
        /// </summary>
        public MettlerToledoDeviceFactory()
        {
            _channelFactory = ChannelFactory.Instance;
        }

        /// <summary>
        /// Internal constructor for dependency injection (used by tests)
        /// </summary>
        /// <param name="channelFactory">Channel factory for creating communication channels</param>
        internal MettlerToledoDeviceFactory(IChannelFactory channelFactory)
        {
            _channelFactory = channelFactory ?? throw new ArgumentNullException(nameof(channelFactory));
        }

        /// <summary>
        /// Create an Ethernet-connected Mettler Toledo device
        /// </summary>
        public MettlerToledoDevice CreateEthernetDevice(
            ProtocolType protocolType,
            string remoteHost,
            int port,
            int timeout = 2000,
            int retries = 1)
        {
            return new MettlerToledoDevice(
                ConnectionMethod.Ethernet,
                protocolType,
                remoteHost,
                port,
                timeout,
                retries,
                _channelFactory);
        }

        /// <summary>
        /// Create a Serial-connected Mettler Toledo device
        /// </summary>
        public MettlerToledoDevice CreateSerialDevice(
            ProtocolType protocolType,
            string portName,
            int baudRate = 9600,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            int timeout = 2000,
            int retries = 1)
        {
            return new MettlerToledoDevice(
                protocolType,
                portName,
                baudRate,
                parity,
                dataBits,
                stopBits,
                handshake,
                timeout,
                retries,
                _channelFactory);
        }
    }
}

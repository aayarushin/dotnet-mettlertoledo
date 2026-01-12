using System;
using System.IO.Ports;
using RICADO.MettlerToledo.Channels;

namespace RICADO.MettlerToledo.Tests.Mocks
{
    /// <summary>
    /// Test channel factory that creates mock Ethernet and Serial channels
    /// Enables realistic behavioral testing without physical hardware
    /// </summary>
    internal class MockChannelFactory : IChannelFactory
    {
        private readonly Func<MockEthernetChannel> _ethernetChannelFactory;
        private readonly Func<int, MockSerialChannel> _serialChannelFactory;

        /// <summary>
        /// Create a mock channel factory with default mock implementations
        /// </summary>
        public MockChannelFactory()
        {
            _ethernetChannelFactory = () => new MockEthernetChannel();
            _serialChannelFactory = (baudRate) => new MockSerialChannel(baudRate);
        }

        /// <summary>
        /// Create a mock channel factory with custom channel factories
        /// This allows tests to configure specific mock behaviors
        /// </summary>
        /// <param name="ethernetChannelFactory">Factory function for creating Ethernet mocks</param>
        /// <param name="serialChannelFactory">Factory function for creating Serial mocks</param>
        public MockChannelFactory(
            Func<MockEthernetChannel> ethernetChannelFactory,
            Func<int, MockSerialChannel> serialChannelFactory)
        {
            _ethernetChannelFactory =
                ethernetChannelFactory ?? throw new ArgumentNullException(nameof(ethernetChannelFactory));
            _serialChannelFactory =
                serialChannelFactory ?? throw new ArgumentNullException(nameof(serialChannelFactory));
        }

        /// <summary>
        /// Create a mock Ethernet channel with TCP/IP behavioral emulation
        /// </summary>
        public IChannel CreateEthernetChannel(string remoteHost, int port)
        {
            return _ethernetChannelFactory();
        }

        /// <summary>
        /// Create a mock Serial channel with RS-232 behavioral emulation
        /// </summary>
        public IChannel CreateSerialChannel(
            string portName,
            int baudRate,
            Parity parity,
            int dataBits,
            StopBits stopBits,
            Handshake handshake)
        {
            return _serialChannelFactory(baudRate);
        }
    }
}
using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Tests for the MettlerToledoDeviceFactory pattern
    /// Demonstrates clean dependency injection using factories
    /// </summary>
    public class DeviceFactoryTests
    {
        [Fact]
        public void ProductionFactory_CreateEthernetDevice_WithCustomParameters()
        {
            // Arrange
            var factory = new MettlerToledoDeviceFactory();

            // Act
            var device = factory.CreateEthernetDevice(
                ProtocolType.SICS,
                "10.0.0.50",
                8080,
                timeout: 5000,
                retries: 3);

            // Assert
            Assert.Equal("10.0.0.50", device.RemoteHost);
            Assert.Equal(8080, device.Port);
            Assert.Equal(5000, device.Timeout);
            Assert.Equal(3, device.Retries);
        }

        [Fact]
        public void ProductionFactory_CreateSerialDevice_WithCustomParameters()
        {
            // Arrange
            var factory = new MettlerToledoDeviceFactory();

            // Act
            var device = factory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM3",
                19200,
                Parity.Even,
                7,
                StopBits.Two,
                Handshake.RequestToSend,
                3000,
                2);

            // Assert
            Assert.Equal("COM3", device.PortName);
            Assert.Equal(19200, device.BaudRate);
            Assert.Equal(Parity.Even, device.Parity);
            Assert.Equal(7, device.DataBits);
            Assert.Equal(StopBits.Two, device.StopBits);
            Assert.Equal(Handshake.RequestToSend, device.Handshake);
            Assert.Equal(3000, device.Timeout);
            Assert.Equal(2, device.Retries);
        }

        [Fact]
        public async Task MockFactory_CreateEthernetDevice_WorksWithMocks()
        {
            // Arrange
            var mockEthernet = new MockEthernetChannel();
            mockEthernet.ConfigureSerialNumberResponse("FACTORY001");
            await mockEthernet.InitializeAsync(2000, CancellationToken.None);

            var mockChannelFactory = new MockChannelFactory(
                () => mockEthernet,
                (baudRate) => new MockSerialChannel(baudRate));

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            // Act
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            await device.InitializeAsync(CancellationToken.None);
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal("FACTORY001", result.SerialNumber);
            Assert.Equal(ConnectionMethod.Ethernet, device.ConnectionMethod);
        }

        [Fact]
        public async Task MockFactory_CreateSerialDevice_WorksWithMocks()
        {
            // Arrange
            var mockSerial = new MockSerialChannel(9600);
            mockSerial.ConfigureSerialNumberResponse("SERIAL001");
            await mockSerial.InitializeAsync(2000, CancellationToken.None);

            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockSerial);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal("SERIAL001", result.SerialNumber);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }

        [Theory]
        [InlineData(1200)]
        [InlineData(9600)]
        [InlineData(115200)]
        public async Task MockFactory_DifferentBaudRates_CreatesCorrectDevices(int baudRate)
        {
            // Arrange
            var mockSerial = new MockSerialChannel(baudRate);
            mockSerial.ConfigureSerialNumberResponse($"BAUD{baudRate}");
            await mockSerial.InitializeAsync(2000, CancellationToken.None);

            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (br) => mockSerial);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                baudRate);

            await device.InitializeAsync(CancellationToken.None);
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal($"BAUD{baudRate}", result.SerialNumber);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }
    }
}
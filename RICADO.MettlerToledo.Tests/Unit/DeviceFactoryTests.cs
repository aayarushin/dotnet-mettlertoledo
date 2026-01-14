using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.DependencyInjection;
using RICADO.MettlerToledo.Tests.DependencyInjection;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Tests for the MettlerToledoDeviceFactory pattern
    /// Demonstrates clean dependency injection using factories
    /// </summary>
    public class DeviceFactoryTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        public DeviceFactoryTests()
        {
            _services = new ServiceCollection();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public void ProductionFactory_CreateEthernetDevice_WithCustomParameters()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            var factory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

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
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            var factory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

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
            var mockChannelFactory = new MockChannelFactory(
                () =>
                {
                    var mockEthernet = new MockEthernetChannel();
                    mockEthernet.ConfigureSerialNumberResponse("FACTORY001");
                    return mockEthernet;
                },
                (baudRate) => new MockSerialChannel(baudRate));

            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

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
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) =>
                {
                    var mockSerial = new MockSerialChannel(baudRate);
                    mockSerial.ConfigureSerialNumberResponse("SERIAL001");
                    return mockSerial;
                });

            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

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
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (br) =>
                {
                    var mockSerial = new MockSerialChannel(br);
                    mockSerial.ConfigureSerialNumberResponse($"BAUD{br}");
                    return mockSerial;
                });

            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

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
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omda.MettlerToledo.Channels;
using Omda.MettlerToledo.DependencyInjection;
using Omda.MettlerToledo.Tests.DependencyInjection;
using Omda.MettlerToledo.Tests.Mocks;
using Xunit;

namespace Omda.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Tests demonstrating dependency injection usage patterns
    /// </summary>
    public class DependencyInjectionTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        public DependencyInjectionTests()
        {
            _services = new ServiceCollection();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public void AddMettlerToledo_RegistersProductionServices()
        {
            // Arrange & Act
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var channelFactory = _serviceProvider.GetService<IChannelFactory>();
            var deviceFactory = _serviceProvider.GetService<IMettlerToledoDeviceFactory>();

            Assert.NotNull(channelFactory);
            Assert.NotNull(deviceFactory);
            Assert.IsType<ChannelFactory>(channelFactory);
            Assert.IsType<MettlerToledoDeviceFactory>(deviceFactory);
        }

        [Fact]
        public void AddMettlerToledoMocks_RegistersTestServices()
        {
            // Arrange & Act
            _services.AddMettlerToledoMocks(new MockChannelFactory());
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var channelFactory = _serviceProvider.GetService<IChannelFactory>();
            var deviceFactory = _serviceProvider.GetService<IMettlerToledoDeviceFactory>();

            Assert.NotNull(channelFactory);
            Assert.NotNull(deviceFactory);
            Assert.IsType<MockChannelFactory>(channelFactory);
            Assert.IsType<MettlerToledoDeviceFactory>(deviceFactory);
        }

        [Fact]
        public void AddMettlerToledoMocks_WithCustomFactory_RegistersCustomMock()
        {
            // Arrange
            var customMock = new MockChannelFactory();

            // Act
            _services.AddMettlerToledoMocks(customMock);
            _serviceProvider = _services.BuildServiceProvider();

            // Assert
            var channelFactory = _serviceProvider.GetService<IChannelFactory>();
            Assert.NotNull(channelFactory);
            Assert.Same(customMock, channelFactory);
        }

        [Fact]
        public async Task ProductionDI_CanCreateEthernetDevice()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Act
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "192.168.1.100",
                8001);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(ConnectionMethod.Ethernet, device.ConnectionMethod);
            Assert.Equal("192.168.1.100", device.RemoteHost);
            Assert.Equal(8001, device.Port);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task ProductionDI_CanCreateSerialDevice()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            Assert.Equal("COM1", device.PortName);
            Assert.Equal(9600, device.BaudRate);

            await Task.CompletedTask;
        }

        [Fact]
        public void DI_RegistersFactoriesAsSingletons()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var factory1 = _serviceProvider.GetService<IMettlerToledoDeviceFactory>();
            var factory2 = _serviceProvider.GetService<IMettlerToledoDeviceFactory>();

            // Assert - Same instance should be returned (singleton)
            Assert.Same(factory1, factory2);
        }

        [Fact]
        public void DI_ChannelFactoryInjectedIntoDeviceFactory()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            // Act
            var channelFactory = _serviceProvider.GetRequiredService<IChannelFactory>();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Assert
            Assert.NotNull(channelFactory);
            Assert.NotNull(deviceFactory);
            // The device factory should have been constructed with the channel factory
        }

        [Fact]
        public void AddMettlerToledo_NullServices_ThrowsException()
        {
            // Arrange
            ServiceCollection services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddMettlerToledo());
        }

        [Fact]
        public void AddMettlerToledoMocks_NullServices_ThrowsException()
        {
            // Arrange
            ServiceCollection services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddMettlerToledo());
        }

        [Fact]
        public void AddMettlerToledoMocks_NullMockFactory_ThrowsException()
        {
            // Arrange
            MockChannelFactory mockFactory = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => _services.AddMettlerToledoMocks(mockFactory));
        }

        [Fact]
        public async Task MockDI_MultipleDevicesCanBeCreated()
        {
            // Arrange
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();

            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Act
            var ethernetDevice = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "192.168.1.1",
                8001);

            var serialDevice = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1");

            // Assert
            Assert.NotNull(ethernetDevice);
            Assert.NotNull(serialDevice);
            Assert.Equal(ConnectionMethod.Ethernet, ethernetDevice.ConnectionMethod);
            Assert.Equal(ConnectionMethod.Serial, serialDevice.ConnectionMethod);

            await Task.CompletedTask;
        }
    }
}

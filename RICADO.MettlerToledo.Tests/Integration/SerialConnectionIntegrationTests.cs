using System;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo.Channels;
using RICADO.MettlerToledo.Tests.Emulators;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Integration
{
    /// <summary>
    /// Integration tests for RS-232 Serial connection method
    /// </summary>
    public class SerialConnectionIntegrationTests
    {
        [Fact]
        public async Task SerialDevice_WithMockChannel_ReadSerialNumber_Success()
        {
            // Arrange
            string expectedSerialNumber = "SERIAL001";

            // Use Serial-specific mock
            var mockChannel = new MockSerialChannel();
            mockChannel.ConfigureSerialNumberResponse(expectedSerialNumber);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockChannel);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, result.SerialNumber);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            // Serial channel: PacketsReceived equals BytesReceived (byte-by-byte)
            Assert.True(result.PacketsReceived >= 1);
        }

        [Fact]
        public async Task SerialDevice_WithEmulator_ReadSerialNumber_Success()
        {
            // Arrange
            string expectedSerialNumber = "RS232TEST";

            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber(expectedSerialNumber);

            var mockChannel = CreateSerialChannelWithEmulator(emulator);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockChannel);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, result.SerialNumber);
        }

        [Fact]
        public async Task SerialDevice_WithEmulator_ReadFirmwareRevision_Success()
        {
            // Arrange
            string expectedVersion = "3.1.4";

            var emulator = new SICSResponseEmulator();
            emulator.SetFirmwareVersion(expectedVersion);

            var mockChannel = CreateSerialChannelWithEmulator(emulator);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockChannel);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadFirmwareRevisionAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedVersion, result.Version);
        }

        [Theory]
        [InlineData("COM1", 9600, Parity.None, 8, StopBits.One, Handshake.None)]
        [InlineData("COM3", 19200, Parity.Even, 7, StopBits.Two, Handshake.RequestToSend)]
        [InlineData("/dev/ttyUSB0", 115200, Parity.Odd, 8, StopBits.One, Handshake.XOnXOff)]
        public async Task SerialDevice_DifferentConfigurations_CanBeCreated(
            string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            // Arrange
            // Use production factory for configuration tests (no mocking needed)
            var deviceFactory = new MettlerToledoDeviceFactory();

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                portName,
                baudRate,
                parity,
                dataBits,
                stopBits,
                handshake);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            Assert.Equal(portName, device.PortName);
            Assert.Equal(baudRate, device.BaudRate);
            Assert.Equal(parity, device.Parity);
            Assert.Equal(dataBits, device.DataBits);
            Assert.Equal(stopBits, device.StopBits);
            Assert.Equal(handshake, device.Handshake);

            await Task.CompletedTask; // Suppress async warning
        }

        [Fact]
        public async Task SerialDevice_MultipleCommands_WithEmulator_Success()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("MULTI001");
            emulator.SetFirmwareVersion("2.0.0");

            var mockChannel = CreateSerialChannelWithEmulator(emulator);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockChannel);

            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);
            
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var serialResult = await device.ReadSerialNumberAsync(CancellationToken.None);
            var firmwareResult = await device.ReadFirmwareRevisionAsync(CancellationToken.None);

            // Assert
            Assert.Equal("MULTI001", serialResult.SerialNumber);
            Assert.Equal("2.0.0", firmwareResult.Version);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }

        [Theory]
        [InlineData(1200)]
        [InlineData(2400)]
        [InlineData(4800)]
        [InlineData(9600)]
        [InlineData(19200)]
        [InlineData(38400)]
        [InlineData(57600)]
        [InlineData(115200)]
        public async Task SerialDevice_CommonBaudRates_CanReadSerialNumber(int baudRate)
        {
            // Arrange
            string expectedSerialNumber = $"BAUD{baudRate}";

            // Use Serial-specific mock with the specified baud rate
            var mockChannel = new MockSerialChannel(baudRate);
            mockChannel.ConfigureSerialNumberResponse(expectedSerialNumber);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (br) => mockChannel);

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                baudRate);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, result.SerialNumber);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }

        [Fact]
        public async Task SerialDevice_vs_EthernetDevice_BothWork()
        {
            // Arrange
            string serialSN = "SERIAL123";
            string ethernetSN = "ETHERNET456";

            
            var serialMock = new MockSerialChannel();
            serialMock.ConfigureSerialNumberResponse(serialSN);
            await serialMock.InitializeAsync(2000, CancellationToken.None);

            var serialChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => serialMock);

            var serialDeviceFactory = new MettlerToledoDeviceFactory(serialChannelFactory);

            var serialDevice = serialDeviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await serialDevice.InitializeAsync(CancellationToken.None);

            
            var ethernetMock = new MockEthernetChannel();
            ethernetMock.ConfigureSerialNumberResponse(ethernetSN);
            await ethernetMock.InitializeAsync(2000, CancellationToken.None);

            var ethernetChannelFactory = new MockChannelFactory(
                () => ethernetMock,
                (baudRate) => new MockSerialChannel(baudRate));

            var ethernetDeviceFactory = new MettlerToledoDeviceFactory(ethernetChannelFactory);

            var ethernetDevice = ethernetDeviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            await ethernetDevice.InitializeAsync(CancellationToken.None);

            // Act
            var serialResult = await serialDevice.ReadSerialNumberAsync(CancellationToken.None);
            var ethernetResult = await ethernetDevice.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(serialSN, serialResult.SerialNumber);
            Assert.Equal(ConnectionMethod.Serial, serialDevice.ConnectionMethod);

            Assert.Equal(ethernetSN, ethernetResult.SerialNumber);
            Assert.Equal(ConnectionMethod.Ethernet, ethernetDevice.ConnectionMethod);

            // Verify behavioral difference: Serial has more packets (byte-by-byte)
            Assert.True(serialResult.PacketsReceived >= ethernetResult.PacketsReceived);
        }

        [Fact]
        public async Task SerialDevice_Dispose_CanBeCalledMultipleTimes()
        {
            // Arrange
            var mockChannel = new MockSerialChannel();
            mockChannel.ConfigureSerialNumberResponse("TEST");
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            var mockChannelFactory = new MockChannelFactory(
                () => new MockEthernetChannel(),
                (baudRate) => mockChannel);

            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                9600);

            await device.InitializeAsync(CancellationToken.None);

            // Act & Assert
            device.Dispose();
            var exception = Record.Exception(() => device.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void SerialDevice_CanCreateWithMinimalParameters()
        {
            // Arrange
            // Use production factory for creation tests
            var deviceFactory = new MettlerToledoDeviceFactory();

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1");

            // Assert
            Assert.NotNull(device);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            Assert.Equal("COM1", device.PortName);
            Assert.Equal(9600, device.BaudRate); // Default
            Assert.Equal(Parity.None, device.Parity); // Default
            Assert.Equal(8, device.DataBits); // Default
            Assert.Equal(StopBits.One, device.StopBits); // Default
            Assert.Equal(Handshake.None, device.Handshake); // Default
        }

        [Fact]
        public async Task SerialDevice_PropertiesAreReadable_AfterCreation()
        {
            // Arrange
            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();

            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM5",
                19200,
                Parity.Even,
                7,
                StopBits.Two,
                Handshake.RequestToSend,
                timeout: 3000,
                retries: 2);

            // Act & Assert
            Assert.Equal("COM5", device.PortName);
            Assert.Equal(19200, device.BaudRate);
            Assert.Equal(Parity.Even, device.Parity);
            Assert.Equal(7, device.DataBits);
            Assert.Equal(StopBits.Two, device.StopBits);
            Assert.Equal(Handshake.RequestToSend, device.Handshake);
            Assert.Equal(3000, device.Timeout);
            Assert.Equal(2, device.Retries);
            Assert.Equal(ProtocolType.SICS, device.ProtocolType);
            Assert.False(device.IsInitialized);

            await Task.CompletedTask; // Suppress async warning
        }

        [Theory]
        [InlineData("COM1")]
        [InlineData("COM10")]
        [InlineData("COM255")]
        [InlineData("/dev/ttyUSB0")]
        [InlineData("/dev/ttyS0")]
        [InlineData("/dev/cu.usbserial")]
        [InlineData("\\\\.\\COM10")] // Windows extended format
        public void SerialDevice_VariousPortNames_AreAccepted(string portName)
        {
            // Arrange
            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();

            // Act
            var device = deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                portName);

            // Assert
            Assert.Equal(portName, device.PortName);
        }

        // Helper method
        private MockSerialChannel CreateSerialChannelWithEmulator(SICSResponseEmulator emulator)
        {
            var mockChannel = new MockSerialChannel();

            // Configure responses from emulator
            string i4Response = emulator.ProcessCommand("I4").Replace("\r\n", "");
            mockChannel.ConfigureResponse("I4", i4Response);

            string i3Response = emulator.ProcessCommand("I3").Replace("\r\n", "");
            mockChannel.ConfigureResponse("I3", i3Response);

            return mockChannel;
        }
    }
}
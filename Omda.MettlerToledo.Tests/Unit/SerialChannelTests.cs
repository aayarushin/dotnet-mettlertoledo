using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Omda.MettlerToledo;
using Omda.MettlerToledo.Channels;
using Omda.MettlerToledo.DependencyInjection;
using Omda.MettlerToledo.Tests.DependencyInjection;
using Omda.MettlerToledo.Tests.Emulators;
using Omda.MettlerToledo.Tests.Mocks;
using Xunit;

namespace Omda.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Unit tests specifically for the RS-232 Serial Channel implementation
    /// </summary>
    public class SerialChannelTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider _serviceProvider;
        private IMettlerToledoDeviceFactory _deviceFactory;
        private IChannelFactory _channelFactory;

        public SerialChannelTests()
        {
            _services = new ServiceCollection();
            _services.AddMettlerToledo();
            _serviceProvider = _services.BuildServiceProvider();
            _deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();
            _channelFactory = _serviceProvider.GetRequiredService<IChannelFactory>();
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public void SerialChannel_Constructor_SetsProperties()
        {
            // Arrange & Act
            var channel = _channelFactory.CreateSerialChannel(
                portName: "COM1",
                baudRate: 9600,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One,
                handshake: Handshake.None) as SerialChannel;

            // Assert
            Assert.Equal("COM1", channel.PortName);
            Assert.Equal(9600, channel.BaudRate);
            Assert.Equal(Parity.None, channel.Parity);
            Assert.Equal(8, channel.DataBits);
            Assert.Equal(StopBits.One, channel.StopBits);
            Assert.Equal(Handshake.None, channel.Handshake);
        }

        [Theory]
        [InlineData("COM1", 9600, Parity.None, 8, StopBits.One, Handshake.None)]
        [InlineData("COM3", 19200, Parity.Even, 7, StopBits.Two, Handshake.RequestToSend)]
        [InlineData("/dev/ttyUSB0", 115200, Parity.Odd, 8, StopBits.One, Handshake.XOnXOff)]
        public void SerialChannel_DifferentConfigurations_SetsPropertiesCorrectly(
            string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            // Arrange & Act
            var channel = _channelFactory.CreateSerialChannel(
                portName, baudRate, parity, dataBits, stopBits, handshake) as SerialChannel;

            // Assert
            Assert.Equal(portName, channel.PortName);
            Assert.Equal(baudRate, channel.BaudRate);
            Assert.Equal(parity, channel.Parity);
            Assert.Equal(dataBits, channel.DataBits);
            Assert.Equal(stopBits, channel.StopBits);
            Assert.Equal(handshake, channel.Handshake);
        }

        [Fact]
        public void SerialChannel_Dispose_DoesNotThrow()
        {
            // Arrange
            var channel = _channelFactory.CreateSerialChannel(
                "COM1", 9600, Parity.None, 8, StopBits.One, Handshake.None) as SerialChannel;

            // Act & Assert
            var exception = Record.Exception(() => channel.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public void SerialChannel_DoubleDispose_DoesNotThrow()
        {
            // Arrange
            var channel = _channelFactory.CreateSerialChannel(
                "COM1", 9600, Parity.None, 8, StopBits.One, Handshake.None) as SerialChannel;

            // Act & Assert
            channel.Dispose();
            var exception = Record.Exception(() => channel.Dispose());
            Assert.Null(exception);
        }

        [Fact]
        public async Task MettlerToledoDevice_SerialConstructor_SetsPropertiesCorrectly()
        {
            // Arrange & Act
            var device = _deviceFactory.CreateSerialDevice(
                protocolType: ProtocolType.SICS,
                portName: "COM1",
                baudRate: 9600,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One,
                handshake: Handshake.None,
                timeout: 2000,
                retries: 1);

            // Assert
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            Assert.Equal("COM1", device.PortName);
            Assert.Equal(9600, device.BaudRate);
            Assert.Equal(Parity.None, device.Parity);
            Assert.Equal(8, device.DataBits);
            Assert.Equal(StopBits.One, device.StopBits);
            Assert.Equal(Handshake.None, device.Handshake);
            Assert.Equal(2000, device.Timeout);
            Assert.Equal(1, device.Retries);
            Assert.Equal(ProtocolType.SICS, device.ProtocolType);

            await Task.CompletedTask;
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void MettlerToledoDevice_SerialConstructor_InvalidPortName_ThrowsException(string invalidPortName)
        {
            // Arrange & Act & Assert
            if (invalidPortName == null)
            {
                Assert.Throws<ArgumentNullException>(() => _deviceFactory.CreateSerialDevice(
                    ProtocolType.SICS, invalidPortName, 9600));
            }
            else
            {
                Assert.Throws<ArgumentException>(() => _deviceFactory.CreateSerialDevice(
                    ProtocolType.SICS, invalidPortName, 9600));
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-9600)]
        public void MettlerToledoDevice_SerialConstructor_InvalidBaudRate_ThrowsException(int invalidBaudRate)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", invalidBaudRate));
        }

        [Theory]
        [InlineData(4)]
        [InlineData(9)]
        [InlineData(0)]
        public void MettlerToledoDevice_SerialConstructor_InvalidDataBits_ThrowsException(int invalidDataBits)
        {
            // Arrange & Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", 9600, Parity.None, invalidDataBits));
        }

        [Theory]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void MettlerToledoDevice_SerialConstructor_ValidDataBits_DoesNotThrow(int validDataBits)
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", 9600, Parity.None, validDataBits));
            Assert.Null(exception);
        }

        [Fact]
        public void MettlerToledoDevice_SerialConstructor_DefaultValues_AreCorrect()
        {
            // Arrange & Act
            var device = _deviceFactory.CreateSerialDevice(ProtocolType.SICS, "COM1");

            // Assert
            Assert.Equal(9600, device.BaudRate);
            Assert.Equal(Parity.None, device.Parity);
            Assert.Equal(8, device.DataBits);
            Assert.Equal(StopBits.One, device.StopBits);
            Assert.Equal(Handshake.None, device.Handshake);
            Assert.Equal(2000, device.Timeout);
            Assert.Equal(1, device.Retries);
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
        public void MettlerToledoDevice_SerialConstructor_CommonBaudRates_Work(int baudRate)
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", baudRate));
            Assert.Null(exception);
        }

        [Fact]
        public void ConnectionMethod_Serial_EnumValue_Exists()
        {
            // Arrange & Act
            var serialMethod = ConnectionMethod.Serial;

            // Assert
            Assert.Equal(1, (int)serialMethod);
            Assert.True(Enum.IsDefined(typeof(ConnectionMethod), serialMethod));
        }

        [Fact]
        public void ConnectionMethod_Ethernet_EnumValue_Exists()
        {
            // Arrange & Act
            var ethernetMethod = ConnectionMethod.Ethernet;

            // Assert
            Assert.Equal(0, (int)ethernetMethod);
            Assert.True(Enum.IsDefined(typeof(ConnectionMethod), ethernetMethod));
        }

        [Fact]
        public void SerialChannel_Properties_AreInternal()
        {
            // Arrange
            var channel = _channelFactory.CreateSerialChannel(
                "COM1", 9600, Parity.None, 8, StopBits.One, Handshake.None) as SerialChannel;

            // Assert
            Assert.Equal("COM1", channel.PortName);
            Assert.Equal(9600, channel.BaudRate);
            Assert.Equal(Parity.None, channel.Parity);
            Assert.Equal(8, channel.DataBits);
            Assert.Equal(StopBits.One, channel.StopBits);
            Assert.Equal(Handshake.None, channel.Handshake);

            var portNameProperty = typeof(SerialChannel).GetProperty("PortName",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.Null(portNameProperty);

            var internalPortNameProperty = typeof(SerialChannel).GetProperty("PortName",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(internalPortNameProperty);
        }

        [Fact]
        public async Task MettlerToledoDevice_SerialDevice_CanBeCreatedAndDisposed()
        {
            // Arrange
            var device = _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "COM1",
                baudRate: 9600,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One,
                handshake: Handshake.None);

            // Act & Assert
            var exception = Record.Exception(() => device.Dispose());
            Assert.Null(exception);

            await Task.CompletedTask;
        }

        [Fact]
        public async Task MettlerToledoDevice_SerialConstructor_WithAllParameters_CreatesDevice()
        {
            // Arrange & Act
            var device = _deviceFactory.CreateSerialDevice(
                protocolType: ProtocolType.SICS,
                portName: "COM3",
                baudRate: 19200,
                parity: Parity.Even,
                dataBits: 7,
                stopBits: StopBits.Two,
                handshake: Handshake.RequestToSend,
                timeout: 5000,
                retries: 3);

            // Assert
            Assert.NotNull(device);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
            Assert.Equal("COM3", device.PortName);
            Assert.Equal(19200, device.BaudRate);
            Assert.Equal(Parity.Even, device.Parity);
            Assert.Equal(7, device.DataBits);
            Assert.Equal(StopBits.Two, device.StopBits);
            Assert.Equal(Handshake.RequestToSend, device.Handshake);
            Assert.Equal(5000, device.Timeout);
            Assert.Equal(3, device.Retries);

            await Task.CompletedTask;
        }

        [Theory]
        [InlineData(Parity.None)]
        [InlineData(Parity.Odd)]
        [InlineData(Parity.Even)]
        [InlineData(Parity.Mark)]
        [InlineData(Parity.Space)]
        public void MettlerToledoDevice_SerialConstructor_AllParityOptions_Work(Parity parity)
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", 9600, parity));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(StopBits.None)]
        [InlineData(StopBits.One)]
        [InlineData(StopBits.OnePointFive)]
        [InlineData(StopBits.Two)]
        public void MettlerToledoDevice_SerialConstructor_AllStopBitsOptions_Work(StopBits stopBits)
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", 9600, Parity.None, 8, stopBits));
            Assert.Null(exception);
        }

        [Theory]
        [InlineData(Handshake.None)]
        [InlineData(Handshake.XOnXOff)]
        [InlineData(Handshake.RequestToSend)]
        [InlineData(Handshake.RequestToSendXOnXOff)]
        public void MettlerToledoDevice_SerialConstructor_AllHandshakeOptions_Work(Handshake handshake)
        {
            // Arrange & Act & Assert
            var exception = Record.Exception(() => _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS, "COM1", 9600, Parity.None, 8, StopBits.One, handshake));
            Assert.Null(exception);
        }

        [Fact]
        public void MettlerToledoDevice_SerialConstructor_LinuxPortName_Works()
        {
            // Arrange & Act
            var device = _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "/dev/ttyUSB0");

            // Assert
            Assert.Equal("/dev/ttyUSB0", device.PortName);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }

        [Fact]
        public void MettlerToledoDevice_SerialConstructor_MacPortName_Works()
        {
            // Arrange & Act
            var device = _deviceFactory.CreateSerialDevice(
                ProtocolType.SICS,
                "/dev/cu.usbserial");

            // Assert
            Assert.Equal("/dev/cu.usbserial", device.PortName);
            Assert.Equal(ConnectionMethod.Serial, device.ConnectionMethod);
        }
    }
}
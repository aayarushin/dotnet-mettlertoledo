using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using RICADO.MettlerToledo.Channels;
using RICADO.MettlerToledo.SICS;
using RICADO.MettlerToledo.Tests.Emulators;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Unit tests for the ReadSerialNumber SICS command
    /// </summary>
    public class ReadSerialNumberTests
    {
        [Fact]
        public void ReadSerialNumberRequest_BuildsCorrectMessage()
        {
            // Arrange
            // Use production factory for message building tests
            var deviceFactory = new MettlerToledoDeviceFactory();
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            var request = ReadSerialNumberRequest.CreateNew(device);

            // Act
#if NETSTANDARD
  byte[] message = request.BuildMessage();
#else
            ReadOnlyMemory<byte> message = request.BuildMessage();
#endif

            // Assert
            string messageString = Encoding.ASCII.GetString(message.ToArray());
            Assert.Equal("I4\r\n", messageString);
        }

        [Theory]
        [InlineData("12345678")]
        [InlineData("ABC123XYZ")]
        [InlineData("SN987654")]
        [InlineData("MT2024001")]
        public async Task ReadSerialNumber_WithMockChannel_ReturnsExpectedSerialNumber(string expectedSerialNumber)
        {
            // Arrange - Use Ethernet-specific mock for Ethernet device
            var mockChannel = new MockEthernetChannel();
            mockChannel.ConfigureSerialNumberResponse(expectedSerialNumber);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => mockChannel,
                (baudRate) => new MockSerialChannel(baudRate));

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedSerialNumber, result.SerialNumber);
            Assert.True(result.BytesSent > 0);
            Assert.True(result.BytesReceived > 0);
            Assert.Equal(1, result.PacketsSent);
            Assert.Equal(1, result.PacketsReceived); // Ethernet: 1 packet
        }

        [Fact]
        public async Task ReadSerialNumber_WithEmulator_ReturnsPresetSerialNumber()
        {
            // Arrange
            string expectedSerialNumber = "TEST123456";
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber(expectedSerialNumber);

            var mockChannel = CreateEthernetChannelWithEmulator(emulator);
            await mockChannel.InitializeAsync(2000, CancellationToken.None);

            
            var mockChannelFactory = new MockChannelFactory(
                () => mockChannel,
                (baudRate) => new MockSerialChannel(baudRate));

            
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory);

            
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, result.SerialNumber);
        }

        [Fact]
        public void ReadSerialNumberResponse_ParsesValidResponse()
        {
            // Arrange
            string serialNumber = "ABCD1234";
            string responseString = $"I4 A \"{serialNumber}\"\r\n";
            byte[] responseMessage = Encoding.ASCII.GetBytes(responseString);

            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            var request = ReadSerialNumberRequest.CreateNew(device);

            // Act
#if NETSTANDARD
            var response = request.UnpackResponseMessage(responseMessage);
#else
            var response = request.UnpackResponseMessage(new Memory<byte>(responseMessage));
#endif

            // Assert
            Assert.NotNull(response);
            Assert.Equal(serialNumber, response.SerialNumber);
        }

        [Fact]
        public void ReadSerialNumberResponse_ThrowsOnInvalidFormat()
        {
            // Arrange
            string invalidResponse = "I4 X \"12345\"\r\n"; // 'X' instead of 'A'
            byte[] responseMessage = Encoding.ASCII.GetBytes(invalidResponse);

            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            var request = ReadSerialNumberRequest.CreateNew(device);

            // Act & Assert
#if NETSTANDARD
        Assert.Throws<SICSException>(() => request.UnpackResponseMessage(responseMessage));
#else
            Assert.Throws<SICSException>(() => request.UnpackResponseMessage(new Memory<byte>(responseMessage)));
#endif
        }

        [Fact]
        public void ReadSerialNumberResponse_ThrowsOnMissingETX()
        {
            // Arrange
            string responseWithoutETX = "I4 A \"12345\""; // Missing CRLF
            byte[] responseMessage = Encoding.ASCII.GetBytes(responseWithoutETX);

            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            var request = ReadSerialNumberRequest.CreateNew(device);

            // Act & Assert
#if NETSTANDARD
  Assert.Throws<SICSException>(() => request.UnpackResponseMessage(responseMessage));
#else
            Assert.Throws<SICSException>(() => request.UnpackResponseMessage(new Memory<byte>(responseMessage)));
#endif
        }

        [Theory]
        [InlineData("ES\r\n", "The SICS Command is not Supported by the Device")]
        [InlineData("ET\r\n", "SICS Communication or Protocol Error")]
        [InlineData("EL\r\n", "The SICS Command Parameters were Incorrect")]
        [InlineData("EI\r\n", "The SICS Command cannot be Executed at this Time")]
        public void ReadSerialNumberResponse_ThrowsOnErrorResponses(string errorResponse, string expectedMessagePart)
        {
            // Arrange
            byte[] responseMessage = Encoding.ASCII.GetBytes(errorResponse);

            // Use production factory
            var deviceFactory = new MettlerToledoDeviceFactory();
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            var request = ReadSerialNumberRequest.CreateNew(device);

            // Act & Assert
#if NETSTANDARD
            var exception = Assert.Throws<SICSException>(() => request.UnpackResponseMessage(responseMessage));
#else
            var exception =
                Assert.Throws<SICSException>(() => request.UnpackResponseMessage(new Memory<byte>(responseMessage)));
#endif
            Assert.Contains(expectedMessagePart, exception.Message);
        }

        [Fact]
        public async Task ReadSerialNumber_WithMoq_VerifiesChannelInteraction()
        {
            // Arrange
            string expectedSerialNumber = "MOQ123";
            var mockChannel = new Mock<IChannel>();

            // Setup the mock to return a proper SICS response
            string responseString = $"I4 A \"{expectedSerialNumber}\"\r\n";
            byte[] responseBytes = Encoding.ASCII.GetBytes(responseString);

            mockChannel
                .Setup(m => m.InitializeAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

#if NETSTANDARD
            mockChannel
                .Setup(m => m.ProcessMessageAsync(
                    It.IsAny<byte[]>(),
                    It.IsAny<ProtocolType>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessMessageResult
                {
                    BytesSent = 4,
                    PacketsSent = 1,
                    BytesReceived = responseBytes.Length,
                    PacketsReceived = 1,
                    Duration = 5.0,
                    ResponseMessage = responseBytes
                });
#else
            mockChannel
                .Setup(m => m.ProcessMessageAsync(
                    It.IsAny<ReadOnlyMemory<byte>>(),
                    It.IsAny<ProtocolType>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ProcessMessageResult
                {
                    BytesSent = 4,
                    PacketsSent = 1,
                    BytesReceived = responseBytes.Length,
                    PacketsReceived = 1,
                    Duration = 5.0,
                    ResponseMessage = new Memory<byte>(responseBytes)
                });
#endif

            // Create a mock channel factory that returns the Moq mock
            var mockChannelFactory = new Mock<IChannelFactory>();
            mockChannelFactory
                .Setup(f => f.CreateEthernetChannel(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(mockChannel.Object);

            // Use MettlerToledoDeviceFactory with mock channel factory
            var deviceFactory = new MettlerToledoDeviceFactory(mockChannelFactory.Object);
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, result.SerialNumber);

            // Verify the channel was called correctly
            mockChannel.Verify(
                m => m.ProcessMessageAsync(
#if NETSTANDARD
    It.Is<byte[]>(b => Encoding.ASCII.GetString(b) == "I4\r\n"),
#else
                    It.Is<ReadOnlyMemory<byte>>(b => Encoding.ASCII.GetString(b.ToArray()) == "I4\r\n"),
#endif
                    ProtocolType.SICS,
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);

            // Verify the factory was called to create the channel
            mockChannelFactory.Verify(
                f => f.CreateEthernetChannel("127.0.0.1", 8001),
                Times.Once);
        }

        // Helper method to create an Ethernet channel that uses the emulator
        private MockEthernetChannel CreateEthernetChannelWithEmulator(SICSResponseEmulator emulator)
        {
            var mockChannel = new MockEthernetChannel();

            // Configure the mock channel to use the emulator for I4 command
            string i4Response = emulator.ProcessCommand("I4").Replace("\r\n", "");
            mockChannel.ConfigureResponse("I4", i4Response);

            return mockChannel;
        }
    }
}
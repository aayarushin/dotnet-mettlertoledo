using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Tests that demonstrate and validate the behavioral differences
    /// between Ethernet (TCP/IP) and Serial (RS-232) channels
    /// </summary>
    public class ChannelBehaviorDifferenceTests
    {
        [Fact]
        public async Task EthernetChannel_HasLowerLatency_ThanSerialChannel()
        {
            // Arrange
            var ethernetChannel = new MockEthernetChannel(latencyMs: 10);
            var serialChannel = new MockSerialChannel(baudRate: 9600);

            ethernetChannel.ConfigureSerialNumberResponse("ETH001");
            serialChannel.ConfigureSerialNumberResponse("SER001");

            await ethernetChannel.InitializeAsync(2000, CancellationToken.None);
            await serialChannel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var ethernetResult = await ethernetChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            var serialResult = await serialChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert
            // Ethernet should generally be faster for small messages
            Assert.True(ethernetResult.Duration < serialResult.Duration,
                $"Ethernet ({ethernetResult.Duration}ms) should be faster than Serial ({serialResult.Duration}ms)");
        }

        [Fact]
        public async Task SerialChannel_HasHigherPacketCount_ThanEthernet()
        {
            // Arrange
            var ethernetChannel = new MockEthernetChannel();
            var serialChannel = new MockSerialChannel();

            ethernetChannel.ConfigureSerialNumberResponse("ETH001");
            serialChannel.ConfigureSerialNumberResponse("SER001");

            await ethernetChannel.InitializeAsync(2000, CancellationToken.None);
            await serialChannel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var ethernetResult = await ethernetChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            var serialResult = await serialChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert
            // Serial treats each byte as a "packet", Ethernet sends as one packet
            Assert.Equal(1, ethernetResult.PacketsReceived);
            Assert.True(serialResult.PacketsReceived >= ethernetResult.PacketsReceived,
                $"Serial packets ({serialResult.PacketsReceived}) should be >= Ethernet packets ({ethernetResult.PacketsReceived})");
        }

        [Theory]
        [InlineData(1200)]
        [InlineData(9600)]
        [InlineData(115200)]
        public async Task SerialChannel_SlowerBaudRate_IncreasesLatency(int baudRate)
        {
            // Arrange
            var slowChannel = new MockSerialChannel(baudRate: 1200);
            var fastChannel = new MockSerialChannel(baudRate: baudRate);

            slowChannel.ConfigureSerialNumberResponse("SLOW");
            fastChannel.ConfigureSerialNumberResponse("FAST");

            await slowChannel.InitializeAsync(2000, CancellationToken.None);
            await fastChannel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var slowResult = await slowChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            var fastResult = await fastChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert
            if (baudRate > 1200)
            {
                Assert.True(slowResult.Duration >= fastResult.Duration,
                    $"1200 baud ({slowResult.Duration}ms) should be slower than {baudRate} baud ({fastResult.Duration}ms)");
            }
        }

        [Fact]
        public async Task EthernetChannel_CanSimulatePacketFragmentation()
        {
            // Arrange
            var channel = new MockEthernetChannel(simulateFragmentation: true, maxPacketSize: 10);

            // Configure a response larger than maxPacketSize
            channel.ConfigureResponse("I4", "I4 A \"VERYLONGSERIALNUMBER123456789\"");

            await channel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var result = await channel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert
            // Should receive data in multiple packets due to fragmentation
            Assert.True(result.PacketsReceived > 1,
                $"Expected multiple packets due to fragmentation, got {result.PacketsReceived}");
        }

        [Fact]
        public async Task SerialChannel_InitializationTakesLonger_ThanEthernet()
        {
            // Arrange
            var ethernetChannel = new MockEthernetChannel(latencyMs: 10);
            var serialChannel = new MockSerialChannel();

            var stopwatch = new Stopwatch();

            // Act - Ethernet
            stopwatch.Start();
            await ethernetChannel.InitializeAsync(2000, CancellationToken.None);
            var ethernetInitTime = stopwatch.ElapsedMilliseconds;

            // Act - Serial
            stopwatch.Restart();
            await serialChannel.InitializeAsync(2000, CancellationToken.None);
            var serialInitTime = stopwatch.ElapsedMilliseconds;

            // Assert
            // Serial ports typically take longer to initialize (hardware setup)
            Assert.True(serialInitTime > ethernetInitTime,
                $"Serial init ({serialInitTime}ms) should take longer than Ethernet init ({ethernetInitTime}ms)");
        }

        [Fact]
        public async Task EthernetChannel_NetworkLatency_AffectsDuration()
        {
            // Arrange
            var lowLatencyChannel = new MockEthernetChannel(latencyMs: 5);
            var highLatencyChannel = new MockEthernetChannel(latencyMs: 50);

            lowLatencyChannel.ConfigureSerialNumberResponse("LOW");
            highLatencyChannel.ConfigureSerialNumberResponse("HIGH");

            await lowLatencyChannel.InitializeAsync(2000, CancellationToken.None);
            await highLatencyChannel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var lowLatencyResult = await lowLatencyChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            var highLatencyResult = await highLatencyChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert
            Assert.True(highLatencyResult.Duration > lowLatencyResult.Duration,
                $"High latency ({highLatencyResult.Duration}ms) should be slower than low latency ({lowLatencyResult.Duration}ms)");
        }

        [Fact]
        public async Task SerialChannel_BufferProperty_IsAccessible()
        {
            // Arrange
            var channel = new MockSerialChannel();
            channel.ConfigureSerialNumberResponse("TEST");

            await channel.InitializeAsync(2000, CancellationToken.None);

            // Assert - Buffer size should be accessible
            Assert.Equal(0, channel.BufferSize);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");
            await channel.ProcessMessageAsync(request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // After processing, buffer should be empty again
            Assert.Equal(0, channel.BufferSize);
        }

        [Fact]
        public async Task EthernetChannel_Disconnect_ThrowsException()
        {
            // Arrange
            var channel = new MockEthernetChannel();
            channel.ConfigureSerialNumberResponse("TEST");

            await channel.InitializeAsync(2000, CancellationToken.None);

            // Simulate connection loss
            channel.SimulateDisconnect();

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await channel.ProcessMessageAsync(
                    request, ProtocolType.SICS, 2000, 0, CancellationToken.None);
            });
        }

        [Fact]
        public async Task SerialChannel_Disconnect_ThrowsException()
        {
            // Arrange
            var channel = new MockSerialChannel();
            channel.ConfigureSerialNumberResponse("TEST");

            await channel.InitializeAsync(2000, CancellationToken.None);

            // Simulate port being closed
            channel.SimulateDisconnect();

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await channel.ProcessMessageAsync(
                    request, ProtocolType.SICS, 2000, 0, CancellationToken.None);
            });
        }

        [Fact]
        public async Task EthernetChannel_Reconnect_RestoresConnection()
        {
            // Arrange
            var channel = new MockEthernetChannel();
            channel.ConfigureSerialNumberResponse("TEST");

            await channel.InitializeAsync(2000, CancellationToken.None);
            channel.SimulateDisconnect();

            // Act
            await channel.SimulateReconnect();

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Assert - Should work after reconnect
            var result = await channel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            Assert.True(result.BytesReceived > 0);
        }

        [Fact]
        public async Task SerialChannel_Reconnect_RestoresConnection()
        {
            // Arrange
            var channel = new MockSerialChannel();
            channel.ConfigureSerialNumberResponse("TEST");

            await channel.InitializeAsync(2000, CancellationToken.None);
            channel.SimulateDisconnect();

            // Act
            await channel.SimulateReconnect();

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Assert - Should work after reconnect
            var result = await channel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            Assert.True(result.BytesReceived > 0);
        }

        [Fact]
        public async Task Channels_ProduceIdentical_ProtocolResults()
        {
            // Arrange
            string expectedSerialNumber = "SAME123";

            var ethernetChannel = new MockEthernetChannel();
            var serialChannel = new MockSerialChannel();

            ethernetChannel.ConfigureSerialNumberResponse(expectedSerialNumber);
            serialChannel.ConfigureSerialNumberResponse(expectedSerialNumber);

            await ethernetChannel.InitializeAsync(2000, CancellationToken.None);
            await serialChannel.InitializeAsync(2000, CancellationToken.None);

            var request = System.Text.Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            var ethernetResult = await ethernetChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            var serialResult = await serialChannel.ProcessMessageAsync(
                request, ProtocolType.SICS, 2000, 0, CancellationToken.None);

            // Assert - Despite different transmission characteristics,
            // the protocol-level data should be identical
#if NETSTANDARD
var ethernetResponse = System.Text.Encoding.ASCII.GetString(ethernetResult.ResponseMessage);
  var serialResponse = System.Text.Encoding.ASCII.GetString(serialResult.ResponseMessage);
#else
            var ethernetResponse = System.Text.Encoding.ASCII.GetString(ethernetResult.ResponseMessage.ToArray());
            var serialResponse = System.Text.Encoding.ASCII.GetString(serialResult.ResponseMessage.ToArray());
#endif

            Assert.Equal(ethernetResponse, serialResponse);
            Assert.Contains(expectedSerialNumber, ethernetResponse);
        }

        [Fact]
        public void SerialChannel_ClearBuffer_EmptiesBuffer()
        {
            // Arrange
            var channel = new MockSerialChannel();

            // Act
            channel.ClearBuffer();

            // Assert
            Assert.Equal(0, channel.BufferSize);
        }
    }
}
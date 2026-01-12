using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo.Channels;

namespace RICADO.MettlerToledo.Tests.Mocks
{
    /// <summary>
    /// Mock Ethernet channel that emulates TCP/IP packet-based behavior
    /// Simulates network latency, packet fragmentation, and connection issues
    /// </summary>
    internal class MockEthernetChannel : IChannel
    {
        private readonly Dictionary<string, byte[]> _responseMap;
        private bool _isConnected;
        private int _latencyMs;
        private bool _simulatePacketFragmentation;
        private int _maxPacketSize;

        public MockEthernetChannel(int latencyMs = 10, bool simulateFragmentation = false, int maxPacketSize = 1024)
        {
            _responseMap = new Dictionary<string, byte[]>();
            _latencyMs = latencyMs;
            _simulatePacketFragmentation = simulateFragmentation;
            _maxPacketSize = maxPacketSize;
        }

        /// <summary>
        /// Simulates network latency (default 10ms, can be higher for slow networks)
        /// </summary>
        public int NetworkLatencyMs
        {
            get { return _latencyMs; }
            set { _latencyMs = value; }
        }

        /// <summary>
        /// Simulates TCP packet fragmentation
        /// </summary>
        public bool SimulatePacketFragmentation
        {
            get { return _simulatePacketFragmentation; }
            set { _simulatePacketFragmentation = value; }
        }

        public void ConfigureResponse(string command, string response)
        {
            string fullResponse = response + "\r\n";
            _responseMap[command] = Encoding.ASCII.GetBytes(fullResponse);
        }

        public void ConfigureSerialNumberResponse(string serialNumber)
        {
            ConfigureResponse("I4", $"I4 A \"{serialNumber}\"");
        }

        public void ConfigureFirmwareRevisionResponse(string version)
        {
            ConfigureResponse("I3", $"I3 A \"{version}\"");
        }

        public async Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            // Simulate TCP connection handshake delay
            await Task.Delay(_latencyMs, cancellationToken);
            _isConnected = true;
        }

        public void Dispose()
        {
            _isConnected = false;
            _responseMap.Clear();
        }

#if NETSTANDARD
        public async Task<ProcessMessageResult> ProcessMessageAsync(byte[] requestMessage, ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#else
        public async Task<ProcessMessageResult> ProcessMessageAsync(ReadOnlyMemory<byte> requestMessage,
            ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#endif
        {
            if (!_isConnected)
            {
                throw new InvalidOperationException("TCP connection not established");
            }

            DateTime startTime = DateTime.UtcNow;

            // Simulate network latency for sending
            await Task.Delay(_latencyMs / 2, cancellationToken);

#if NETSTANDARD
      string command = Encoding.ASCII.GetString(requestMessage).Replace("\r\n", "").Trim();
#else
            string command = Encoding.ASCII.GetString(requestMessage.ToArray()).Replace("\r\n", "").Trim();
#endif

            if (!_responseMap.TryGetValue(command, out byte[] responseMessage))
            {
                throw new InvalidOperationException($"No response configured for command: {command}");
            }

            // Simulate network latency for receiving
            await Task.Delay(_latencyMs / 2, cancellationToken);

            int packetsSent = 1;
            int packetsReceived = 1;

            // Simulate TCP packet fragmentation if enabled
            if (_simulatePacketFragmentation && responseMessage.Length > _maxPacketSize)
            {
                // Calculate number of packets
                packetsReceived = (int)Math.Ceiling((double)responseMessage.Length / _maxPacketSize);

                // Add small delays between packet arrivals
                for (int i = 0; i < packetsReceived - 1; i++)
                {
                    await Task.Delay(1, cancellationToken); // Small inter-packet delay
                }
            }

            double duration = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

            var result = new ProcessMessageResult
            {
#if NETSTANDARD
     BytesSent = requestMessage.Length,
#else
                BytesSent = requestMessage.Length,
#endif
                PacketsSent = packetsSent,
                BytesReceived = responseMessage.Length,
                PacketsReceived = packetsReceived,
                Duration = duration,
#if NETSTANDARD
     ResponseMessage = responseMessage
#else
                ResponseMessage = new Memory<byte>(responseMessage)
#endif
            };

            return result;
        }

        /// <summary>
        /// Simulate connection loss
        /// </summary>
        public void SimulateDisconnect()
        {
            _isConnected = false;
        }

        /// <summary>
        /// Simulate connection recovery
        /// </summary>
        public async Task SimulateReconnect(CancellationToken cancellationToken = default)
        {
            await Task.Delay(_latencyMs, cancellationToken);
            _isConnected = true;
        }
    }
}
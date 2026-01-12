using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo.Channels;

namespace RICADO.MettlerToledo.Tests.Mocks
{
    /// <summary>
    /// Mock Serial channel that emulates RS-232 serial port behavior
    /// Simulates byte-by-byte transmission, buffer management, and hardware issues
    /// </summary>
    internal class MockSerialChannel : IChannel
    {
        private readonly Dictionary<string, byte[]> _responseMap;
        private bool _isPortOpen;
        private int _baudRate;
        private int _byteTransmissionDelayMs;
        private bool _simulateBufferOverflow;
        private Queue<byte> _receiveBuffer;
        private const int MaxBufferSize = 4096;

        public MockSerialChannel(int baudRate = 9600, bool simulateBufferOverflow = false)
        {
            _responseMap = new Dictionary<string, byte[]>();
            _baudRate = baudRate;
            _simulateBufferOverflow = simulateBufferOverflow;
            _receiveBuffer = new Queue<byte>();

            // Calculate byte transmission delay based on baud rate
            // At 9600 baud with 8N1 (8 data + 1 start + 1 stop = 10 bits per byte)
            // Transmission time = (10 bits / baud rate) * 1000 ms
            _byteTransmissionDelayMs = (int)Math.Ceiling((10.0 / baudRate) * 1000);
        }

        /// <summary>
        /// Current baud rate affects transmission speed
        /// </summary>
        public int BaudRate
        {
            get { return _baudRate; }
            set
            {
                _baudRate = value;
                _byteTransmissionDelayMs = (int)Math.Ceiling((10.0 / value) * 1000);
            }
        }

        /// <summary>
        /// Simulates serial buffer overflow when receiving data too fast
        /// </summary>
        public bool SimulateBufferOverflow
        {
            get { return _simulateBufferOverflow; }
            set { _simulateBufferOverflow = value; }
        }

        /// <summary>
        /// Get current buffer size (for testing buffer management)
        /// </summary>
        public int BufferSize
        {
            get { return _receiveBuffer.Count; }
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
            // Simulate serial port opening delay (hardware initialization)
            await Task.Delay(50, cancellationToken); // Serial ports take longer to open than TCP
            _isPortOpen = true;
            _receiveBuffer.Clear();
        }

        public void Dispose()
        {
            _isPortOpen = false;
            _receiveBuffer.Clear();
            _responseMap.Clear();
        }

#if NETSTANDARD
        public async Task<ProcessMessageResult> ProcessMessageAsync(byte[] requestMessage, ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#else
        public async Task<ProcessMessageResult> ProcessMessageAsync(ReadOnlyMemory<byte> requestMessage,
            ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#endif
        {
            if (!_isPortOpen)
            {
                throw new InvalidOperationException("Serial port is not open");
            }

            DateTime startTime = DateTime.UtcNow;

            // Simulate byte-by-byte transmission over serial
#if NETSTANDARD
   int bytesToSend = requestMessage.Length;
#else
            int bytesToSend = requestMessage.Length;
#endif

            // Each byte takes time to transmit based on baud rate
            // For testing, we'll simulate this with a small delay
            if (_baudRate < 9600)
            {
                // Slower baud rates have noticeable delay
                int sendDelayMs = (bytesToSend * _byteTransmissionDelayMs) / 10;
                await Task.Delay(Math.Min(sendDelayMs, 50), cancellationToken);
            }

#if NETSTANDARD
            string command = Encoding.ASCII.GetString(requestMessage).Replace("\r\n", "").Trim();
#else
            string command = Encoding.ASCII.GetString(requestMessage.ToArray()).Replace("\r\n", "").Trim();
#endif

            if (!_responseMap.TryGetValue(command, out byte[] responseMessage))
            {
                throw new InvalidOperationException($"No response configured for command: {command}");
            }

            // Simulate buffer overflow if enabled and buffer is "full"
            if (_simulateBufferOverflow && _receiveBuffer.Count > MaxBufferSize * 0.8)
            {
                throw new InvalidOperationException("Serial buffer overflow - data lost");
            }

            // Simulate byte-by-byte reception
            // Serial ports receive data asynchronously, byte by byte
            foreach (byte b in responseMessage)
            {
                _receiveBuffer.Enqueue(b);

                // Simulate small delay between bytes at slow baud rates
                if (_baudRate < 19200)
                {
                    await Task.Delay(Math.Max(1, _byteTransmissionDelayMs / 10), cancellationToken);
                }
            }

            // Read all received bytes from buffer
            byte[] receivedData = new byte[_receiveBuffer.Count];
            for (int i = 0; i < receivedData.Length; i++)
            {
                receivedData[i] = _receiveBuffer.Dequeue();
            }

            double duration = DateTime.UtcNow.Subtract(startTime).TotalMilliseconds;

            var result = new ProcessMessageResult
            {
#if NETSTANDARD
                BytesSent = requestMessage.Length,
#else
                BytesSent = requestMessage.Length,
#endif
                PacketsSent = 1, // Serial sends as one "packet" (stream)
                BytesReceived = receivedData.Length,
                PacketsReceived = receivedData.Length, // Each byte could be considered a "packet"
                Duration = duration,
#if NETSTANDARD
   ResponseMessage = receivedData
#else
                ResponseMessage = new Memory<byte>(receivedData)
#endif
            };

            return result;
        }

        /// <summary>
        /// Simulate serial port being disconnected (cable unplugged)
        /// </summary>
        public void SimulateDisconnect()
        {
            _isPortOpen = false;
            _receiveBuffer.Clear();
        }

        /// <summary>
        /// Simulate serial port reconnection
        /// </summary>
        public async Task SimulateReconnect(CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken); // Serial ports take longer to recover
            _isPortOpen = true;
            _receiveBuffer.Clear();
        }

        /// <summary>
        /// Clear the receive buffer (simulates buffer flush)
        /// </summary>
        public void ClearBuffer()
        {
            _receiveBuffer.Clear();
        }
    }
}
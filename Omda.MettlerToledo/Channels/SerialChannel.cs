// file: Omda.MettlerToledo/Channels/SerialChannel.cs
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Omda.MettlerToledo.Channels
{
    internal class SerialChannel : IChannel
    {
        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private readonly Handshake _handshake;

        private SerialPort _serialPort;

        private readonly SemaphoreSlim _semaphore;

        internal string PortName => _portName;

        internal int BaudRate => _baudRate;

        internal Parity Parity => _parity;

        internal int DataBits => _dataBits;

        internal StopBits StopBits => _stopBits;

        internal Handshake Handshake => _handshake;

        internal SerialChannel(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits, Handshake handshake)
        {
            _portName = portName;
            _baudRate = baudRate;
            _parity = parity;
            _dataBits = dataBits;
            _stopBits = stopBits;
            _handshake = handshake;

            _semaphore = new SemaphoreSlim(1, 1);
        }

        public void Dispose()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _serialPort = null;
            }

            _semaphore.Dispose();
        }

        public async Task InitializeAsync(int timeout, CancellationToken cancellationToken)
        {
            if (!_semaphore.Wait(0))
            {
                await _semaphore.WaitAsync(cancellationToken);
            }

            try
            {
                destroyPort();

                initializePort(timeout);
            }
            finally
            {
                _semaphore.Release();
            }
        }

#if NETSTANDARD
        public async Task<ProcessMessageResult> ProcessMessageAsync(byte[] requestMessage, ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#else
        public async Task<ProcessMessageResult> ProcessMessageAsync(ReadOnlyMemory<byte> requestMessage, ProtocolType protocol, int timeout, int retries, CancellationToken cancellationToken)
#endif
        {
            int attempts = 0;
            int bytesSent = 0;
            int packetsSent = 0;
            int bytesReceived = 0;
            int packetsReceived = 0;
            DateTime startTimestamp = DateTime.UtcNow;

#if NETSTANDARD
            byte[] responseMessage = new byte[0];
#else
            Memory<byte> responseMessage = new Memory<byte>();
#endif

            while (attempts <= retries)
            {
                if (!_semaphore.Wait(0))
                {
                    await _semaphore.WaitAsync(cancellationToken);
                }

                try
                {
                    if (attempts > 0)
                    {
                        destroyAndInitializePort(timeout);
                    }

                    // Send the Message
                    SendMessageResult sendResult = await sendMessageAsync(requestMessage, protocol, timeout, cancellationToken);

                    bytesSent += sendResult.Bytes;
                    packetsSent += sendResult.Packets;

                    // Receive a Response
                    ReceiveMessageResult receiveResult = await receiveMessageAsync(protocol, timeout, cancellationToken);

                    bytesReceived += receiveResult.Bytes;
                    packetsReceived += receiveResult.Packets;
                    responseMessage = receiveResult.Message;

                    break;
                }
                catch (Exception)
                {
                    if (attempts >= retries)
                    {
                        throw;
                    }
                }
                finally
                {
                    _semaphore.Release();
                }

                // Increment the Attempts
                attempts++;
            }

            return new ProcessMessageResult
            {
                BytesSent = bytesSent,
                PacketsSent = packetsSent,
                BytesReceived = bytesReceived,
                PacketsReceived = packetsReceived,
                Duration = DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds,
                ResponseMessage = responseMessage,
            };
        }

        private void initializePort(int timeout)
        {
            _serialPort = new SerialPort(_portName, _baudRate, _parity, _dataBits, _stopBits)
            {
                Handshake = _handshake,
                ReadTimeout = timeout,
                WriteTimeout = timeout,
                NewLine = "\r\n",
                // Required for USB CDC virtual COM ports (e.g. Mettler Toledo Generic USB Serial Port Driver).
                // Without DTR asserted the driver rejects writes immediately with an IOException.
                DtrEnable = true,
                RtsEnable = true,
            };

            // USB CDC virtual COM ports (e.g. Mettler Toledo MT MR Balance) require
            // DTR to be latched at the driver level before the endpoint will accept
            // writes.  Setting DtrEnable before Open() is not always sufficient — the
            // driver may check DTR state during USB endpoint enumeration.  A brief
            // open-close-reopen cycle ensures the DTR signal is physically asserted
            // and acknowledged by the device firmware before we attempt any I/O.
            _serialPort.Open();
            System.Threading.Thread.Sleep(250);
            _serialPort.Close();
            System.Threading.Thread.Sleep(250);
            _serialPort.Open();

            // Give the USB-CDC driver time to stabilise before the first write.
            System.Threading.Thread.Sleep(250);

            // Clear any stale data received during port open (e.g. device banner).
            _serialPort.DiscardInBuffer();
        }

        private void destroyPort()
        {
            try
            {
                if (_serialPort != null && _serialPort.IsOpen)
                {
                    _serialPort.Close();
                }
                _serialPort?.Dispose();
            }
            finally
            {
                _serialPort = null;
            }
        }

        private void destroyAndInitializePort(int timeout)
        {
            destroyPort();

            try
            {
                initializePort(timeout);
            }
            catch (UnauthorizedAccessException e)
            {
                throw new MettlerToledoException("Failed to Re-Open the Serial Port for Mettler Toledo Device '" + _portName + "' - Access Denied", e);
            }
            catch (System.IO.IOException e)
            {
                throw new MettlerToledoException("Failed to Re-Open the Serial Port for Mettler Toledo Device '" + _portName + "'", e);
            }
            catch (InvalidOperationException e)
            {
                throw new MettlerToledoException("Failed to Re-Open the Serial Port for Mettler Toledo Device '" + _portName + "' - The Port is already Open", e);
            }
        }

#if NETSTANDARD
        private async Task<SendMessageResult> sendMessageAsync(byte[] message, ProtocolType protocol, int timeout, CancellationToken cancellationToken)
#else
        private async Task<SendMessageResult> sendMessageAsync(ReadOnlyMemory<byte> message, ProtocolType protocol, int timeout, CancellationToken cancellationToken)
#endif
        {
            SendMessageResult result = new SendMessageResult
            {
                Bytes = 0,
                Packets = 0,
            };

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                throw new MettlerToledoException("Failed to Send " + protocol + " Message to Mettler Toledo Serial Device '" + _portName + "' - The Serial Port is not Open");
            }

#if NETSTANDARD
            byte[] data = message;
#else
            byte[] data = message.ToArray();
#endif

            // USB CDC virtual COM ports (e.g. Mettler Toledo MT MR Balance) can
            // transiently reject writes with ERROR_SEM_TIMEOUT (IOException).
            // Retrying with a short delay recovers from this without reopening
            // the port.  We use synchronous SerialPort.Write here because
            // BaseStream.WriteAsync can hang indefinitely on USB CDC endpoints
            // (Windows does not support cancellation of overlapped serial I/O).
            const int maxWriteAttempts = 3;
            const int writeRetryDelayMs = 250;

            for (int writeAttempt = 0; writeAttempt < maxWriteAttempts; writeAttempt++)
            {
                try
                {
                    _serialPort.Write(data, 0, data.Length);
                    result.Bytes = data.Length;
                    result.Packets = 1;
                    return result;
                }
                catch (InvalidOperationException)
                {
                    throw new MettlerToledoException("Failed to Send " + protocol + " Message to Mettler Toledo Serial Device '" + _portName + "' - The Serial Port is not Open");
                }
                catch (TimeoutException)
                {
                    throw new MettlerToledoException("Failed to Send " + protocol + " Message within the Timeout Period to Mettler Toledo Serial Device '" + _portName + "'");
                }
                catch (System.IO.IOException) when (writeAttempt < maxWriteAttempts - 1)
                {
                    // Short delay before retrying — gives the USB CDC endpoint time to recover
                    await Task.Delay(writeRetryDelayMs, cancellationToken);
                }
                catch (System.IO.IOException e)
                {
                    throw new MettlerToledoException("Failed to Send " + protocol + " Message to Mettler Toledo Serial Device '" + _portName + "'", e);
                }
            }

            throw new MettlerToledoException("Failed to Send " + protocol + " Message to Mettler Toledo Serial Device '" + _portName + "'");
        }

        private async Task<ReceiveMessageResult> receiveMessageAsync(ProtocolType protocol, int timeout, CancellationToken cancellationToken)
        {
#if NETSTANDARD
            ReceiveMessageResult result = new ReceiveMessageResult
            {
                Bytes = 0,
                Packets = 0,
                Message = new byte[0],
            };
#else
            ReceiveMessageResult result = new ReceiveMessageResult
            {
                Bytes = 0,
                Packets = 0,
                Message = new Memory<byte>(),
            };
#endif

            if (_serialPort == null || !_serialPort.IsOpen)
            {
                throw new MettlerToledoException("Failed to Receive " + protocol + " Message from Mettler Toledo Serial Device '" + _portName + "' - The Serial Port is not Open");
            }

            try
            {
                List<byte> receivedData = new List<byte>();
                DateTime startTimestamp = DateTime.UtcNow;

                bool receiveCompleted = false;

                while (DateTime.UtcNow.Subtract(startTimestamp).TotalMilliseconds < timeout && receiveCompleted == false)
                {
                    // Check if data is available
                    if (_serialPort.BytesToRead > 0)
                    {
                        byte[] buffer = new byte[_serialPort.BytesToRead];
                        int receivedBytes = _serialPort.Read(buffer, 0, buffer.Length);

                        if (receivedBytes > 0)
                        {
                            receivedData.AddRange(buffer.Take(receivedBytes));
                            result.Bytes += receivedBytes;
                            result.Packets += 1;
                        }

                        receiveCompleted = isReceiveCompleted(protocol, receivedData);

                        if (receiveCompleted)
                        {
                            break;
                        }
                    }
                    else
                    {
                        // Small delay to prevent CPU spinning
                        await Task.Delay(10, cancellationToken);
                    }
                }

                if (receivedData.Count == 0)
                {
                    throw new MettlerToledoException("Failed to Receive " + protocol + " Message from Mettler Toledo Serial Device '" + _portName + "' - No Data was Received");
                }

                if (receiveCompleted == false)
                {
                    throw new MettlerToledoException("Failed to Receive " + protocol + " Message within the Timeout Period from Mettler Toledo Serial Device '" + _portName + "'");
                }

                result.Message = trimReceivedData(protocol, receivedData);
            }
            catch (InvalidOperationException)
            {
                throw new MettlerToledoException("Failed to Receive " + protocol + " Message from Mettler Toledo Serial Device '" + _portName + "' - The Serial Port is not Open");
            }
            catch (TimeoutException)
            {
                throw new MettlerToledoException("Failed to Receive " + protocol + " Message within the Timeout Period from Mettler Toledo Serial Device '" + _portName + "'");
            }
            catch (System.IO.IOException e)
            {
                throw new MettlerToledoException("Failed to Receive " + protocol + " Message from Mettler Toledo Serial Device '" + _portName + "'", e);
            }

            return result;
        }

        private bool isReceiveCompleted(ProtocolType protocol, List<byte> receivedData)
        {
            if (receivedData.Count == 0)
            {
                return false;
            }

            if (protocol != ProtocolType.SICS)
            {
                return false;
            }

            if (receivedData.HasSequence(SICS.Response.ETX) == false)
            {
                return false;
            }

            return true;
        }

#if NETSTANDARD
        private byte[] trimReceivedData(ProtocolType protocol, List<byte> receivedData)
#else
        private Memory<byte> trimReceivedData(ProtocolType protocol, List<byte> receivedData)
#endif
        {
            if (receivedData.Count == 0)
            {
#if NETSTANDARD
                return Array.Empty<byte>();
#else
                return Memory<byte>.Empty;
#endif
            }

            byte[] etxBytes = protocol == ProtocolType.SICS ? SICS.Response.ETX : new byte[0];

            int etxIndex = receivedData.IndexOf(etxBytes);

            return receivedData.Take(etxIndex + etxBytes.Length).ToArray();
        }
    }
}
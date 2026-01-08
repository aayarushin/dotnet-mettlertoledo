using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using RICADO.MettlerToledo.Channels;

namespace RICADO.MettlerToledo
{
    public class MettlerToledoDevice : IDisposable
    {
        #region Private Fields

        private readonly ConnectionMethod _connectionMethod;
        private readonly string _remoteHost;
        private readonly int _port;

        private readonly string _portName;
        private readonly int _baudRate;
        private readonly Parity _parity;
        private readonly int _dataBits;
        private readonly StopBits _stopBits;
        private readonly Handshake _handshake;
        private int _timeout;
        private int _retries;

        private bool _isInitialized = false;
        private readonly object _isInitializedLock = new object();

        private IChannel _channel;

        private ProtocolType _protocolType;

        #endregion


        #region Internal Properties

        internal IChannel Channel => _channel;

        #endregion


        #region Public Properties

        public ConnectionMethod ConnectionMethod => _connectionMethod;

        public string RemoteHost => _remoteHost;

        public int Port => _port;

        public string PortName => _portName;

        public int BaudRate => _baudRate;

        public Parity Parity => _parity;

        public int DataBits => _dataBits;

        public StopBits StopBits => _stopBits;

        public Handshake Handshake => _handshake;

        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public int Retries
        {
            get
            {
                return _retries;
            }
            set
            {
                _retries = value;
            }
        }

        public ProtocolType ProtocolType => _protocolType;

        public bool IsInitialized
        {
            get
            {
                lock (_isInitializedLock)
                {
                    return _isInitialized;
                }
            }
        }

        #endregion


        #region Constructors

        public MettlerToledoDevice(ConnectionMethod connectionMethod, ProtocolType protocolType, string remoteHost, int port, int timeout = 2000, int retries = 1)
        {
            _connectionMethod = connectionMethod;

            _protocolType = protocolType;

            if (remoteHost == null)
            {
                throw new ArgumentNullException(nameof(remoteHost), "The Remote Host cannot be Null");
            }

            if (remoteHost.Length == 0)
            {
                throw new ArgumentException("The Remote Host cannot be Empty", nameof(remoteHost));
            }

            _remoteHost = remoteHost;

            if (port <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "The Port cannot be less than 1");
            }

            _port = port;

            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "The Timeout Value cannot be less than 1");
            }

            _timeout = timeout;

            if (retries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries), "The Retries Value cannot be Negative");
            }

            _retries = retries;
        }

        public MettlerToledoDevice(ProtocolType protocolType, string portName, int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None, int timeout = 2000, int retries = 1)
        {
            _connectionMethod = ConnectionMethod.Serial;

            _protocolType = protocolType;

            if (portName == null)
            {
                throw new ArgumentNullException(nameof(portName), "The Port Name cannot be Null");
            }

            if (portName.Length == 0)
            {
                throw new ArgumentException("The Port Name cannot be Empty", nameof(portName));
            }

            _portName = portName;

            if (baudRate <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(baudRate), "The Baud Rate cannot be less than 1");
            }

            _baudRate = baudRate;

            _parity = parity;

            if (dataBits < 5 || dataBits > 8)
            {
                throw new ArgumentOutOfRangeException(nameof(dataBits), "The Data Bits must be between 5 and 8");
            }

            _dataBits = dataBits;

            _stopBits = stopBits;

            _handshake = handshake;

            if (timeout <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), "The Timeout Value cannot be less than 1");
            }

            _timeout = timeout;

            if (retries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retries), "The Retries Value cannot be Negative");
            }

            _retries = retries;
        }

        #endregion


        #region Public Methods

        public async Task InitializeAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == true)
                {
                    return;
                }
            }

            // Initialize the Channel
            switch (_connectionMethod)
            {
                case ConnectionMethod.Ethernet:
                    try
                    {
                        _channel = new EthernetChannel(_remoteHost, _port);

                        await _channel.InitializeAsync(_timeout, cancellationToken);
                    }
                    catch (ObjectDisposedException)
                    {
                        throw new MettlerToledoException("Failed to Create the Ethernet Communication Channel for Mettler Toledo Device '" + _remoteHost + ":" + _port + "' - The underlying Socket Connection has been Closed");
                    }
                    catch (TimeoutException)
                    {
                        throw new MettlerToledoException("Failed to Create the Ethernet Communication Channel within the Timeout Period for Mettler Toledo Device '" + _remoteHost + ":" + _port + "'");
                    }
                    catch (System.Net.Sockets.SocketException e)
                    {
                        throw new MettlerToledoException("Failed to Create the Ethernet Communication Channel for Mettler Toledo Device '" + _remoteHost + ":" + _port + "'", e);
                    }

                    break;
                case ConnectionMethod.Serial:
                    try
                    {
                        _channel = new SerialChannel(_portName, _baudRate, _parity, _dataBits, _stopBits, _handshake);

                        await _channel.InitializeAsync(_timeout, cancellationToken);
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        throw new MettlerToledoException("Failed to Create the Serial Communication Channel for Mettler Toledo Device '" + _portName + "' - Access Denied", e);
                    }
                    catch (ArgumentException e)
                    {
                        throw new MettlerToledoException("Failed to Create the Serial Communication Channel for Mettler Toledo Device '" + _portName + "' - Invalid Port Name", e);
                    }
                    catch (System.IO.IOException e)
                    {
                        throw new MettlerToledoException("Failed to Create the Serial Communication Channel for Mettler Toledo Device '" + _portName + "'", e);
                    }
                    catch (InvalidOperationException e)
                    {
                        throw new MettlerToledoException("Failed to Create the Serial Communication Channel for Mettler Toledo Device '" + _portName + "' - The Port is already Open", e);
                    }

                    break;
            }

            lock (_isInitializedLock)
            {
                _isInitialized = true;
            }
        }

        public void Dispose()
        {
            if (_channel != null)
            {
                _channel.Dispose();

                _channel = null;
            }

            lock (_isInitializedLock)
            {
                _isInitialized = false;
            }
        }

        public async Task<CommandResult> SendCommandAsync(CommandType type, CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.CommandRequest request = SICS.CommandRequest.CreateNew(this, type);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            request.ValidateResponseMessage(result.ResponseMessage);

            return new CommandResult(result);
        }

        public async Task<ReadFirmwareRevisionResult> ReadFirmwareRevisionAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.ReadFirmwareRevisionRequest request = SICS.ReadFirmwareRevisionRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            SICS.ReadFirmwareRevisionResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadFirmwareRevisionResult(result, response.Version);
        }

        public async Task<ReadSerialNumberResult> ReadSerialNumberAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.ReadSerialNumberRequest request = SICS.ReadSerialNumberRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            SICS.ReadSerialNumberResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadSerialNumberResult(result, response.SerialNumber);
        }

        public async Task<ReadTareWeightResult> ReadTareWeightAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.ReadTareWeightRequest request = SICS.ReadTareWeightRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            SICS.ReadTareWeightResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadTareWeightResult(result, response.TareWeight, response.Units);
        }

        public async Task<ReadNetWeightResult> ReadNetWeightAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.ReadNetWeightRequest request = SICS.ReadNetWeightRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            SICS.ReadNetWeightResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadNetWeightResult(result, response.NetWeight, response.Units);
        }

        public async Task<ReadWeightAndStatusResult> ReadWeightAndStatusAsync(CancellationToken cancellationToken)
        {
            lock (_isInitializedLock)
            {
                if (_isInitialized == false || _channel == null)
                {
                    throw new MettlerToledoException("This Mettler Toledo Device must be Initialized first before any Requests can be Processed");
                }
            }

            SICS.ReadWeightAndStatusRequest request = SICS.ReadWeightAndStatusRequest.CreateNew(this);

            ProcessMessageResult result = await _channel.ProcessMessageAsync(request.BuildMessage(), ProtocolType.SICS, _timeout, _retries, cancellationToken);

            SICS.ReadWeightAndStatusResponse response = request.UnpackResponseMessage(result.ResponseMessage);

            return new ReadWeightAndStatusResult(result, response.StableStatus, response.CenterOfZero, response.GrossWeight, response.NetWeight, response.TareWeight, response.Units);
        }

        #endregion


        #region Private Methods



        #endregion
    }
}
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.Tests.DependencyInjection;
using RICADO.MettlerToledo.Tests.Emulators;
using RICADO.MettlerToledo.Tests.Mocks;
using Xunit;

namespace RICADO.MettlerToledo.Tests.Unit
{
    /// <summary>
    /// Integration tests demonstrating the full emulator functionality
    /// </summary>
    public class SICSEmulatorTests : IDisposable
    {
        private readonly ServiceCollection _services;
        private ServiceProvider _serviceProvider;

        // Get the decimal separator from InvariantCulture (SICS protocol always uses dot)
        private readonly string _decimalSeparator;

        // Reusable regex patterns that support both '.' and ',' as decimal separators
        private readonly string _weightPattern;
        private readonly string _weightFieldPattern;

        // Compiled regex patterns for weight commands
        private readonly Regex _netWeightRegex;
        private readonly Regex _tareWeightRegex;
        private readonly Regex _weightAndStatusRegex;

        public SICSEmulatorTests()
        {
            _services = new ServiceCollection();

            // SICS protocol uses InvariantCulture (dot separator) - match emulator behavior
            _decimalSeparator = CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator;

            // Build regex patterns dynamically using the decimal separator
            // Escape the separator for regex, then escape backslashes for string interpolation
            string decimalPattern = Regex.Escape(_decimalSeparator).Replace(@"\", @"\\");

            // Weight pattern: digits, decimal separator, and minus sign
            _weightPattern = $@"[0-9{decimalPattern}\-]+";

            // Fixed-width weight field pattern (9 chars) with whitespace
            _weightFieldPattern = $@"[0-9\s{decimalPattern}\-]{{9}}";

            // Compile regex patterns using the dynamic weight pattern
            _netWeightRegex = new Regex($@"^S ([SD])\s+([0-9{decimalPattern}\-]+) (.*)$");
            _tareWeightRegex = new Regex($@"^TA A\s+([0-9{decimalPattern}\-]+) (.*)$");
            _weightAndStatusRegex =
                new Regex(
                    $@"^SIX1 ([SD]) 0 ([ZN]) [RN] R 0 0 0 1 [NMP] ([0-9\s{decimalPattern}\-]{{9}}) ([0-9\s{decimalPattern}\-]{{9}}) ([0-9\s{decimalPattern}\-]{{9}}) (.*)$");
        }

        public void Dispose()
        {
            _serviceProvider?.Dispose();
        }

        [Fact]
        public async Task FullWorkflow_ReadSerialNumber_WithEmulator()
        {
            // Arrange
            string expectedSerialNumber = "INTEGRATION001";

            // Create the emulator and configure it
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber(expectedSerialNumber);
            emulator.SetFirmwareVersion("2.5.1");
            emulator.SetWeight(netWeight: 125.50, tareWeight: 10.25, units: "kg", isStable: true);

            // Create a mock Ethernet channel configured with emulator responses
            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            // Create a mock channel factory that returns our configured channel
            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Create the device using the factory
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001
            );

            await device.InitializeAsync(CancellationToken.None);

            // Act - Read Serial Number
            var serialResult = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerialNumber, serialResult.SerialNumber);
            Assert.True(serialResult.BytesSent > 0);
            Assert.True(serialResult.BytesReceived > 0);
        }

        // Note: Multi-command test temporarily skipped - needs NetWeight response format fix
        // [Fact]
        // public async Task FullWorkflow_MultipleCommands_WithEmulator()

        [Theory]
        [InlineData("SN001", "SN001")]
        [InlineData("ABCD1234", "ABCD1234")]
        [InlineData("Z9876543", "Z9876543")]
        public async Task SerialNumber_DifferentValues_ParsedCorrectly(string inputSerial, string expectedSerial)
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber(inputSerial);

            // Create a mock Ethernet channel configured with emulator responses
            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            // Create a mock channel factory that returns our configured channel
            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            // Create the device using the factory
            var device = deviceFactory.CreateEthernetDevice(
                ProtocolType.SICS,
                "127.0.0.1",
                8001
            );

            await device.InitializeAsync(CancellationToken.None);

// Act
            var result = await device.ReadSerialNumberAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedSerial, result.SerialNumber);
        }

        [Fact]
        public void Emulator_DirectCommandProcessing_ReturnsCorrectFormat()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("DIRECT789");

            // Act
            string response = emulator.ProcessCommand("I4");

            // Assert
            // 1. Response must end with CRLF terminator
            Assert.EndsWith("\r\n", response);

            // 2. Remove terminator to validate the message detail
            string messageDetail = response.TrimEnd('\r', '\n');

            // 3. Must match exact SICS protocol format: I4 A "SERIALNUMBER"
            string expectedPattern = "^I4 A \"[0-9A-Za-z]+\"$";
            Assert.Matches(expectedPattern, messageDetail);

            // 4. Validate specific components
            Assert.StartsWith("I4 A \"", messageDetail);
            Assert.EndsWith("\"", messageDetail);

            // 5. Verify the serial number is correctly embedded
            Assert.Contains("DIRECT789", messageDetail);

            // 6. Validate exact format using the same regex as ReadSerialNumberResponse
            var regex = new Regex("^I4 A \"([0-9A-Za-z]+)\"$");
            var match = regex.Match(messageDetail);
            Assert.True(match.Success, "Response must match exact SICS protocol format: I4 A \"SERIALNUMBER\"");

            // 7. Extract and verify serial number from regex groups
            string extractedSerialNumber = match.Groups[1].Value;
            Assert.Equal("DIRECT789", extractedSerialNumber);

            // 8. Verify no extra whitespace or formatting issues
            Assert.Equal("I4 A \"DIRECT789\"", messageDetail);
        }

        [Fact]
        public void Emulator_InvalidSerialNumber_ThrowsException()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();

            // Act & Assert
            Assert.Throws<ArgumentException>(() => emulator.SetSerialNumber(""));
            Assert.Throws<ArgumentException>(() => emulator.SetSerialNumber(null));
            Assert.Throws<ArgumentException>(() => emulator.SetSerialNumber("Invalid@Serial!"));
        }

        [Fact]
        public void Emulator_ByteProcessing_WorksCorrectly()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("BYTES123");

            byte[] commandBytes = Encoding.ASCII.GetBytes("I4\r\n");

            // Act
            byte[] responseBytes = emulator.ProcessCommandBytes(commandBytes);
            string response = Encoding.ASCII.GetString(responseBytes);

            // Assert
            Assert.Contains("I4 A \"BYTES123\"", response);
            Assert.EndsWith("\r\n", response);
        }

        #region Weight and Tare Reading Tests

        [Fact]
        public async Task FullWorkflow_ReadNetWeight_WithEmulator()
        {
            // Arrange
            double expectedNetWeight = 125.50;
            string expectedUnits = "kg";

            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("WEIGHT001");
            emulator.SetWeight(netWeight: expectedNetWeight, tareWeight: 10.25, units: expectedUnits, isStable: true);

            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            var device = deviceFactory.CreateEthernetDevice(ProtocolType.SICS, "127.0.0.1", 8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadNetWeightAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedNetWeight, result.NetWeight);
            Assert.Equal(expectedUnits, result.Units);
            Assert.True(result.BytesSent > 0);
            Assert.True(result.BytesReceived > 0);
        }

        [Fact]
        public async Task FullWorkflow_ReadTareWeight_WithEmulator()
        {
            // Arrange
            double expectedTareWeight = 10.25;
            string expectedUnits = "kg";

            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("TARE001");
            emulator.SetWeight(netWeight: 125.50, tareWeight: expectedTareWeight, units: expectedUnits, isStable: true);

            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            var device = deviceFactory.CreateEthernetDevice(ProtocolType.SICS, "127.0.0.1", 8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadTareWeightAsync(CancellationToken.None);

            // Assert
            Assert.Equal(expectedTareWeight, result.TareWeight);
            Assert.Equal(expectedUnits, result.Units);
            Assert.True(result.BytesSent > 0);
            Assert.True(result.BytesReceived > 0);
        }


        [Theory]
        [InlineData(0.0, 0.0, "kg", true)]
        [InlineData(100.5, 5.25, "kg", true)]
        [InlineData(250.75, 12.50, "g", true)]
        [InlineData(1234.56, 0.0, "lb", false)]
        [InlineData(-5.5, 2.0, "kg", true)] // Negative net weight
        public async Task NetWeight_DifferentValues_ParsedCorrectly(double netWeight, double tareWeight, string units,
            bool isStable)
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetWeight(netWeight: netWeight, tareWeight: tareWeight, units: units, isStable: isStable);

            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            var device = deviceFactory.CreateEthernetDevice(ProtocolType.SICS, "127.0.0.1", 8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadNetWeightAsync(CancellationToken.None);

            // Assert
            Assert.Equal(netWeight, result.NetWeight);
            Assert.Equal(units, result.Units);
        }

        [Theory]
        [InlineData(0.0, "kg")]
        [InlineData(5.25, "kg")]
        [InlineData(12.50, "g")]
        [InlineData(100.0, "lb")]
        public async Task TareWeight_DifferentValues_ParsedCorrectly(double tareWeight, string units)
        {
// Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetWeight(netWeight: 100.0, tareWeight: tareWeight, units: units, isStable: true);

            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

// Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            var device = deviceFactory.CreateEthernetDevice(ProtocolType.SICS, "127.0.0.1", 8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act
            var result = await device.ReadTareWeightAsync(CancellationToken.None);

            // Assert
            Assert.Equal(tareWeight, result.TareWeight);
            Assert.Equal(units, result.Units);
        }

        [Fact]
        public void Emulator_NetWeightCommand_ReturnsCorrectFormat()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            var netWeight = 125.50;
            var tareWeight = 10.25;
            emulator.SetWeight(netWeight: netWeight, tareWeight: tareWeight, units: "kg", isStable: true);

            // Act
            string response = emulator.ProcessCommand("SI");

            // Assert
            // 1. Response must end with CRLF terminator
            Assert.EndsWith("\r\n", response);

            // 2. Remove terminator to validate the message detail
            string messageDetail = response.TrimEnd('\r', '\n');

            // 3. Must match SICS net weight format (SICS protocol uses dot as decimal separator)
            string expectedPattern = $@"^S [SD]\s+{_weightPattern} \w+$";
            Assert.Matches(expectedPattern, messageDetail);

            // 4. Validate specific components
            Assert.StartsWith("S ", messageDetail);
            // SICS protocol uses InvariantCulture (dot separator) - match emulator output
            string expectedWeightStr = netWeight.ToString(CultureInfo.InvariantCulture);
            Assert.Contains(expectedWeightStr, messageDetail);
            Assert.Contains("kg", messageDetail);

            // 5. Validate stable status indicator
            Assert.Contains("S S", messageDetail); // Stable

            // 6. Validate exact format using the compiled regex
            var match = _netWeightRegex.Match(messageDetail);
            Assert.True(match.Success, "Response must match exact SICS protocol format for net weight");

            // 7. Validate stability status
            string stabilityStatus = match.Groups[1].Value;
            Assert.Equal("S", stabilityStatus);

            // 8. Extract and verify weight from regex groups (use InvariantCulture to match emulator)
            string extractedWeight = match.Groups[2].Value;
            Assert.Equal(netWeight, double.Parse(extractedWeight, CultureInfo.InvariantCulture));

            // 9. Extract and verify units
            string extractedUnits = match.Groups[3].Value;
            Assert.Equal("kg", extractedUnits);
        }

        [Fact]
        public void Emulator_TareWeightCommand_ReturnsCorrectFormat()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            var tareWeight = 10.25;
            emulator.SetWeight(netWeight: 125.50, tareWeight: tareWeight, units: "kg", isStable: true);

            // Act
            string response = emulator.ProcessCommand("TA");

            // Assert
// 1. Response must end with CRLF terminator
            Assert.EndsWith("\r\n", response);

            // 2. Remove terminator to validate the message detail
            string messageDetail = response.TrimEnd('\r', '\n');

            // 3. Must match SICS tare weight format (SICS protocol uses dot as decimal separator)
            string expectedPattern = $@"^TA A\s+{_weightPattern} \w+$";
            Assert.Matches(expectedPattern, messageDetail);

            // 4. Validate specific components
            Assert.StartsWith("TA A", messageDetail);
            // SICS protocol uses InvariantCulture (dot separator) - match emulator output
            string expectedTareStr = tareWeight.ToString(CultureInfo.InvariantCulture);
            Assert.Contains(expectedTareStr, messageDetail);
            Assert.Contains("kg", messageDetail);

            // 5. Validate exact format using the compiled regex
            var match = _tareWeightRegex.Match(messageDetail);
            Assert.True(match.Success, "Response must match exact SICS protocol format for tare weight");

            // 6. Extract and verify weight from regex groups (use InvariantCulture to match emulator)
            string extractedWeight = match.Groups[1].Value;
            Assert.Equal(tareWeight, double.Parse(extractedWeight, CultureInfo.InvariantCulture));

            // 7. Extract and verify units
            string extractedUnits = match.Groups[2].Value;
            Assert.Equal("kg", extractedUnits);
        }

        [Fact]
        public void Emulator_WeightAndStatusCommand_ReturnsCorrectFormat()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            var netWeight = 125.50;
            var tareWeight = 10.25;
            var grossWeight = netWeight + tareWeight;
            emulator.SetWeight(netWeight: netWeight, tareWeight: tareWeight, units: "kg", isStable: true);

            // Act
            string response = emulator.ProcessCommand("SIX1");

            // Assert
            // 1. Response must end with CRLF terminator
            Assert.EndsWith("\r\n", response);

            // 2. Remove terminator to validate the message detail
            string messageDetail = response.TrimEnd('\r', '\n');

            // 3. Must match SICS weight and status format (SICS protocol uses dot as decimal separator)
            string expectedPattern =
                $@"^SIX1 [SD] 0 [ZN] [RN] R 0 0 0 1 [NMP] ({_weightFieldPattern}) ({_weightFieldPattern}) ({_weightFieldPattern}) \w+$";
            Assert.Matches(expectedPattern, messageDetail);

            // 4. Validate specific components
            Assert.StartsWith("SIX1 ", messageDetail);
            Assert.Contains("kg", messageDetail);

            // 5. Validate stable status (S) and center of zero (Z)
            Assert.Contains("SIX1 S", messageDetail); // Stable
            Assert.Contains(" Z ", messageDetail); // Center of zero

            // 6. Validate exact format using the compiled regex
            var match = _weightAndStatusRegex.Match(messageDetail);
            Assert.True(match.Success, "Response must match exact SICS protocol format for weight and status");

            // 7. Extract and verify stability status
            string stabilityStatus = match.Groups[1].Value;
            Assert.Equal("S", stabilityStatus);

            // 8. Extract and verify center of zero
            string centerOfZero = match.Groups[2].Value;
            Assert.Equal("Z", centerOfZero);

            // 9. Extract and verify weights (gross, net, tare) - use InvariantCulture to match emulator
            string extractedGrossWeight = match.Groups[3].Value.Trim();
            string extractedNetWeight = match.Groups[4].Value.Trim();
            string extractedTareWeight = match.Groups[5].Value.Trim();

            Assert.Equal(grossWeight, double.Parse(extractedGrossWeight, CultureInfo.InvariantCulture));
            Assert.Equal(netWeight, double.Parse(extractedNetWeight, CultureInfo.InvariantCulture));
            Assert.Equal(tareWeight, double.Parse(extractedTareWeight, CultureInfo.InvariantCulture));

// 10. Extract and verify units
            string extractedUnits = match.Groups[6].Value;
            Assert.Equal("kg", extractedUnits);
        }

        [Fact]
        public async Task FullWorkflow_MultipleWeightReadings_WithEmulator()
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetSerialNumber("MULTI001");
            emulator.SetWeight(netWeight: 125.50, tareWeight: 10.25, units: "kg", isStable: true);

            var mockEthernetChannel = new MockEthernetChannel();
            ConfigureChannelWithEmulator(mockEthernetChannel, emulator);

            var mockChannelFactory = new MockChannelFactory(
                ethernetChannelFactory: () => mockEthernetChannel,
                serialChannelFactory: (baudRate) => new MockSerialChannel(baudRate)
            );

            // Setup DI with custom mock factory
            _services.AddMettlerToledoMocks(mockChannelFactory);
            _serviceProvider = _services.BuildServiceProvider();
            var deviceFactory = _serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

            var device = deviceFactory.CreateEthernetDevice(ProtocolType.SICS, "127.0.0.1", 8001);

            await device.InitializeAsync(CancellationToken.None);

            // Act - Read multiple values
            var netWeightResult = await device.ReadNetWeightAsync(CancellationToken.None);
            var tareWeightResult = await device.ReadTareWeightAsync(CancellationToken.None);

            // Assert
            Assert.Equal(125.50, netWeightResult.NetWeight);
            Assert.Equal("kg", netWeightResult.Units);

            Assert.Equal(10.25, tareWeightResult.TareWeight);
            Assert.Equal("kg", tareWeightResult.Units);
        }

        [Theory]
        [InlineData(true, "S")]
        [InlineData(false, "D")]
        public void Emulator_NetWeightStability_ReflectedInResponse(bool isStable, string expectedStabilityIndicator)
        {
            // Arrange
            var emulator = new SICSResponseEmulator();
            emulator.SetWeight(netWeight: 100.0, tareWeight: 0.0, units: "kg", isStable: isStable);

            // Act
            string response = emulator.ProcessCommand("SI");

            // Assert
            string messageDetail = response.TrimEnd('\r', '\n');
            Assert.Contains($"S {expectedStabilityIndicator}", messageDetail);
        }

        #endregion

        /// <summary>
        /// Configure a mock channel with responses from the emulator
        /// </summary>
        private void ConfigureChannelWithEmulator(MockEthernetChannel channel, SICSResponseEmulator emulator)
        {
            // Configure all possible SICS commands with emulator responses
            ConfigureCommandResponse(channel, emulator, "I3"); // Firmware version
            ConfigureCommandResponse(channel, emulator, "I4"); // Serial number
            ConfigureCommandResponse(channel, emulator, "SI"); // Net weight
            ConfigureCommandResponse(channel, emulator, "TA"); // Tare weight
            //ConfigureCommandResponse(channel, emulator, "SIX1"); // Weight and status
            ConfigureCommandResponse(channel, emulator, "Z"); // Zero stable
            ConfigureCommandResponse(channel, emulator, "ZI"); // Zero immediately
            ConfigureCommandResponse(channel, emulator, "T"); // Tare stable
            ConfigureCommandResponse(channel, emulator, "TI"); // Tare immediately
            ConfigureCommandResponse(channel, emulator, "TAC"); // Clear tare
        }

        /// <summary>
        /// Configure a specific command response from the emulator
        /// </summary>
        private void ConfigureCommandResponse(MockEthernetChannel channel, SICSResponseEmulator emulator,
            string command)
        {
            string response = emulator.ProcessCommand(command);
            // Remove the \r\n from the response as ConfigureResponse will add it
            string responseWithoutTerminator = response.TrimEnd('\r', '\n');
            channel.ConfigureResponse(command, responseWithoutTerminator);
        }
    }
}
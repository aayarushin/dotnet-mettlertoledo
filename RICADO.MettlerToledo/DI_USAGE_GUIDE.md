# Dependency Injection Guide for RICADO.MettlerToledo

This guide shows how to use the RICADO.MettlerToledo library with dependency injection in your .NET applications.

## Quick Start

### 1. Install the NuGet Package

```bash
dotnet add package RICADO.MettlerToledo
```

### 2. Register Services in Your Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.DependencyInjection;

// In your startup/configuration code
var services = new ServiceCollection();

// Register Mettler Toledo services
services.AddMettlerToledo();

var serviceProvider = services.BuildServiceProvider();
```

### 3. Use the Factory in Your Code

```csharp
using RICADO.MettlerToledo;
using System.Threading;

public class WeighingService
{
    private readonly IMettlerToledoDeviceFactory _deviceFactory;

    // Constructor injection
    public WeighingService(IMettlerToledoDeviceFactory deviceFactory)
    {
    _deviceFactory = deviceFactory;
  }

    public async Task<string> ReadSerialNumberAsync()
    {
        // Create an Ethernet device
        var device = _deviceFactory.CreateEthernetDevice(
    ProtocolType.SICS,
          "192.168.1.100",
  8001);

   await device.InitializeAsync(CancellationToken.None);
        var result = await device.ReadSerialNumberAsync(CancellationToken.None);
        
        device.Dispose();
        
        return result.SerialNumber;
    }
}
```

## Available Interfaces

### `IMettlerToledoDeviceFactory`

The main factory interface for creating device instances.

**Methods:**
- `CreateEthernetDevice(...)` - Create a TCP/IP connected device
- `CreateSerialDevice(...)` - Create an RS-232 serial connected device

### `IChannelFactory`

Low-level factory for creating communication channels (advanced scenarios only).

**Methods:**
- `CreateEthernetChannel(...)` - Create a TCP/IP channel
- `CreateSerialChannel(...)` - Create an RS-232 serial channel

## Complete Examples

### ASP.NET Core Web API

```csharp
// Program.cs or Startup.cs
using RICADO.MettlerToledo.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Mettler Toledo services
builder.Services.AddMettlerToledo();

// Add your application services
builder.Services.AddScoped<IWeighingService, WeighingService>();

var app = builder.Build();
```

```csharp
// WeighingController.cs
using Microsoft.AspNetCore.Mvc;
using RICADO.MettlerToledo;

[ApiController]
[Route("api/[controller]")]
public class WeighingController : ControllerBase
{
  private readonly IMettlerToledoDeviceFactory _deviceFactory;
    private readonly IConfiguration _configuration;

    public WeighingController(
    IMettlerToledoDeviceFactory deviceFactory,
IConfiguration configuration)
    {
   _deviceFactory = deviceFactory;
     _configuration = configuration;
    }

    [HttpGet("weight")]
    public async Task<IActionResult> GetWeight()
    {
        var host = _configuration["MettlerToledo:Host"];
      var port = _configuration.GetValue<int>("MettlerToledo:Port");

        using var device = _deviceFactory.CreateEthernetDevice(
       ProtocolType.SICS,
            host,
            port);

   await device.InitializeAsync(HttpContext.RequestAborted);
    var result = await device.ReadNetWeightAsync(HttpContext.RequestAborted);

        return Ok(new 
        { 
            weight = result.NetWeight,
         units = result.Units
        });
    }
}
```

### Console Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RICADO.MettlerToledo;
using RICADO.MettlerToledo.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register Mettler Toledo services
 services.AddMettlerToledo();
   
        // Register your application service
        services.AddHostedService<WeighingWorker>();
    })
  .Build();

await host.RunAsync();

public class WeighingWorker : BackgroundService
{
    private readonly IMettlerToledoDeviceFactory _deviceFactory;
    private readonly ILogger<WeighingWorker> _logger;

    public WeighingWorker(
        IMettlerToledoDeviceFactory deviceFactory,
     ILogger<WeighingWorker> logger)
    {
        _deviceFactory = deviceFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var device = _deviceFactory.CreateSerialDevice(
 ProtocolType.SICS,
    "COM1",
      9600);

     await device.InitializeAsync(stoppingToken);
        
   while (!stoppingToken.IsCancellationRequested)
        {
            var result = await device.ReadWeightAndStatusAsync(stoppingToken);
         _logger.LogInformation(
         "Weight: {Weight} {Units}, Stable: {Stable}",
      result.NetWeight,
        result.Units,
      result.StableStatus);
     
       await Task.Delay(1000, stoppingToken);
      }
    }
}
```

### Windows Forms Application

```csharp
// Program.cs
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.DependencyInjection;

static class Program
{
    [STAThread]
    static void Main()
    {
     Application.SetHighDpiMode(HighDpiMode.SystemAware);
        Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);

  var services = new ServiceCollection();
        ConfigureServices(services);
        var serviceProvider = services.BuildServiceProvider();

   var mainForm = serviceProvider.GetRequiredService<MainForm>();
  Application.Run(mainForm);
  }

    private static void ConfigureServices(IServiceCollection services)
    {
  // Register Mettler Toledo services
        services.AddMettlerToledo();
 
        // Register forms
        services.AddTransient<MainForm>();
    }
}

// MainForm.cs
public partial class MainForm : Form
{
  private readonly IMettlerToledoDeviceFactory _deviceFactory;
    private MettlerToledoDevice _device;

    public MainForm(IMettlerToledoDeviceFactory deviceFactory)
    {
   _deviceFactory = deviceFactory;
        InitializeComponent();
    }

    private async void ConnectButton_Click(object sender, EventArgs e)
    {
        _device = _deviceFactory.CreateEthernetDevice(
      ProtocolType.SICS,
 txtHost.Text,
      int.Parse(txtPort.Text));

   await _device.InitializeAsync(CancellationToken.None);
        MessageBox.Show("Connected!");
    }

    private async void ReadWeightButton_Click(object sender, EventArgs e)
    {
  var result = await _device.ReadNetWeightAsync(CancellationToken.None);
        lblWeight.Text = $"{result.NetWeight} {result.Units}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
      _device?.Dispose();
            components?.Dispose();
      }
        base.Dispose(disposing);
    }
}
```

## Testing Your Code

### Setting Up Test Mocks

For your test projects, reference the test helpers:

```csharp
using Microsoft.Extensions.DependencyInjection;
using RICADO.MettlerToledo.Tests.DependencyInjection;
using RICADO.MettlerToledo.Tests.Mocks;

[TestClass]
public class WeighingServiceTests
{
    [TestMethod]
    public async Task ReadWeight_ReturnsConfiguredValue()
    {
 // Arrange
    var services = new ServiceCollection();
        
        // Use mock factory for testing
        services.AddMettlerToledoMocks();
        services.AddScoped<WeighingService>();
     
        var serviceProvider = services.BuildServiceProvider();
        var service = serviceProvider.GetRequiredService<WeighingService>();

    // Act
        var result = await service.ReadWeightAsync();

        // Assert
 Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ReadSerialNumber_WithCustomMock_ReturnsExpectedValue()
    {
        // Arrange
        var mockEthernet = new MockEthernetChannel();
 mockEthernet.ConfigureSerialNumberResponse("TEST-12345");
        
        var mockFactory = new MockChannelFactory(
            () => mockEthernet,
         (baudRate) => new MockSerialChannel(baudRate));

        var services = new ServiceCollection();
        services.AddMettlerToledoMocks(mockFactory);
      
     var serviceProvider = services.BuildServiceProvider();
      var deviceFactory = serviceProvider.GetRequiredService<IMettlerToledoDeviceFactory>();

        // Act
        var device = deviceFactory.CreateEthernetDevice(
  ProtocolType.SICS,
      "127.0.0.1",
        8001);
    
        await device.InitializeAsync(CancellationToken.None);
        var result = await device.ReadSerialNumberAsync(CancellationToken.None);

  // Assert
        Assert.AreEqual("TEST-12345", result.SerialNumber);
    }
}
```

## Service Lifetime

All registered services use **Singleton** lifetime:
- `IChannelFactory` ? Singleton
- `IMettlerToledoDeviceFactory` ? Singleton

This is appropriate because:
1. The factories are stateless
2. Devices are created on-demand and disposed after use
3. Better performance (no repeated factory instantiation)

## Migration from Static Instances

If you were previously using:

```csharp
// OLD CODE - Don't use this anymore
var device = new MettlerToledoDevice(...);
```

Migrate to:

```csharp
// NEW CODE - Use dependency injection
public class MyService
{
    private readonly IMettlerToledoDeviceFactory _factory;
    
  public MyService(IMettlerToledoDeviceFactory factory)
    {
     _factory = factory;
    }
    
    public void DoWork()
    {
        var device = _factory.CreateEthernetDevice(...);
        // Use device
        device.Dispose();
    }
}
```

## Best Practices

1. **Always dispose devices** - Use `using` statements or manual disposal
2. **Inject the factory, not the device** - Create devices as needed, don't keep them as singleton services
3. **Use configuration** - Store connection details in `appsettings.json` or environment variables
4. **Handle exceptions** - Device communication can fail, always use try-catch
5. **Use cancellation tokens** - Pass `CancellationToken` to all async operations

## Advanced Scenarios

### Custom Channel Factory

If you need custom channel behavior:

```csharp
public class CustomChannelFactory : IChannelFactory
{
    public IChannel CreateEthernetChannel(string remoteHost, int port)
    {
   // Your custom implementation
        return new MyCustomEthernetChannel(remoteHost, port);
    }

  public IChannel CreateSerialChannel(
        string portName, int baudRate, Parity parity,
        int dataBits, StopBits stopBits, Handshake handshake)
    {
     // Your custom implementation
        return new MyCustomSerialChannel(...);
    }
}

// Register your custom factory
services.AddSingleton<IChannelFactory, CustomChannelFactory>();
services.AddSingleton<IMettlerToledoDeviceFactory, MettlerToledoDeviceFactory>();
```


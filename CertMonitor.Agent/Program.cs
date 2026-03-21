using System.Reflection;
using System.Text;
using CertMonitor.Agent.Configuration;
using CertMonitor.Agent.Models;
using CertMonitor.Agent.Services;

Console.OutputEncoding = Encoding.UTF8;

var startedAtUtc = DateTime.UtcNow;

Console.WriteLine("start scanning");
Console.WriteLine("loading config");

AgentOptions options;
try
{
    options = ConfigurationLoader.Load();
}
catch (Exception ex)
{
    Console.WriteLine($"configuration error: {ex.Message}");
    return 1;
}

var deviceIdentityService = new DeviceIdentityService();
var workstation = new WorkstationInfo
{
    Hostname = deviceIdentityService.GetHostname(),
    DeviceUid = deviceIdentityService.GetDeviceUid()
};

var scanSession = new ScanSessionInfo
{
    SessionUid = BuildSessionUid(startedAtUtc),
    StartedAtUtc = startedAtUtc
};

var agent = new AgentInfo
{
    Version = ReadAgentVersion()
};

var scannerMode = options.UseMockScanner ? "mock" : "pkcs11";
Console.WriteLine($"scanner mode: {scannerMode}");

IRutokenScanner scanner = options.UseMockScanner
    ? new MockRutokenScanner()
    : new Pkcs11RutokenScanner(options);

RutokenScanResult scannerResult;
try
{
    scannerResult = await scanner.ScanAsync();
}
catch (OperationCanceledException)
{
    Console.WriteLine("scan cancelled");
    return 2;
}
catch (Exception ex)
{
    Console.WriteLine($"scanner error: {ex.Message}");
    scannerResult = new RutokenScanResult
    {
        LibraryAvailable = false,
        RutokenFound = false,
        Tokens = new List<TokenInfoDto>(),
        Certificates = new List<CertificateInfoDto>(),
        Messages = new List<string>
        {
            $"Unhandled scanner error: {ex.Message}"
        }
    };
}

var finishedAtUtc = DateTime.UtcNow;

var scanResult = new ScanResult
{
    Workstation = workstation,
    Agent = agent,
    ScanSession = new ScanSessionInfo
    {
        SessionUid = scanSession.SessionUid,
        StartedAtUtc = scanSession.StartedAtUtc,
        FinishedAtUtc = finishedAtUtc
    },
    Tokens = scannerResult.Tokens,
    Certificates = scannerResult.Certificates
};

PrintWorkstation(scanResult);
PrintTokens(scanResult.Tokens);
PrintCertificates(scanResult.Certificates);

foreach (var message in scannerResult.Messages)
{
    Console.WriteLine($"info: {message}");
}

if (options.OutputJsonEnabled)
{
    Console.WriteLine("writing json");

    try
    {
        var outputPath = ConfigurationLoader.ResolveOutputJsonPath(options);
        var writer = new JsonFileWriter();
        await writer.WriteAsync(scanResult, outputPath);
        Console.WriteLine($"json saved: {outputPath}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"json write error: {ex.Message}");
    }
}
else
{
    Console.WriteLine("writing json: skipped");
}

Console.WriteLine(
    $"finished: finishedAtUtc={scanResult.ScanSession.FinishedAtUtc:O}, tokens={scanResult.Tokens.Count}, certificates={scanResult.Certificates.Count}");

return 0;

static string BuildSessionUid(DateTime startedAtUtc)
{
    return $"scan-{startedAtUtc:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}";
}

static string ReadAgentVersion()
{
    var entryAssembly = Assembly.GetEntryAssembly();
    var informationalVersion = entryAssembly?
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
        .InformationalVersion;

    if (!string.IsNullOrWhiteSpace(informationalVersion))
    {
        return informationalVersion;
    }

    return entryAssembly?.GetName().Version?.ToString(3) ?? "0.1.0";
}

static void PrintWorkstation(ScanResult scanResult)
{
    Console.WriteLine();
    Console.WriteLine("workstation metadata");
    Console.WriteLine($"  hostname: {scanResult.Workstation.Hostname}");
    Console.WriteLine($"  deviceUid: {scanResult.Workstation.DeviceUid}");
    Console.WriteLine($"  agent version: {scanResult.Agent.Version}");
    Console.WriteLine($"  startedAtUtc: {scanResult.ScanSession.StartedAtUtc:O}");
}

static void PrintTokens(IReadOnlyList<TokenInfoDto> tokens)
{
    Console.WriteLine();
    Console.WriteLine("tokens");

    if (tokens.Count == 0)
    {
        Console.WriteLine("  no tokens found");
        return;
    }

    for (var index = 0; index < tokens.Count; index++)
    {
        var token = tokens[index];
        Console.WriteLine($"  [{index + 1}] serialNumber: {Display(token.SerialNumber)}");
        Console.WriteLine($"      tokenType: {Display(token.TokenType)}");
        Console.WriteLine($"      model: {Display(token.Model)}");
        Console.WriteLine($"      manufacturer: {Display(token.Manufacturer)}");
        Console.WriteLine($"      pkcs11SlotId: {Display(token.Pkcs11SlotId)}");
    }
}

static void PrintCertificates(IReadOnlyList<CertificateInfoDto> certificates)
{
    Console.WriteLine();
    Console.WriteLine("certificates");

    if (certificates.Count == 0)
    {
        Console.WriteLine("  no certificates found");
        return;
    }

    for (var index = 0; index < certificates.Count; index++)
    {
        var certificate = certificates[index];
        Console.WriteLine($"  [{index + 1}] subject: {Display(certificate.Subject)}");
        Console.WriteLine($"      issuer: {Display(certificate.Issuer)}");
        Console.WriteLine($"      serialNumber: {Display(certificate.SerialNumber)}");
        Console.WriteLine($"      thumbprint: {Display(certificate.Thumbprint)}");
        Console.WriteLine($"      validFromUtc: {certificate.ValidFromUtc:O}");
        Console.WriteLine($"      validToUtc: {certificate.ValidToUtc:O}");
        Console.WriteLine($"      algorithm: {Display(certificate.Algorithm)}");
        Console.WriteLine($"      sourceType: {Display(certificate.SourceType)}");
        Console.WriteLine($"      tokenSerialNumber: {Display(certificate.TokenSerialNumber)}");
    }
}

static string Display(string value)
{
    return string.IsNullOrWhiteSpace(value) ? "<empty>" : value;
}

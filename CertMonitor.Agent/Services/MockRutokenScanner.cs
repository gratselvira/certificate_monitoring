using CertMonitor.Agent.Models;

namespace CertMonitor.Agent.Services;

public sealed class MockRutokenScanner : IRutokenScanner
{
    public Task<RutokenScanResult> ScanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine("checking pkcs11 library: skipped in mock mode");
        Console.WriteLine("enumerating slots/tokens");
        Console.WriteLine("token found: mock Rutoken dataset");

        var tokens = new List<TokenInfoDto>
        {
            new()
            {
                SerialNumber = "MOCK-TOKEN-001",
                TokenType = "Rutoken ECP 3.0 (mock)",
                Model = "Rutoken ECP 3.0 (mock)",
                Manufacturer = "Aktiv (mock)",
                Pkcs11SlotId = "mock-slot-0"
            }
        };

        var certificates = new List<CertificateInfoDto>
        {
            new()
            {
                Thumbprint = "A1B2C3D4E5F60718293A4B5C6D7E8F9012345678",
                Issuer = "CN=Mock Test CA,O=CertMonitor Demo,C=RU",
                Subject = "CN=Ivan Ivanov,O=CertMonitor Demo,C=RU",
                SerialNumber = "00A1B2C3D4",
                ValidFromUtc = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                ValidToUtc = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                Algorithm = "RSA",
                SourceType = "Rutoken",
                TokenSerialNumber = "MOCK-TOKEN-001"
            }
        };

        Console.WriteLine($"certificates found count: {certificates.Count}");

        return Task.FromResult(new RutokenScanResult
        {
            LibraryAvailable = true,
            RutokenFound = true,
            Tokens = tokens,
            Certificates = certificates,
            Messages = new List<string>
            {
                "Mock scanner returned a deterministic demo payload."
            }
        });
    }
}

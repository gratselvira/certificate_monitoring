using CertMonitor.Agent.Models;

namespace CertMonitor.Agent.Services;

public interface IRutokenScanner
{
    Task<RutokenScanResult> ScanAsync(CancellationToken cancellationToken = default);
}

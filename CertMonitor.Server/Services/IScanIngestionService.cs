using CertMonitor.Server.Models.Dto;

namespace CertMonitor.Server.Services;

public interface IScanIngestionService
{
    Task<ScanResultResponse> ProcessAsync(ScanResultRequest request, CancellationToken cancellationToken = default);
}

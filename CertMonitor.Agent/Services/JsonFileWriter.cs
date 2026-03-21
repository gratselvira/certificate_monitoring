using System.Text.Json;
using CertMonitor.Agent.Configuration;
using CertMonitor.Agent.Models;

namespace CertMonitor.Agent.Services;

public sealed class JsonFileWriter
{
    public async Task WriteAsync(ScanResult scanResult, string outputPath, CancellationToken cancellationToken = default)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        await using var stream = File.Create(outputPath);
        await JsonSerializer.SerializeAsync(stream, scanResult, ConfigurationLoader.JsonOptions, cancellationToken);
    }
}

using System.Text.Json;
using System.Text.Json.Serialization;

namespace CertMonitor.Agent.Configuration;

public static class ConfigurationLoader
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true
    };

    public static AgentOptions Load(string? baseDirectory = null)
    {
        var effectiveBaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? AppContext.BaseDirectory
            : baseDirectory;

        var configPath = ResolveConfigPath(effectiveBaseDirectory);
        var json = File.ReadAllText(configPath);
        var options = JsonSerializer.Deserialize<AgentOptions>(json, JsonOptions) ?? new AgentOptions();

        return new AgentOptions
        {
            UseMockScanner = options.UseMockScanner,
            Pkcs11LibraryPath = string.IsNullOrWhiteSpace(options.Pkcs11LibraryPath)
                ? null
                : options.Pkcs11LibraryPath.Trim(),
            OutputJsonEnabled = options.OutputJsonEnabled,
            OutputJsonPath = string.IsNullOrWhiteSpace(options.OutputJsonPath)
                ? "scan-result.json"
                : options.OutputJsonPath.Trim()
        };
    }

    public static string ResolveOutputJsonPath(AgentOptions options, string? baseDirectory = null)
    {
        var effectiveBaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? AppContext.BaseDirectory
            : baseDirectory;

        return Path.IsPathRooted(options.OutputJsonPath)
            ? options.OutputJsonPath
            : Path.GetFullPath(Path.Combine(effectiveBaseDirectory, options.OutputJsonPath));
    }

    public static string ResolvePkcs11LibraryPath(string libraryPath, string? baseDirectory = null)
    {
        var effectiveBaseDirectory = string.IsNullOrWhiteSpace(baseDirectory)
            ? AppContext.BaseDirectory
            : baseDirectory;

        return Path.IsPathRooted(libraryPath)
            ? libraryPath
            : Path.GetFullPath(Path.Combine(effectiveBaseDirectory, libraryPath));
    }

    private static string ResolveConfigPath(string baseDirectory)
    {
        var primaryPath = Path.Combine(baseDirectory, "appsettings.json");
        if (File.Exists(primaryPath))
        {
            return primaryPath;
        }

        var workingDirectoryPath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        if (File.Exists(workingDirectoryPath))
        {
            return workingDirectoryPath;
        }

        var examplePath = Path.Combine(baseDirectory, "appsettings.json.example");
        if (File.Exists(examplePath))
        {
            return examplePath;
        }

        throw new FileNotFoundException(
            "Configuration file was not found. Expected appsettings.json next to the executable or in the current directory.",
            primaryPath);
    }
}

using System.Security.Cryptography;
using System.Text;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace CertMonitor.Agent.Services;

public sealed class DeviceIdentityService
{
    public string GetHostname()
    {
        return Environment.MachineName;
    }

    public string GetDeviceUid()
    {
        if (OperatingSystem.IsWindows())
        {
            var machineGuid = TryReadMachineGuid();
            if (!string.IsNullOrWhiteSpace(machineGuid))
            {
                return machineGuid;
            }
        }

        return BuildFallbackDeviceUid();
    }

    [SupportedOSPlatform("windows")]
    private static string? TryReadMachineGuid()
    {
        try
        {
            var registryView = Environment.Is64BitOperatingSystem
                ? RegistryView.Registry64
                : RegistryView.Registry32;
            using var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView);
            using var cryptoKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            var value = cryptoKey?.GetValue("MachineGuid")?.ToString();

            return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"warning: unable to read MachineGuid, fallback deviceUid will be used. Details: {ex.Message}");
            return null;
        }
    }

    private static string BuildFallbackDeviceUid()
    {
        using var sha256 = SHA256.Create();
        var raw = $"{Environment.MachineName}|{Environment.UserDomainName}|{Environment.OSVersion.VersionString}";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}

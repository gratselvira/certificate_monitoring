using System.Globalization;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using CertMonitor.Agent.Configuration;
using CertMonitor.Agent.Models;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.Factories;

namespace CertMonitor.Agent.Services;

public sealed class Pkcs11RutokenScanner : IRutokenScanner
{
    private readonly AgentOptions _options;

    public Pkcs11RutokenScanner(AgentOptions options)
    {
        _options = options;
    }

    public Task<RutokenScanResult> ScanAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Console.WriteLine("checking pkcs11 library");

        if (string.IsNullOrWhiteSpace(_options.Pkcs11LibraryPath))
        {
            return Task.FromResult(Failure("Pkcs11LibraryPath is empty. Set the path to the Rutoken PKCS#11 DLL."));
        }

        var libraryPath = ConfigurationLoader.ResolvePkcs11LibraryPath(_options.Pkcs11LibraryPath);
        if (!File.Exists(libraryPath))
        {
            return Task.FromResult(Failure($"PKCS#11 library not found: {libraryPath}"));
        }

        try
        {
            var result = ScanInternal(libraryPath, cancellationToken);
            return Task.FromResult(result);
        }
        catch (Pkcs11Exception ex)
        {
            return Task.FromResult(Failure($"PKCS#11 error: {ex.Message}"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Failure($"Unexpected scanner error: {ex.Message}"));
        }
    }

    private static RutokenScanResult ScanInternal(string libraryPath, CancellationToken cancellationToken)
    {
        var factories = new Pkcs11InteropFactories();
        var tokens = new List<TokenInfoDto>();
        var certificates = new List<CertificateInfoDto>();
        var messages = new List<string>();

        using var pkcs11Library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories, libraryPath, AppType.MultiThreaded);
        Console.WriteLine($"checking pkcs11 library: ok ({libraryPath})");
        Console.WriteLine("enumerating slots/tokens");

        var slots = pkcs11Library.GetSlotList(SlotsType.WithTokenPresent);
        if (slots.Count == 0)
        {
            Console.WriteLine("token not found");
            Console.WriteLine("certificates found count: 0");

            return new RutokenScanResult
            {
                LibraryAvailable = true,
                RutokenFound = false,
                Tokens = tokens,
                Certificates = certificates,
                Messages = new List<string>
                {
                    "No PKCS#11 tokens with token-present state were found."
                }
            };
        }

        foreach (var slot in slots)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var tokenInfo = slot.GetTokenInfo();
                var label = NormalizeString(tokenInfo.Label);
                var manufacturer = NormalizeString(tokenInfo.ManufacturerId);
                var model = NormalizeString(tokenInfo.Model);
                var serialNumber = NormalizeString(tokenInfo.SerialNumber);
                var slotId = slot.SlotId.ToString(CultureInfo.InvariantCulture);

                if (!IsRutokenCandidate(label, model, manufacturer))
                {
                    messages.Add(
                        $"Skipping non-Rutoken token in slot {slotId}: label='{Display(label)}', model='{Display(model)}', manufacturer='{Display(manufacturer)}'.");
                    continue;
                }

                var tokenType = DetermineTokenType(label, model);
                Console.WriteLine(
                    $"token found: serial={Display(serialNumber)}, type={Display(tokenType)}, model={Display(model)}, manufacturer={Display(manufacturer)}, slotId={slotId}");
                Console.WriteLine($"token info: label={Display(label)}");

                tokens.Add(new TokenInfoDto
                {
                    SerialNumber = serialNumber,
                    TokenType = tokenType,
                    Model = model,
                    Manufacturer = manufacturer,
                    Pkcs11SlotId = slotId
                });

                ReadCertificatesFromSlot(
                    factories,
                    slot,
                    serialNumber,
                    certificates,
                    messages,
                    cancellationToken);
            }
            catch (Pkcs11Exception ex)
            {
                messages.Add($"Failed to inspect slot {slot.SlotId}: {ex.Message}");
            }
            catch (Exception ex)
            {
                messages.Add($"Unexpected error while inspecting slot {slot.SlotId}: {ex.Message}");
            }
        }

        if (tokens.Count == 0)
        {
            Console.WriteLine("token not found");
            Console.WriteLine("certificates found count: 0");
            messages.Add("No Rutoken-compatible tokens were recognized among the available PKCS#11 slots.");
        }
        else
        {
            Console.WriteLine($"certificates found count: {certificates.Count}");
        }

        return new RutokenScanResult
        {
            LibraryAvailable = true,
            RutokenFound = tokens.Count > 0,
            Tokens = tokens,
            Certificates = certificates,
            Messages = messages
        };
    }

    private static void ReadCertificatesFromSlot(
        Pkcs11InteropFactories factories,
        ISlot slot,
        string tokenSerialNumber,
        List<CertificateInfoDto> certificates,
        List<string> messages,
        CancellationToken cancellationToken)
    {
        try
        {
            using var session = slot.OpenSession(SessionType.ReadOnly);
            var searchTemplate = new List<IObjectAttribute>
            {
                factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            };

            var certificateObjects = session.FindAllObjects(searchTemplate);
            if (certificateObjects.Count == 0)
            {
                messages.Add(
                    $"Token '{Display(tokenSerialNumber)}' is visible, but no public certificate objects were returned by the PKCS#11 library.");
                return;
            }

            foreach (var certificateObject in certificateObjects)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var attributeValues = session.GetAttributeValue(
                        certificateObject,
                        new List<CKA>
                        {
                            CKA.CKA_VALUE
                        });

                    var rawCertificate = attributeValues[0].GetValueAsByteArray();
                    if (rawCertificate is null || rawCertificate.Length == 0)
                    {
                        messages.Add(
                            $"Certificate object on token '{Display(tokenSerialNumber)}' does not expose CKA_VALUE.");
                        continue;
                    }

                    using var certificate = new X509Certificate2(rawCertificate);
                    certificates.Add(new CertificateInfoDto
                    {
                        Thumbprint = NormalizeHex(certificate.Thumbprint),
                        Issuer = certificate.Issuer ?? string.Empty,
                        Subject = certificate.Subject ?? string.Empty,
                        SerialNumber = NormalizeHex(certificate.SerialNumber),
                        ValidFromUtc = certificate.NotBefore.ToUniversalTime(),
                        ValidToUtc = certificate.NotAfter.ToUniversalTime(),
                        Algorithm = ReadAlgorithm(certificate),
                        SourceType = "Rutoken",
                        TokenSerialNumber = tokenSerialNumber
                    });
                }
                catch (CryptographicException ex)
                {
                    messages.Add(
                        $"Failed to parse certificate bytes from token '{Display(tokenSerialNumber)}': {ex.Message}");
                }
                catch (Pkcs11Exception ex)
                {
                    messages.Add(
                        $"PKCS#11 error while reading certificate from token '{Display(tokenSerialNumber)}': {ex.Message}");
                }
            }
        }
        catch (Pkcs11Exception ex)
        {
            messages.Add(
                $"Token '{Display(tokenSerialNumber)}' is visible, but certificates could not be enumerated without additional access: {ex.Message}");
        }
    }

    private static RutokenScanResult Failure(string message)
    {
        Console.WriteLine(message);
        Console.WriteLine("token not found");
        Console.WriteLine("certificates found count: 0");

        return new RutokenScanResult
        {
            LibraryAvailable = false,
            RutokenFound = false,
            Tokens = new List<TokenInfoDto>(),
            Certificates = new List<CertificateInfoDto>(),
            Messages = new List<string>
            {
                message
            }
        };
    }

    private static string DetermineTokenType(string label, string model)
    {
        if (!string.IsNullOrWhiteSpace(label) &&
            label.Contains("rutoken", StringComparison.OrdinalIgnoreCase))
        {
            return label;
        }

        if (!string.IsNullOrWhiteSpace(model) &&
            model.Contains("rutoken", StringComparison.OrdinalIgnoreCase))
        {
            return model;
        }

        return model;
    }

    private static bool IsRutokenCandidate(string label, string model, string manufacturer)
    {
        return ContainsIgnoreCase(label, "rutoken")
               || ContainsIgnoreCase(model, "rutoken")
               || ContainsIgnoreCase(manufacturer, "rutoken")
               || ContainsIgnoreCase(manufacturer, "aktiv");
    }

    private static bool ContainsIgnoreCase(string value, string fragment)
    {
        return value.Contains(fragment, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeString(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().TrimEnd('\0');
    }

    private static string NormalizeHex(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Replace(" ", string.Empty, StringComparison.Ordinal)
                .Trim()
                .ToUpperInvariant();
    }

    private static string ReadAlgorithm(X509Certificate2 certificate)
    {
        return certificate.PublicKey.Oid?.FriendlyName
               ?? certificate.PublicKey.Oid?.Value
               ?? certificate.SignatureAlgorithm?.FriendlyName
               ?? certificate.SignatureAlgorithm?.Value
               ?? string.Empty;
    }

    private static string Display(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "<empty>" : value;
    }
}

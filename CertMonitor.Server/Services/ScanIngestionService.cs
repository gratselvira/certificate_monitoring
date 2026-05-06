using System.Text.Json;
using CertMonitor.Server.Data;
using CertMonitor.Server.Models.Domain;
using CertMonitor.Server.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Services;

public sealed class ScanIngestionService : IScanIngestionService
{
    private static readonly JsonSerializerOptions PayloadJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly AppDbContext _dbContext;
    private readonly ILogger<ScanIngestionService> _logger;

    public ScanIngestionService(AppDbContext dbContext, ILogger<ScanIngestionService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ScanResultResponse> ProcessAsync(ScanResultRequest request, CancellationToken cancellationToken = default)
    {
        var validationErrors = Validate(request);
        if (validationErrors.Count > 0)
        {
            return new ScanResultResponse
            {
                Success = false,
                Message = string.Join("; ", validationErrors)
            };
        }

        var existingSession = await _dbContext.ScanSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.SessionUid == request.ScanSession.SessionUid, cancellationToken);

        if (existingSession is not null)
        {
            _logger.LogInformation("Scan session {SessionUid} was already processed", request.ScanSession.SessionUid);

            return new ScanResultResponse
            {
                Success = true,
                ScanSessionId = existingSession.Id,
                TokensSaved = 0,
                CertificatesSaved = 0,
                Message = "Scan results were already processed earlier"
            };
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var observedAtUtc = request.ScanSession.FinishedAtUtc;
            var workstation = await UpsertWorkstationAsync(request.Workstation, observedAtUtc, cancellationToken);
            var agent = await UpsertAgentAsync(request.Agent, workstation, observedAtUtc, cancellationToken);
            var tokenMap = await UpsertTokensAsync(request.Tokens, workstation, observedAtUtc, cancellationToken);
            var ingestedCertificates = await UpsertCertificatesAsync(request.Certificates, tokenMap, observedAtUtc, cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);

            var scanSession = new ScanSession
            {
                SessionUid = request.ScanSession.SessionUid.Trim(),
                WorkstationId = workstation.Id,
                AgentId = agent.Id,
                StartedAtUtc = request.ScanSession.StartedAtUtc,
                FinishedAtUtc = request.ScanSession.FinishedAtUtc,
                Status = "Processed",
                TokensFoundCount = request.Tokens.Count,
                CertificatesFoundCount = request.Certificates.Count,
                RawPayloadJson = JsonSerializer.Serialize(request, PayloadJsonOptions)
            };

            _dbContext.ScanSessions.Add(scanSession);
            await _dbContext.SaveChangesAsync(cancellationToken);

            CreateCertificateDetections(scanSession, ingestedCertificates, observedAtUtc);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new ScanResultResponse
            {
                Success = true,
                ScanSessionId = scanSession.Id,
                TokensSaved = tokenMap.Count,
                CertificatesSaved = ingestedCertificates.Count,
                Message = "Scan results processed successfully"
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Failed to process scan session {SessionUid}", request.ScanSession.SessionUid);
            throw;
        }
    }

    private async Task<Workstation> UpsertWorkstationAsync(
        WorkstationDto dto,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        var deviceUid = dto.DeviceUid.Trim();
        var hostname = dto.Hostname.Trim();

        var workstation = await _dbContext.Workstations
            .FirstOrDefaultAsync(x => x.DeviceUid == deviceUid, cancellationToken);

        if (workstation is null)
        {
            workstation = new Workstation
            {
                DeviceUid = deviceUid,
                Hostname = hostname,
                FirstSeenAtUtc = observedAtUtc,
                LastSeenAtUtc = observedAtUtc
            };

            _dbContext.Workstations.Add(workstation);
        }
        else
        {
            workstation.Hostname = hostname;
            workstation.LastSeenAtUtc = observedAtUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return workstation;
    }

    private async Task<Agent> UpsertAgentAsync(
        AgentDto dto,
        Workstation workstation,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        var version = dto.Version.Trim();

        var agent = await _dbContext.Agents
            .FirstOrDefaultAsync(x => x.WorkstationId == workstation.Id, cancellationToken);

        if (agent is null)
        {
            agent = new Agent
            {
                WorkstationId = workstation.Id,
                AgentVersion = version,
                LastScanAtUtc = observedAtUtc
            };

            _dbContext.Agents.Add(agent);
        }
        else
        {
            agent.AgentVersion = version;
            agent.LastScanAtUtc = observedAtUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return agent;
    }

    private async Task<Dictionary<string, TokenDevice>> UpsertTokensAsync(
        IReadOnlyCollection<TokenDto> tokens,
        Workstation workstation,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        var tokenMap = new Dictionary<string, TokenDevice>(StringComparer.OrdinalIgnoreCase);

        foreach (var token in tokens)
        {
            var serialNumber = token.SerialNumber.Trim();
            if (string.IsNullOrWhiteSpace(serialNumber))
            {
                _logger.LogWarning("Token entry was skipped because serial number is empty");
                continue;
            }

            var entity = await _dbContext.TokenDevices
                .FirstOrDefaultAsync(x => x.SerialNumber == serialNumber, cancellationToken);

            if (entity is null)
            {
                entity = new TokenDevice
                {
                    SerialNumber = serialNumber,
                    TokenType = token.TokenType.Trim(),
                    Model = token.Model.Trim(),
                    Manufacturer = token.Manufacturer.Trim(),
                    Pkcs11SlotId = token.Pkcs11SlotId.Trim(),
                    WorkstationId = workstation.Id,
                    FirstSeenAtUtc = observedAtUtc,
                    LastSeenAtUtc = observedAtUtc
                };

                _dbContext.TokenDevices.Add(entity);
            }
            else
            {
                entity.TokenType = token.TokenType.Trim();
                entity.Model = token.Model.Trim();
                entity.Manufacturer = token.Manufacturer.Trim();
                entity.Pkcs11SlotId = token.Pkcs11SlotId.Trim();
                entity.WorkstationId = workstation.Id;
                entity.LastSeenAtUtc = observedAtUtc;
            }

            tokenMap[serialNumber] = entity;
        }

        return tokenMap;
    }

    private async Task<List<IngestedCertificate>> UpsertCertificatesAsync(
        IReadOnlyCollection<CertificateDto> certificates,
        IReadOnlyDictionary<string, TokenDevice> tokenMap,
        DateTime observedAtUtc,
        CancellationToken cancellationToken)
    {
        var ingested = new List<IngestedCertificate>();

        foreach (var certificate in certificates)
        {
            var normalizedThumbprint = NormalizeOptional(certificate.Thumbprint);
            var normalizedIssuer = certificate.Issuer.Trim();
            var normalizedSerial = certificate.SerialNumber.Trim();

            Certificate? entity = null;

            if (!string.IsNullOrWhiteSpace(normalizedThumbprint))
            {
                entity = await _dbContext.Certificates
                    .FirstOrDefaultAsync(x => x.Thumbprint == normalizedThumbprint, cancellationToken);
            }

            if (entity is null)
            {
                entity = await _dbContext.Certificates
                    .FirstOrDefaultAsync(
                        x => (x.Thumbprint == null || x.Thumbprint == string.Empty)
                             && x.Issuer == normalizedIssuer
                             && x.SerialNumber == normalizedSerial,
                        cancellationToken);
            }

            var resolvedToken = ResolveToken(tokenMap, certificate.TokenSerialNumber);
            if (resolvedToken is null && !string.IsNullOrWhiteSpace(certificate.TokenSerialNumber))
            {
                _logger.LogWarning(
                    "Certificate with serial {SerialNumber} references token {TokenSerialNumber}, but this token was not found in the payload",
                    normalizedSerial,
                    certificate.TokenSerialNumber);
            }

            if (entity is null)
            {
                entity = new Certificate
                {
                    Thumbprint = normalizedThumbprint,
                    Issuer = normalizedIssuer,
                    Subject = certificate.Subject.Trim(),
                    SerialNumber = normalizedSerial,
                    ValidFromUtc = certificate.ValidFromUtc,
                    ValidToUtc = certificate.ValidToUtc,
                    Algorithm = certificate.Algorithm.Trim(),
                    SourceType = certificate.SourceType.Trim(),
                    TokenDevice = resolvedToken,
                    FirstSeenAtUtc = observedAtUtc,
                    LastSeenAtUtc = observedAtUtc
                };

                _dbContext.Certificates.Add(entity);
            }
            else
            {
                entity.Thumbprint = normalizedThumbprint;
                entity.Issuer = normalizedIssuer;
                entity.Subject = certificate.Subject.Trim();
                entity.SerialNumber = normalizedSerial;
                entity.ValidFromUtc = certificate.ValidFromUtc;
                entity.ValidToUtc = certificate.ValidToUtc;
                entity.Algorithm = certificate.Algorithm.Trim();
                entity.SourceType = certificate.SourceType.Trim();
                entity.TokenDevice = resolvedToken;
                entity.LastSeenAtUtc = observedAtUtc;
            }

            ingested.Add(new IngestedCertificate(entity, resolvedToken));
        }

        return ingested;
    }

    private void CreateCertificateDetections(
        ScanSession scanSession,
        IReadOnlyCollection<IngestedCertificate> ingestedCertificates,
        DateTime detectedAtUtc)
    {
        var uniqueDetections = new HashSet<string>(StringComparer.Ordinal);

        foreach (var ingestedCertificate in ingestedCertificates)
        {
            var tokenIdPart = ingestedCertificate.TokenDevice?.Id.ToString() ?? "null";
            var signature = $"{scanSession.Id}:{ingestedCertificate.Certificate.Id}:{tokenIdPart}";

            if (!uniqueDetections.Add(signature))
            {
                continue;
            }

            _dbContext.CertificateDetections.Add(new CertificateDetection
            {
                ScanSessionId = scanSession.Id,
                CertificateId = ingestedCertificate.Certificate.Id,
                TokenDeviceId = ingestedCertificate.TokenDevice?.Id,
                DetectedAtUtc = detectedAtUtc
            });
        }
    }

    private static TokenDevice? ResolveToken(
        IReadOnlyDictionary<string, TokenDevice> tokenMap,
        string? tokenSerialNumber)
    {
        if (string.IsNullOrWhiteSpace(tokenSerialNumber))
        {
            return null;
        }

        return tokenMap.TryGetValue(tokenSerialNumber.Trim(), out var token)
            ? token
            : null;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static List<string> Validate(ScanResultRequest request)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Workstation.DeviceUid))
        {
            errors.Add("workstation.deviceUid is required");
        }

        if (string.IsNullOrWhiteSpace(request.Workstation.Hostname))
        {
            errors.Add("workstation.hostname is required");
        }

        if (string.IsNullOrWhiteSpace(request.Agent.Version))
        {
            errors.Add("agent.version is required");
        }

        if (string.IsNullOrWhiteSpace(request.ScanSession.SessionUid))
        {
            errors.Add("scanSession.sessionUid is required");
        }

        if (request.ScanSession.FinishedAtUtc < request.ScanSession.StartedAtUtc)
        {
            errors.Add("scanSession.finishedAtUtc must be greater than or equal to scanSession.startedAtUtc");
        }

        foreach (var token in request.Tokens)
        {
            if (string.IsNullOrWhiteSpace(token.SerialNumber))
            {
                errors.Add("token.serialNumber is required");
            }
        }

        foreach (var certificate in request.Certificates)
        {
            if (string.IsNullOrWhiteSpace(certificate.Issuer))
            {
                errors.Add("certificate.issuer is required");
            }

            if (string.IsNullOrWhiteSpace(certificate.Subject))
            {
                errors.Add("certificate.subject is required");
            }

            if (string.IsNullOrWhiteSpace(certificate.SerialNumber))
            {
                errors.Add("certificate.serialNumber is required");
            }

            if (string.IsNullOrWhiteSpace(certificate.Algorithm))
            {
                errors.Add("certificate.algorithm is required");
            }

            if (string.IsNullOrWhiteSpace(certificate.SourceType))
            {
                errors.Add("certificate.sourceType is required");
            }

            if (certificate.ValidToUtc < certificate.ValidFromUtc)
            {
                errors.Add("certificate.validToUtc must be greater than or equal to certificate.validFromUtc");
            }
        }

        return errors;
    }

    private sealed record IngestedCertificate(Certificate Certificate, TokenDevice? TokenDevice);
}

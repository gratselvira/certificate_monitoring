using CertMonitor.Server.Data;
using CertMonitor.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Controllers;

public sealed class CertificatesController : Controller
{
    private readonly AppDbContext _dbContext;

    public CertificatesController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _dbContext.Certificates
            .AsNoTracking()
            .Include(x => x.TokenDevice)
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(x => new CertificateListItemViewModel
            {
                Subject = x.Subject,
                Issuer = x.Issuer,
                SerialNumber = x.SerialNumber,
                Thumbprint = x.Thumbprint,
                ValidFromUtc = x.ValidFromUtc,
                ValidToUtc = x.ValidToUtc,
                Algorithm = x.Algorithm,
                TokenSerialNumber = x.TokenDevice != null ? x.TokenDevice.SerialNumber : null,
                LastSeenAtUtc = x.LastSeenAtUtc
            })
            .ToListAsync(cancellationToken);

        return View(model);
    }
}

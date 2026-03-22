using CertMonitor.Server.Data;
using CertMonitor.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Controllers;

public sealed class ScanSessionsController : Controller
{
    private readonly AppDbContext _dbContext;

    public ScanSessionsController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _dbContext.ScanSessions
            .AsNoTracking()
            .Include(x => x.Workstation)
            .OrderByDescending(x => x.FinishedAtUtc)
            .Select(x => new ScanSessionListItemViewModel
            {
                SessionUid = x.SessionUid,
                StartedAtUtc = x.StartedAtUtc,
                FinishedAtUtc = x.FinishedAtUtc,
                Status = x.Status,
                TokensFoundCount = x.TokensFoundCount,
                CertificatesFoundCount = x.CertificatesFoundCount,
                WorkstationHostname = x.Workstation.Hostname
            })
            .ToListAsync(cancellationToken);

        return View(model);
    }
}

using System.Diagnostics;
using CertMonitor.Server.Data;
using CertMonitor.Server.Models;
using CertMonitor.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Controllers;

public sealed class HomeController : Controller
{
    private readonly AppDbContext _dbContext;

    public HomeController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var latestSession = await _dbContext.ScanSessions
            .AsNoTracking()
            .Include(x => x.Workstation)
            .OrderByDescending(x => x.FinishedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        var model = new DashboardViewModel
        {
            LatestHostname = latestSession?.Workstation.Hostname,
            LastScanAtUtc = latestSession?.FinishedAtUtc,
            TokensCount = await _dbContext.TokenDevices.AsNoTracking().CountAsync(cancellationToken),
            CertificatesCount = await _dbContext.Certificates.AsNoTracking().CountAsync(cancellationToken),
            ScanSessionsCount = await _dbContext.ScanSessions.AsNoTracking().CountAsync(cancellationToken)
        };

        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

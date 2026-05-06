using CertMonitor.Server.Data;
using CertMonitor.Server.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CertMonitor.Server.Controllers;

public sealed class TokensController : Controller
{
    private readonly AppDbContext _dbContext;

    public TokensController(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var model = await _dbContext.TokenDevices
            .AsNoTracking()
            .Include(x => x.Workstation)
            .OrderByDescending(x => x.LastSeenAtUtc)
            .Select(x => new TokenListItemViewModel
            {
                SerialNumber = x.SerialNumber,
                TokenType = x.TokenType,
                Model = x.Model,
                Manufacturer = x.Manufacturer,
                Pkcs11SlotId = x.Pkcs11SlotId,
                WorkstationHostname = x.Workstation.Hostname,
                LastSeenAtUtc = x.LastSeenAtUtc
            })
            .ToListAsync(cancellationToken);

        return View(model);
    }
}

using CertMonitor.Server.Models.Dto;
using CertMonitor.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace CertMonitor.Server.Controllers.Api;

[ApiController]
[Route("api/scan-results")]
public sealed class ScanResultsController : ControllerBase
{
    private readonly IScanIngestionService _scanIngestionService;
    private readonly ILogger<ScanResultsController> _logger;

    public ScanResultsController(
        IScanIngestionService scanIngestionService,
        ILogger<ScanResultsController> logger)
    {
        _scanIngestionService = scanIngestionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ScanResultRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            var validationErrors = ModelState.Values
                .SelectMany(x => x.Errors)
                .Select(x => string.IsNullOrWhiteSpace(x.ErrorMessage) ? "Invalid request payload" : x.ErrorMessage)
                .ToArray();

            return BadRequest(new ScanResultResponse
            {
                Success = false,
                Message = string.Join("; ", validationErrors)
            });
        }

        try
        {
            var result = await _scanIngestionService.ProcessAsync(request, cancellationToken);
            return result.Success ? Ok(result) : BadRequest(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error while processing scan results");

            return StatusCode(StatusCodes.Status500InternalServerError, new ScanResultResponse
            {
                Success = false,
                Message = "Unexpected server error while processing scan results"
            });
        }
    }
}

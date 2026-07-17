using Microsoft.AspNetCore.Mvc;
using MidisSqlAi.Api.Models;
using MidisSqlAi.Api.Services;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/database")]
public sealed class DatabaseController : ControllerBase
{
    private readonly IDatabaseHealthService _databaseHealthService;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(
        IDatabaseHealthService databaseHealthService,
        ILogger<DatabaseController> logger)
    {
        _databaseHealthService = databaseHealthService;
        _logger = logger;
    }

    [HttpGet("health")]
    [ProducesResponseType<DatabaseHealthResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetHealth(
        CancellationToken cancellationToken)
    {
        try
        {
            DatabaseHealthResult result =
                await _databaseHealthService.CheckAsync(cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "The database health check failed.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "Unhealthy",
                    message = "The API could not connect to the database."
                });
        }
    }
}
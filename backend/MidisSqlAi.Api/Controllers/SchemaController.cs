using Microsoft.AspNetCore.Mvc;
using MidisSqlAi.Api.Models;
using MidisSqlAi.Api.Services;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/schema")]
public sealed class SchemaController : ControllerBase
{
    private readonly IDatabaseSchemaService _schemaService;
    private readonly ILogger<SchemaController> _logger;

    public SchemaController(
        IDatabaseSchemaService schemaService,
        ILogger<SchemaController> logger)
    {
        _schemaService = schemaService;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType<DatabaseSchemaResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetSchema(
        CancellationToken cancellationToken)
    {
        try
        {
            DatabaseSchemaResult result =
                await _schemaService.GetSchemaAsync(cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Reading the database schema failed.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new
                {
                    status = "Unhealthy",
                    message =
                        "The API could not read the database schema."
                });
        }
    }

    [HttpGet("prompt")]
    [Produces("text/plain")]
    [ProducesResponseType<string>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPromptSchema(
        CancellationToken cancellationToken)
    {
        try
        {
            DatabaseSchemaResult result =
                await _schemaService.GetSchemaAsync(cancellationToken);

            return Content(
                result.PromptText,
                "text/plain");
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Creating the prompt schema failed.");

            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                "The API could not create the prompt schema.");
        }
    }
}
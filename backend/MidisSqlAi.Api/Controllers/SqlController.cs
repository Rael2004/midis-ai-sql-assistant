using Microsoft.AspNetCore.Mvc;
using MidisSqlAi.Api.Models;
using MidisSqlAi.Api.Services;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/sql")]
public sealed class SqlController : ControllerBase
{
    private const int MaximumSqlLength = 20_000;

    private readonly ISqlValidationService _validationService;
    private readonly ILogger<SqlController> _logger;

    public SqlController(
        ISqlValidationService validationService,
        ILogger<SqlController> logger)
    {
        _validationService = validationService;
        _logger = logger;
    }

    [HttpPost("validate")]
    [ProducesResponseType<SqlValidationResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateSql(
        [FromBody] ValidateSqlRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return BadRequest(new
            {
                message = "SQL is required."
            });
        }

        if (request.Sql.Length > MaximumSqlLength)
        {
            return BadRequest(new
            {
                message =
                    $"SQL cannot exceed " +
                    $"{MaximumSqlLength} characters."
            });
        }

        try
        {
            SqlValidationResult result =
                await _validationService.ValidateAsync(
                    request.Sql,
                    cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unexpected SQL-validation error occurred.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "An unexpected SQL-validation error occurred."
                });
        }
    }
}
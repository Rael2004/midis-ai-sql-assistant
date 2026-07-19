using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MidisSqlAi.Api.Models;
using MidisSqlAi.Api.Services;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/query")]
public sealed class QueryController : ControllerBase
{
    private const int MaximumSqlLength = 20_000;

    private readonly IQueryExecutionService _executionService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(
        IQueryExecutionService executionService,
        ILogger<QueryController> logger)
    {
        _executionService = executionService;
        _logger = logger;
    }

    [HttpPost("execute")]
    [ProducesResponseType<ExecuteSqlResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ExecuteSqlResponse>(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(
        StatusCodes.Status504GatewayTimeout)]
    public async Task<IActionResult> Execute(
        [FromBody] ExecuteSqlRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Sql))
        {
            return BadRequest(new
            {
                message = "SQL is required."
            });
        }

        string sql = request.Sql.Trim();

        if (sql.Length > MaximumSqlLength)
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
            ExecuteSqlResponse response =
                await _executionService.ExecuteAsync(
                    sql,
                    cancellationToken);

            if (!response.Executed)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
        catch (SqlException exception)
            when (exception.Number == -2)
        {
            _logger.LogWarning(
                exception,
                "SQL execution exceeded the timeout.");

            return StatusCode(
                StatusCodes.Status504GatewayTimeout,
                new
                {
                    message =
                        "The query exceeded the execution timeout."
                });
        }
        catch (SqlException exception)
        {
            _logger.LogWarning(
                exception,
                "SQL Server rejected the query.");

            return UnprocessableEntity(new
            {
                message =
                    "SQL Server could not execute the query."
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "An unexpected query-execution error occurred.");

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new
                {
                    message =
                        "An unexpected query-execution error occurred."
                });
        }
    }
}
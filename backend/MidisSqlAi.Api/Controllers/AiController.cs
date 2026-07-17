using Microsoft.AspNetCore.Mvc;
using MidisSqlAi.Api.Models;
using MidisSqlAi.Api.Services;

namespace MidisSqlAi.Api.Controllers;

[ApiController]
[Route("api/ai")]
public sealed class AiController : ControllerBase
{
    private const int MaximumQuestionLength = 1000;

    private readonly ISqlGenerationService _sqlGenerationService;
    private readonly ILogger<AiController> _logger;

    public AiController(
        ISqlGenerationService sqlGenerationService,
        ILogger<AiController> logger)
    {
        _sqlGenerationService = sqlGenerationService;
        _logger = logger;
    }

    [HttpPost("generate-sql")]
    [ProducesResponseType<GenerateSqlResult>(
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> GenerateSql(
        [FromBody] GenerateSqlRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
        {
            return BadRequest(new
            {
                message = "Question is required."
            });
        }

        string question = request.Question.Trim();

        if (question.Length > MaximumQuestionLength)
        {
            return BadRequest(new
            {
                message =
                    $"Question cannot exceed " +
                    $"{MaximumQuestionLength} characters."
            });
        }

        try
        {
            GenerateSqlResult result =
                await _sqlGenerationService.GenerateSqlAsync(
                    question,
                    cancellationToken);

            return Ok(result);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "SQL generation failed.");

            return StatusCode(
                StatusCodes.Status502BadGateway,
                new
                {
                    message =
                        "The API could not generate SQL using the AI model."
                });
        }
    }
}
#pragma warning disable OPENAI001

using System.ClientModel;
using Microsoft.Extensions.Configuration;
using MidisSqlAi.Api.Models;
using OpenAI.Responses;

namespace MidisSqlAi.Api.Services;

public sealed class SqlGenerationService : ISqlGenerationService
{
    private readonly IDatabaseSchemaService _schemaService;

    private readonly ISqlValidationService _validationService;
    private readonly ResponsesClient _responsesClient;
    private readonly string _deploymentName;
    private readonly string _instructions;

    public SqlGenerationService(
        IConfiguration configuration,
        IWebHostEnvironment environment,
        IDatabaseSchemaService schemaService,
        ISqlValidationService validationService)
    {
        _schemaService = schemaService;
        _validationService = validationService;

        string endpoint =
            configuration["AzureOpenAI:Endpoint"]
            ?? throw new InvalidOperationException(
                "AzureOpenAI:Endpoint was not configured.");

        string apiKey =
            configuration["AzureOpenAI:ApiKey"]
            ?? throw new InvalidOperationException(
                "AzureOpenAI:ApiKey was not configured.");

        _deploymentName =
            configuration["AzureOpenAI:DeploymentName"]
            ?? throw new InvalidOperationException(
                "AzureOpenAI:DeploymentName was not configured.");

        if (!Uri.TryCreate(
                endpoint,
                UriKind.Absolute,
                out Uri? endpointUri))
        {
            throw new InvalidOperationException(
                "AzureOpenAI:Endpoint is not a valid absolute URL.");
        }

        _responsesClient = new ResponsesClient(
            credential: new ApiKeyCredential(apiKey),
            options: new ResponsesClientOptions
            {
                Endpoint = endpointUri
            });

        string instructionsPath = Path.Combine(
            environment.ContentRootPath,
            "Prompts",
            "SqlGenerationInstructions.txt");

        if (!File.Exists(instructionsPath))
        {
            throw new FileNotFoundException(
                "The SQL-generation instructions file was not found.",
                instructionsPath);
        }

        _instructions = File.ReadAllText(instructionsPath);
    }

    public async Task<GenerateSqlResult> GenerateSqlAsync(
        string question,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);

        DatabaseSchemaResult schema =
            await _schemaService.GetSchemaAsync(cancellationToken);

        string schemaContext = $"""
            Use the following trusted Microsoft SQL Server schema.

            {schema.PromptText}
            """;

        CreateResponseOptions options = new()
        {
            Model = _deploymentName,
            StoredOutputEnabled = false,
            InputItems =
            {
                ResponseItem.CreateSystemMessageItem(_instructions),
                ResponseItem.CreateSystemMessageItem(schemaContext),
                ResponseItem.CreateUserMessageItem(question)
            }
        };

        ResponseResult response =
            await _responsesClient.CreateResponseAsync(options);

        string modelOutput = response.GetOutputText().Trim();

if (string.Equals(
        modelOutput,
        "CANNOT_ANSWER",
        StringComparison.OrdinalIgnoreCase))
{
    return new GenerateSqlResult(
        CanAnswer: false,
        GeneratedSql: null,
        IsValid: false,
        ValidationErrors: Array.Empty<string>(),
        ModelDeployment: _deploymentName);
}

string normalizedSql = NormalizeModelOutput(modelOutput);

SqlValidationResult validation =
    await _validationService.ValidateAsync(
        normalizedSql,
        cancellationToken);

return new GenerateSqlResult(
    CanAnswer: true,
    GeneratedSql: normalizedSql,
    IsValid: validation.IsValid,
    ValidationErrors: validation.Errors,
    ModelDeployment: _deploymentName);
    }

    private static string NormalizeModelOutput(string output)
    {
        string normalized = output.Trim();

        if (normalized.StartsWith(
                "```sql",
                StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[6..];
        }
        else if (normalized.StartsWith("```"))
        {
            normalized = normalized[3..];
        }

        if (normalized.EndsWith("```"))
        {
            normalized = normalized[..^3];
        }

        return normalized.Trim();
    }
}
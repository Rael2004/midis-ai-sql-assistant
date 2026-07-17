namespace MidisSqlAi.Api.Models;

public sealed record GenerateSqlResult(
    bool CanAnswer,
    string? GeneratedSql,
    string ModelDeployment
);
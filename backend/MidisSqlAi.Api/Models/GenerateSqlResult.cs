namespace MidisSqlAi.Api.Models;

public sealed record GenerateSqlResult(
    bool CanAnswer,
    string? GeneratedSql,
    bool IsValid,
    IReadOnlyList<string> ValidationErrors,
    string ModelDeployment
);
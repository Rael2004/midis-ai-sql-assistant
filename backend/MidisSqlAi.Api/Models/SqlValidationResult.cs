namespace MidisSqlAi.Api.Models;

public sealed record SqlValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors
);
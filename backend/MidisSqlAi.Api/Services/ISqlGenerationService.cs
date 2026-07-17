using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public interface ISqlGenerationService
{
    Task<GenerateSqlResult> GenerateSqlAsync(
        string question,
        CancellationToken cancellationToken = default);
}
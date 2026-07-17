using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public interface ISqlValidationService
{
    Task<SqlValidationResult> ValidateAsync(
        string sql,
        CancellationToken cancellationToken = default);
}
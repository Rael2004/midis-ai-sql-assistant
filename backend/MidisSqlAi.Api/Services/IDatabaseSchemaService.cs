using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public interface IDatabaseSchemaService
{
    Task<DatabaseSchemaResult> GetSchemaAsync(
        CancellationToken cancellationToken = default);
}
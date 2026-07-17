using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public interface IDatabaseHealthService
{
    Task<DatabaseHealthResult> CheckAsync(
        CancellationToken cancellationToken = default);
}
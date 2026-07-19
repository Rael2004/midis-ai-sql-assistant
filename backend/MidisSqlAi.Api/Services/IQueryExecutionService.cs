using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public interface IQueryExecutionService
{
    Task<ExecuteSqlResponse> ExecuteAsync(
        string sql,
        CancellationToken cancellationToken = default);
}
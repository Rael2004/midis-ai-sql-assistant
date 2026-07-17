using Microsoft.Data.SqlClient;
using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public sealed class DatabaseHealthService : IDatabaseHealthService
{
    private readonly string _connectionString;

    public DatabaseHealthService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");
    }

    public async Task<DatabaseHealthResult> CheckAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            SELECT
                DB_NAME() AS DatabaseName,
                CAST(SERVERPROPERTY('ServerName') AS NVARCHAR(128)) AS ServerName,
                (SELECT COUNT(*) FROM dbo.Clients) AS ClientCount,
                (SELECT COUNT(*) FROM dbo.Tickets) AS TicketCount;
            """;

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 10
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
        {
            throw new InvalidOperationException(
                "The database health query returned no result.");
        }

        return new DatabaseHealthResult(
            CanConnect: true,
            DatabaseName: reader.GetString(
                reader.GetOrdinal("DatabaseName")),
            ServerName: reader.GetString(
                reader.GetOrdinal("ServerName")),
            ClientCount: reader.GetInt32(
                reader.GetOrdinal("ClientCount")),
            TicketCount: reader.GetInt32(
                reader.GetOrdinal("TicketCount"))
        );
    }
}
using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public sealed class QueryExecutionService
    : IQueryExecutionService
{
    private const int MaximumReturnedRows = 100;
    private const int CommandTimeoutSeconds = 10;

    private readonly string _readOnlyConnectionString;
    private readonly ISqlValidationService _validationService;

    public QueryExecutionService(
        IConfiguration configuration,
        ISqlValidationService validationService)
    {
        _validationService = validationService;

        _readOnlyConnectionString =
            configuration.GetConnectionString(
                "ReadOnlyConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'ReadOnlyConnection' " +
                "was not configured.");
    }

    public async Task<ExecuteSqlResponse> ExecuteAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        /*
         * Validate here, immediately before execution.
         *
         * We do not trust the browser, frontend, or an earlier
         * validation result.
         */
        SqlValidationResult validation =
            await _validationService.ValidateAsync(
                sql,
                cancellationToken);

        if (!validation.IsValid)
        {
            return new ExecuteSqlResponse(
                Executed: false,
                ValidationErrors: validation.Errors,
                Result: null);
        }

        var stopwatch = Stopwatch.StartNew();

        await using var connection =
            new SqlConnection(_readOnlyConnectionString);

        await connection.OpenAsync(cancellationToken);

        await using var command =
            new SqlCommand(sql, connection)
            {
                CommandTimeout = CommandTimeoutSeconds
            };

        /*
         * SingleResult indicates that only one result set is
         * expected.
         *
         * SequentialAccess avoids unnecessarily buffering large
         * field values in memory.
         */
        CommandBehavior behavior =
            CommandBehavior.SingleResult |
            CommandBehavior.SequentialAccess;

        await using SqlDataReader reader =
            await command.ExecuteReaderAsync(
                behavior,
                cancellationToken);

        var columns = new List<QueryColumn>(
            reader.FieldCount);

        for (int index = 0;
             index < reader.FieldCount;
             index++)
        {
            columns.Add(
                new QueryColumn(
                    Name: reader.GetName(index),
                    DataType: reader.GetDataTypeName(index)
                )
            );
        }

        var rows =
            new List<IReadOnlyList<object?>>(
                MaximumReturnedRows);

        bool wasTruncated = false;

        while (await reader.ReadAsync(cancellationToken))
        {
            /*
             * We read one additional row only to detect whether
             * more than 100 rows were available.
             */
            if (rows.Count >= MaximumReturnedRows)
            {
                wasTruncated = true;
                break;
            }

            var row =
                new List<object?>(reader.FieldCount);

            for (int index = 0;
                 index < reader.FieldCount;
                 index++)
            {
                if (reader.IsDBNull(index))
                {
                    row.Add(null);
                    continue;
                }

                object value = reader.GetValue(index);

                /*
                 * JSON serializers represent byte arrays as
                 * Base64. Converting explicitly makes this
                 * behavior clear and predictable.
                 */
                if (value is byte[] bytes)
                {
                    row.Add(Convert.ToBase64String(bytes));
                }
                else
                {
                    row.Add(value);
                }
            }

            rows.Add(row);
        }

        stopwatch.Stop();

        var result = new QueryExecutionResult(
            Columns: columns,
            Rows: rows,
            ReturnedRowCount: rows.Count,
            WasTruncated: wasTruncated,
            MaximumRows: MaximumReturnedRows,
            DurationMilliseconds:
                stopwatch.ElapsedMilliseconds);

        return new ExecuteSqlResponse(
            Executed: true,
            ValidationErrors: Array.Empty<string>(),
            Result: result);
    }
}
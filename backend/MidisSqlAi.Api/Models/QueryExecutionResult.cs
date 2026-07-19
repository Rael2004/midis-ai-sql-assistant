namespace MidisSqlAi.Api.Models;

/// <summary>
/// Describes one column returned by SQL Server.
/// </summary>
public sealed record QueryColumn(
    string Name,
    string DataType
);

/// <summary>
/// Contains the rows and metadata returned by a query.
/// </summary>
public sealed record QueryExecutionResult(
    IReadOnlyList<QueryColumn> Columns,
    IReadOnlyList<IReadOnlyList<object?>> Rows,
    int ReturnedRowCount,
    bool WasTruncated,
    int MaximumRows,
    long DurationMilliseconds
);

/// <summary>
/// Complete response from the controlled execution service.
/// </summary>
public sealed record ExecuteSqlResponse(
    bool Executed,
    IReadOnlyList<string> ValidationErrors,
    QueryExecutionResult? Result
);
namespace MidisSqlAi.Api.Models;

/// <summary>
/// The complete database schema returned by the API.
/// </summary>
public sealed record DatabaseSchemaResult(
    string DatabaseName,
    IReadOnlyList<TableSchema> Tables,
    IReadOnlyList<ForeignKeySchema> ForeignKeys,
    string PromptText
);

/// <summary>
/// Describes one table and all of its columns.
/// </summary>
public sealed class TableSchema
{
    public TableSchema(string schemaName, string tableName)
    {
        SchemaName = schemaName;
        TableName = tableName;
    }

    public string SchemaName { get; }

    public string TableName { get; }

    public string QualifiedName => $"{SchemaName}.{TableName}";

    public List<ColumnSchema> Columns { get; } = new();
}

/// <summary>
/// Describes one database column.
/// </summary>
public sealed record ColumnSchema(
    string ColumnName,
    string DataType,
    bool IsNullable,
    bool IsPrimaryKey
);

/// <summary>
/// Describes one foreign-key relationship.
/// </summary>
public sealed record ForeignKeySchema(
    string ConstraintName,
    string FromSchema,
    string FromTable,
    string FromColumn,
    string ToSchema,
    string ToTable,
    string ToColumn
);
using System.Text;
using Microsoft.Data.SqlClient;
using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public sealed class DatabaseSchemaService : IDatabaseSchemaService
{
    private readonly string _connectionString;

    public DatabaseSchemaService(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Connection string 'DefaultConnection' was not found.");
    }

    public async Task<DatabaseSchemaResult> GetSchemaAsync(
        CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_connectionString);

        await connection.OpenAsync(cancellationToken);

        const string sql = """
            -- First result set: tables and columns
            SELECT
                tableSchema.name AS SchemaName,
                tableInfo.name AS TableName,
                columnInfo.column_id AS ColumnOrder,
                columnInfo.name AS ColumnName,
                typeInfo.name AS TypeName,
                columnInfo.max_length AS MaxLength,
                columnInfo.precision AS NumericPrecision,
                columnInfo.scale AS NumericScale,
                columnInfo.is_nullable AS IsNullable,
                CAST(
                    CASE
                        WHEN primaryKeyColumn.column_id IS NULL THEN 0
                        ELSE 1
                    END
                    AS bit
                ) AS IsPrimaryKey
            FROM sys.tables AS tableInfo
            INNER JOIN sys.schemas AS tableSchema
                ON tableInfo.schema_id = tableSchema.schema_id
            INNER JOIN sys.columns AS columnInfo
                ON tableInfo.object_id = columnInfo.object_id
            INNER JOIN sys.types AS typeInfo
                ON columnInfo.user_type_id = typeInfo.user_type_id
            LEFT JOIN
            (
                SELECT
                    indexColumn.object_id,
                    indexColumn.column_id
                FROM sys.indexes AS indexInfo
                INNER JOIN sys.index_columns AS indexColumn
                    ON indexInfo.object_id = indexColumn.object_id
                    AND indexInfo.index_id = indexColumn.index_id
                WHERE indexInfo.is_primary_key = 1
            ) AS primaryKeyColumn
                ON columnInfo.object_id = primaryKeyColumn.object_id
                AND columnInfo.column_id = primaryKeyColumn.column_id
            WHERE tableInfo.is_ms_shipped = 0
            ORDER BY
                tableSchema.name,
                tableInfo.name,
                columnInfo.column_id;

            -- Second result set: foreign-key relationships
            SELECT
                foreignKey.name AS ConstraintName,
                fromSchema.name AS FromSchema,
                fromTable.name AS FromTable,
                fromColumn.name AS FromColumn,
                toSchema.name AS ToSchema,
                toTable.name AS ToTable,
                toColumn.name AS ToColumn
            FROM sys.foreign_keys AS foreignKey
            INNER JOIN sys.foreign_key_columns AS foreignKeyColumn
                ON foreignKey.object_id =
                   foreignKeyColumn.constraint_object_id
            INNER JOIN sys.tables AS fromTable
                ON foreignKeyColumn.parent_object_id =
                   fromTable.object_id
            INNER JOIN sys.schemas AS fromSchema
                ON fromTable.schema_id = fromSchema.schema_id
            INNER JOIN sys.columns AS fromColumn
                ON foreignKeyColumn.parent_object_id =
                   fromColumn.object_id
                AND foreignKeyColumn.parent_column_id =
                    fromColumn.column_id
            INNER JOIN sys.tables AS toTable
                ON foreignKeyColumn.referenced_object_id =
                   toTable.object_id
            INNER JOIN sys.schemas AS toSchema
                ON toTable.schema_id = toSchema.schema_id
            INNER JOIN sys.columns AS toColumn
                ON foreignKeyColumn.referenced_object_id =
                   toColumn.object_id
                AND foreignKeyColumn.referenced_column_id =
                    toColumn.column_id
            ORDER BY
                fromSchema.name,
                fromTable.name,
                foreignKey.name;
            """;

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 10
        };

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        var tables = new List<TableSchema>();

        var tableLookup =
            new Dictionary<string, TableSchema>(
                StringComparer.OrdinalIgnoreCase);

        // Read the first result set: tables and columns.
        while (await reader.ReadAsync(cancellationToken))
        {
            string schemaName =
                reader.GetString(reader.GetOrdinal("SchemaName"));

            string tableName =
                reader.GetString(reader.GetOrdinal("TableName"));

            string lookupKey = $"{schemaName}.{tableName}";

            if (!tableLookup.TryGetValue(
                    lookupKey,
                    out TableSchema? table))
            {
                table = new TableSchema(schemaName, tableName);

                tableLookup.Add(lookupKey, table);
                tables.Add(table);
            }

            string typeName =
                reader.GetString(reader.GetOrdinal("TypeName"));

            short maxLength =
                reader.GetInt16(reader.GetOrdinal("MaxLength"));

            byte precision =
                reader.GetByte(reader.GetOrdinal("NumericPrecision"));

            byte scale =
                reader.GetByte(reader.GetOrdinal("NumericScale"));

            string formattedType = FormatSqlType(
                typeName,
                maxLength,
                precision,
                scale);

            table.Columns.Add(
                new ColumnSchema(
                    ColumnName: reader.GetString(
                        reader.GetOrdinal("ColumnName")),
                    DataType: formattedType,
                    IsNullable: reader.GetBoolean(
                        reader.GetOrdinal("IsNullable")),
                    IsPrimaryKey: reader.GetBoolean(
                        reader.GetOrdinal("IsPrimaryKey"))
                )
            );
        }

        var foreignKeys = new List<ForeignKeySchema>();

        // Move from the column result set to the foreign-key result set.
        if (await reader.NextResultAsync(cancellationToken))
        {
            while (await reader.ReadAsync(cancellationToken))
            {
                foreignKeys.Add(
                    new ForeignKeySchema(
                        ConstraintName: reader.GetString(
                            reader.GetOrdinal("ConstraintName")),
                        FromSchema: reader.GetString(
                            reader.GetOrdinal("FromSchema")),
                        FromTable: reader.GetString(
                            reader.GetOrdinal("FromTable")),
                        FromColumn: reader.GetString(
                            reader.GetOrdinal("FromColumn")),
                        ToSchema: reader.GetString(
                            reader.GetOrdinal("ToSchema")),
                        ToTable: reader.GetString(
                            reader.GetOrdinal("ToTable")),
                        ToColumn: reader.GetString(
                            reader.GetOrdinal("ToColumn"))
                    )
                );
            }
        }

        string promptText = BuildPromptText(
            connection.Database,
            tables,
            foreignKeys);

        return new DatabaseSchemaResult(
            DatabaseName: connection.Database,
            Tables: tables,
            ForeignKeys: foreignKeys,
            PromptText: promptText
        );
    }

    private static string FormatSqlType(
        string typeName,
        short maxLength,
        byte precision,
        byte scale)
    {
        return typeName.ToLowerInvariant() switch
        {
            "nvarchar" or "nchar" =>
                $"{typeName}({FormatUnicodeLength(maxLength)})",

            "varchar" or "char" or "varbinary" or "binary" =>
                $"{typeName}({FormatStandardLength(maxLength)})",

            "decimal" or "numeric" =>
                $"{typeName}({precision},{scale})",

            "datetime2" or "datetimeoffset" or "time" =>
                $"{typeName}({scale})",

            _ => typeName
        };
    }

    private static string FormatUnicodeLength(short maxLength)
    {
        if (maxLength == -1)
        {
            return "max";
        }

        // SQL Server stores nvarchar/nchar max_length in bytes.
        // Unicode characters use two bytes each.
        return (maxLength / 2).ToString();
    }

    private static string FormatStandardLength(short maxLength)
    {
        return maxLength == -1
            ? "max"
            : maxLength.ToString();
    }

    private static string BuildPromptText(
        string databaseName,
        IReadOnlyList<TableSchema> tables,
        IReadOnlyList<ForeignKeySchema> foreignKeys)
    {
        var text = new StringBuilder();

        text.AppendLine($"Database: {databaseName}");
        text.AppendLine();
        text.AppendLine("Tables:");

        foreach (TableSchema table in tables)
        {
            text.AppendLine($"{table.QualifiedName} (");

            for (int index = 0;
                 index < table.Columns.Count;
                 index++)
            {
                ColumnSchema column = table.Columns[index];

                string nullability =
                    column.IsNullable ? "NULL" : "NOT NULL";

                string primaryKey =
                    column.IsPrimaryKey ? " PRIMARY KEY" : string.Empty;

                string comma =
                    index < table.Columns.Count - 1 ? "," : string.Empty;

                text.AppendLine(
                    $"  {column.ColumnName} " +
                    $"{column.DataType} " +
                    $"{nullability}" +
                    $"{primaryKey}" +
                    $"{comma}");
            }

            text.AppendLine(")");
            text.AppendLine();
        }

        text.AppendLine("Relationships:");

        if (foreignKeys.Count == 0)
        {
            text.AppendLine("None");
        }
        else
        {
            foreach (ForeignKeySchema foreignKey in foreignKeys)
            {
                text.AppendLine(
                    $"{foreignKey.FromSchema}." +
                    $"{foreignKey.FromTable}." +
                    $"{foreignKey.FromColumn} -> " +
                    $"{foreignKey.ToSchema}." +
                    $"{foreignKey.ToTable}." +
                    $"{foreignKey.ToColumn}");
            }
        }

        return text.ToString();
    }
}
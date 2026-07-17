using Microsoft.SqlServer.TransactSql.ScriptDom;
using MidisSqlAi.Api.Models;

namespace MidisSqlAi.Api.Services;

public sealed class SqlValidationService : ISqlValidationService
{
    private const int MaximumSqlLength = 20_000;

    private readonly IDatabaseSchemaService _schemaService;

    public SqlValidationService(
        IDatabaseSchemaService schemaService)
    {
        _schemaService = schemaService;
    }

    public async Task<SqlValidationResult> ValidateAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(sql))
        {
            errors.Add("SQL cannot be empty.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        string normalizedSql = sql.Trim();

        if (normalizedSql.Length > MaximumSqlLength)
        {
            errors.Add(
                $"SQL cannot exceed {MaximumSqlLength} characters.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        /*
         * TSql160Parser understands SQL Server 2022 syntax.
         *
         * true means quoted identifiers are enabled, so syntax such as:
         * SELECT [Status] FROM dbo.Tickets
         * is parsed correctly.
         */
        var parser = new TSql160Parser(
            initialQuotedIdentifiers: true);

        TSqlFragment fragment;
        IList<ParseError> parseErrors;

        using (var reader = new StringReader(normalizedSql))
        {
            fragment = parser.Parse(
                reader,
                out parseErrors);
        }

        if (parseErrors.Count > 0)
        {
            foreach (ParseError parseError in parseErrors)
            {
                errors.Add(
                    $"SQL syntax error at line " +
                    $"{parseError.Line}, column " +
                    $"{parseError.Column}: " +
                    $"{parseError.Message}");
            }

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        if (fragment is not TSqlScript script)
        {
            errors.Add(
                "The SQL could not be parsed as a complete script.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        /*
         * A batch is a group of statements separated by GO.
         * We permit only one batch.
         */
        if (script.Batches.Count != 1)
        {
            errors.Add(
                "Exactly one SQL batch is allowed.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        TSqlBatch batch = script.Batches[0];

        /*
         * Reject:
         *
         * SELECT ...;
         * DROP TABLE ...;
         *
         * because it contains two statements.
         */
        if (batch.Statements.Count != 1)
        {
            errors.Add(
                "Exactly one SQL statement is allowed.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        TSqlStatement statement = batch.Statements[0];

        /*
         * Only a SelectStatement is allowed.
         *
         * INSERT, UPDATE, DELETE, EXECUTE, CREATE,
         * DROP, ALTER, MERGE, and other statements
         * all have different ScriptDOM types.
         */
        if (statement is not SelectStatement selectStatement)
        {
            errors.Add(
                "Only a read-only SELECT statement is allowed.");

            return new SqlValidationResult(
                IsValid: false,
                Errors: errors);
        }

        /*
         * SELECT INTO is syntactically a SELECT but creates
         * a new table, so it must be rejected.
         *
         * Example:
         * SELECT * INTO dbo.Copy FROM dbo.Tickets;
         */
        if (selectStatement.Into is not null)
        {
            errors.Add(
                "SELECT INTO is not allowed because it creates a table.");
        }

        /*
         * Collect common-table-expression names so a query such as:
         *
         * WITH TicketTotals AS (...)
         * SELECT ... FROM TicketTotals;
         *
         * does not incorrectly treat TicketTotals as a database table.
         */
        var cteCollector = new CteNameCollector();

        fragment.Accept(cteCollector);

        DatabaseSchemaResult databaseSchema =
            await _schemaService.GetSchemaAsync(
                cancellationToken);

        var allowedTables = databaseSchema.Tables
            .Select(table => table.QualifiedName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var queryVisitor = new QuerySafetyVisitor(
            allowedTables,
            cteCollector.Names);

        fragment.Accept(queryVisitor);

        errors.AddRange(queryVisitor.Errors);

        return new SqlValidationResult(
            IsValid: errors.Count == 0,
            Errors: errors);
    }

    private sealed class CteNameCollector
        : TSqlFragmentVisitor
    {
        public HashSet<string> Names { get; } =
            new(StringComparer.OrdinalIgnoreCase);

        public override void ExplicitVisit(
            CommonTableExpression node)
        {
            Names.Add(node.ExpressionName.Value);
        }
    }

    private sealed class QuerySafetyVisitor
        : TSqlFragmentVisitor
    {
        private readonly HashSet<string> _allowedTables;
        private readonly HashSet<string> _cteNames;

        public QuerySafetyVisitor(
            HashSet<string> allowedTables,
            HashSet<string> cteNames)
        {
            _allowedTables = allowedTables;
            _cteNames = cteNames;
        }

        public List<string> Errors { get; } = new();

        /*
         * Validate every directly named table or CTE.
         */
        public override void ExplicitVisit(
            NamedTableReference node)
        {
            SchemaObjectName objectName =
                node.SchemaObject;

            if (node.TableHints.Count > 0)
            {
                Errors.Add(
                    "SQL table hints are not allowed.");
            }

            if (objectName.ServerIdentifier is not null)
            {
                Errors.Add(
                    "Cross-server table references are not allowed.");

                return;
            }

            if (objectName.DatabaseIdentifier is not null)
            {
                Errors.Add(
                    "Cross-database table references are not allowed.");

                return;
            }

            string tableName =
                objectName.BaseIdentifier.Value;

            string? schemaName =
                objectName.SchemaIdentifier?.Value;

            /*
             * A one-part name may refer to a CTE:
             *
             * FROM TicketTotals
             */
            if (schemaName is null &&
                _cteNames.Contains(tableName))
            {
                return;
            }

            /*
             * For a normal database table, require the schema
             * explicitly. For example:
             *
             * dbo.Tickets
             *
             * rather than only:
             *
             * Tickets
             */
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                Errors.Add(
                    $"Table '{tableName}' must be schema-qualified, " +
                    $"for example 'dbo.{tableName}'.");

                return;
            }

            string qualifiedName =
                $"{schemaName}.{tableName}";

            if (!_allowedTables.Contains(qualifiedName))
            {
                Errors.Add(
                    $"Table '{qualifiedName}' is not included " +
                    $"in the approved database schema.");
            }
        }

        /*
         * SELECT * is rejected because the application should
         * return only fields needed to answer the question.
         */
        public override void ExplicitVisit(
            SelectStarExpression node)
        {
            Errors.Add(
                "SELECT * is not allowed. " +
                "The query must specify the required columns.");
        }

        /*
         * Reject external or indirect table sources.
         */

        public override void ExplicitVisit(
            OpenRowsetTableReference node)
        {
            Errors.Add(
                "OPENROWSET is not allowed.");
        }

        public override void ExplicitVisit(
            OpenQueryTableReference node)
        {
            Errors.Add(
                "OPENQUERY is not allowed.");
        }

        public override void ExplicitVisit(
            SchemaObjectFunctionTableReference node)
        {
            Errors.Add(
                "Table-valued function references are not allowed.");
        }

        public override void ExplicitVisit(
            BuiltInFunctionTableReference node)
        {
            Errors.Add(
                "Built-in table-function references are not allowed.");
        }

        public override void ExplicitVisit(
            OpenJsonTableReference node)
        {
            Errors.Add(
                "OPENJSON table references are not allowed.");
        }

        public override void ExplicitVisit(
            OpenXmlTableReference node)
        {
            Errors.Add(
                "OPENXML table references are not allowed.");
        }

        public override void ExplicitVisit(
            VariableTableReference node)
        {
            Errors.Add(
                "Table-variable references are not allowed.");
        }

        public override void ExplicitVisit(
            FullTextTableReference node)
        {
            Errors.Add(
                "Full-text table functions are not allowed.");
        }
    }
}
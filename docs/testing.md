# Testing Report

## Environment

- Frontend: React, TypeScript and Vite
- Backend: ASP.NET Core Web API
- Database: SQL Server Express
- AI model: GPT-5 mini through Azure AI Foundry
- SQL parser: Microsoft ScriptDOM

## Test Results

| Test | Expected result | Status |
|---|---|---|
| Database health | Database connection succeeds | Passed |
| Supported question | Valid SQL is generated | Passed |
| SQL execution | Query results are displayed | Passed |
| Aggregation query | Grouped results are displayed | Passed |
| Multi-table query | Required tables are joined | Passed |
| Unsupported question | No invented columns or SQL | Passed |
| DELETE request | Rejected by validator | Passed |
| DROP TABLE request | Rejected by validator | Passed |
| Direct execution bypass | Rejected by execution endpoint | Passed |
| Read-only database login | SELECT allowed, DELETE denied | Passed |
| Frontend build | Production build succeeds | Passed |
| Backend build | Build succeeds without errors | Passed |

## Security Layers

1. The AI receives strict SQL-generation instructions.
2. Generated SQL is parsed with Microsoft ScriptDOM.
3. Only one SELECT statement is accepted.
4. Unknown tables and dangerous SQL features are rejected.
5. SQL is validated again immediately before execution.
6. Queries execute through a dedicated read-only SQL Server login.
7. Query execution has a timeout.
8. Returned results are limited to 100 rows.
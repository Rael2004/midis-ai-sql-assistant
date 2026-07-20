# AI SQL Assistant

A secure full-stack natural-language-to-SQL application developed as part of a Midis ICT internship project.

The application allows users to ask questions about an IT support-ticket database in plain English. Azure AI Foundry generates Microsoft SQL Server queries, the ASP.NET Core backend validates them, and approved read-only queries are executed and displayed through a React interface.

## Application Preview

### Generated and Validated SQL

![Generated SQL](docs/screenshots/02-generated-sql.png)

### Query Results

![Query results](docs/screenshots/03-query-results.png)

## Main Workflow

```text
Natural-language question
        ↓
React and TypeScript frontend
        ↓
ASP.NET Core Web API
        ↓
Azure AI Foundry — GPT-5 mini
        ↓
Generated T-SQL
        ↓
Microsoft ScriptDOM validation
        ↓
Restricted read-only SQL execution
        ↓
Results displayed in React
```

## Key Features

- Ask database questions in natural language
- Extract the current SQL Server schema automatically
- Generate T-SQL using Azure GPT-5 mini
- Reject questions that cannot be answered from the schema
- Parse generated SQL using Microsoft ScriptDOM
- Allow only one read-only `SELECT` statement
- Reject destructive or unauthorized SQL
- Execute approved queries using a restricted SQL Server login
- Limit query results to 100 rows
- Apply a query execution timeout
- Display generated SQL, validation status and query results
- Show backend and database connection status

## Security Design

AI-generated SQL is treated as untrusted input.

The application uses several protection layers:

1. Strict SQL-generation instructions
2. The current database schema is supplied to the AI model
3. Generated SQL is parsed with Microsoft ScriptDOM
4. Only one SQL batch and one statement are accepted
5. Only `SELECT` statements are allowed
6. Unknown tables and cross-database access are rejected
7. Dangerous operations such as `INSERT`, `UPDATE`, `DELETE`, `DROP`, `ALTER` and `SELECT INTO` are rejected
8. SQL is validated again immediately before execution
9. Queries execute through a dedicated read-only SQL Server login
10. Query execution has a timeout
11. Returned results are limited to 100 rows
12. Credentials remain in backend-only secret storage

## Technology Stack

### Frontend

- React
- TypeScript
- Vite
- CSS

### Backend

- ASP.NET Core Web API
- C#
- Microsoft.Data.SqlClient
- Microsoft ScriptDOM

### Database

- Microsoft SQL Server Express
- SQL Server Management Studio
- T-SQL

### Artificial Intelligence

- Azure AI Foundry
- GPT-5 mini

### Development Tools

- Git and GitHub
- Visual Studio Code
- PowerShell
- .NET User Secrets

## API Endpoints

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `/api/database/health` | Check the database connection |
| `GET` | `/api/schema` | Return the database schema |
| `GET` | `/api/schema/prompt` | Return the prompt-friendly schema |
| `POST` | `/api/ai/generate-sql` | Generate and validate SQL |
| `POST` | `/api/sql/validate` | Validate SQL independently |
| `POST` | `/api/query/execute` | Validate and execute read-only SQL |

## Example Questions

```text
Which client submitted the most tickets?

Show all open critical tickets.

How many tickets are there for each status?

Which employee resolved the most tickets?

Show each open ticket with its client and assigned employee.
```

## Project Structure

```text
midis-ai-sql-assistant/
├── backend/
│   └── MidisSqlAi.Api/
│       ├── Controllers/
│       ├── Models/
│       ├── Prompts/
│       ├── Services/
│       ├── Program.cs
│       └── MidisSqlAi.Api.csproj
├── database/
│   ├── schema.sql
│   └── seed-data.sql
├── docs/
│   ├── architecture.md
│   ├── setup.md
│   ├── testing.md
│   └── screenshots/
├── frontend/
│   ├── public/
│   └── src/
├── .gitignore
├── MidisSqlAi.sln
└── README.md
```

## Local Setup

Detailed configuration instructions are available in the [Local Setup Guide](docs/setup.md).

Run the backend:

```powershell
cd backend\MidisSqlAi.Api
dotnet run --launch-profile https
```

Run the frontend in a second terminal:

```powershell
cd frontend
npm install
npm run dev
```

Default development addresses:

```text
Frontend: http://localhost:5173
Backend:  https://localhost:7238
```

The backend port may differ according to `launchSettings.json`.

## Documentation

- [System Architecture](docs/architecture.md)
- [Local Setup Guide](docs/setup.md)
- [Testing Report](docs/testing.md)

## Additional Screenshots

### Application Home

![Application home](docs/screenshots/01-home-page.png)

### Rejected Dangerous Query

![Rejected query](docs/screenshots/04-rejected-query.png)

### Database Schema

![Database schema](docs/screenshots/05-database-schema.png)

## Current Scope

The completed MVP includes:

- SQL Server support-ticket database
- Azure AI SQL generation
- SQL syntax and security validation
- Restricted read-only query execution
- React frontend
- Full frontend-to-backend integration
- Result visualization
- Functional and security testing
- Technical documentation

## Limitations

- The database currently runs locally using SQL Server Express
- Only read-only database questions are supported
- Queries are limited to the configured support-ticket schema
- Query results are limited to 100 rows
- User authentication is outside the current MVP scope
- Production deployment is outside the current MVP scope

## Future Improvements

- User authentication and authorization
- Query history
- Saved questions
- CSV export
- SQL syntax highlighting
- Additional audit logging
- Automated unit and integration tests
- Azure-hosted API and database
- Support for multiple approved database schemas
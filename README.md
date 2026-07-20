# AI SQL Assistant

A full-stack natural-language-to-SQL application developed as part of a Midis ICT internship project.

The application allows users to ask questions about an IT support-ticket database in plain English. Azure AI generates Microsoft SQL Server queries, the backend validates them, and approved read-only queries are executed and displayed in a React interface.

## Project Objectives

The project demonstrates:

- Generative AI integration
- Prompt engineering
- Natural-language-to-SQL generation
- ASP.NET Core API development
- React and TypeScript frontend development
- SQL Server database design
- Secure AI-output validation
- Read-only database execution
- Full-stack system integration

## Main Workflow

Natural-language question
        ↓
React frontend
        ↓
ASP.NET Core API
        ↓
Azure GPT-5 mini
        ↓
Generated SQL
        ↓
Microsoft ScriptDOM validation
        ↓
Restricted read-only execution
        ↓
Results table

## Project Structure

midis-ai-sql-assistant/
├── backend/
│   └── MidisSqlAi.Api/
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
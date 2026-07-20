# System Architecture

## Overview

The AI SQL Assistant is a full-stack application that allows a user to ask questions about an IT support-ticket database using natural language.

The application uses Azure AI to generate SQL, validates the generated query, executes approved queries through a restricted database account, and displays the results in a React interface.

## Architecture Diagram

```mermaid
flowchart TD
    A[User] --> B[React and TypeScript Frontend]

    B -->|POST natural-language question| C[ASP.NET Core Web API]

    C --> D[Database Schema Service]
    D --> E[SQL Server Express]

    C -->|Schema and question| F[Azure AI Foundry - GPT-5 mini]

    F -->|Generated T-SQL| C

    C --> G[Microsoft ScriptDOM Validator]

    G -->|Rejected SQL| B
    G -->|Approved SELECT query| H[Query Execution Service]

    H -->|Read-only SQL account| E

    E -->|Query results| H
    H --> C
    C -->|JSON response| B
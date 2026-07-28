#!/usr/bin/env bash

set -Eeuo pipefail

SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
SERVER="sqlserver"

echo "Checking whether MidisSqlAiDb already exists..."

DATABASE_EXISTS="$(
    "$SQLCMD" \
        -S "$SERVER" \
        -U sa \
        -P "$MSSQL_SA_PASSWORD" \
        -C \
        -h -1 \
        -W \
        -Q "
            SET NOCOUNT ON;

            SELECT
                CASE
                    WHEN DB_ID(N'MidisSqlAiDb') IS NULL
                        THEN 0
                    ELSE 1
                END;
        "
)"

DATABASE_EXISTS="$(
    printf '%s' "$DATABASE_EXISTS" |
    tr -d '[:space:]'
)"

if [[ "$DATABASE_EXISTS" == "0" ]]
then
    echo "Creating the application database and tables..."

    "$SQLCMD" \
        -S "$SERVER" \
        -U sa \
        -P "$MSSQL_SA_PASSWORD" \
        -C \
        -b \
        -i /scripts/schema.sql

    echo "Inserting application seed data..."

    "$SQLCMD" \
        -S "$SERVER" \
        -U sa \
        -P "$MSSQL_SA_PASSWORD" \
        -C \
        -b \
        -i /scripts/seed-data.sql
else
    echo "MidisSqlAiDb already exists; schema and seed steps were skipped."
fi

echo "Creating or updating the read-only login..."

"$SQLCMD" \
    -S "$SERVER" \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -b \
    -v ReadOnlyPassword="$READONLY_DB_PASSWORD" \
    -i /scripts/create-readonly-user.sql

echo "Verifying the database..."

"$SQLCMD" \
    -S "$SERVER" \
    -U sa \
    -P "$MSSQL_SA_PASSWORD" \
    -C \
    -b \
    -d MidisSqlAiDb \
    -Q "
        SET NOCOUNT ON;

        SELECT
            (SELECT COUNT(*) FROM dbo.Clients)
                AS ClientCount,
            (SELECT COUNT(*) FROM dbo.Tickets)
                AS TicketCount;
    "

echo "Database initialization completed successfully."
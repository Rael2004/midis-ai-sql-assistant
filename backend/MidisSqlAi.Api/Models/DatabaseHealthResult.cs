namespace MidisSqlAi.Api.Models;

public sealed record DatabaseHealthResult(
    bool CanConnect,
    string DatabaseName,
    string ServerName,
    int ClientCount,
    int TicketCount
);
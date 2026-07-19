using MidisSqlAi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controller-based API support.
builder.Services.AddControllers();

// Register application services.
// Each interface is mapped to its concrete implementation.
builder.Services.AddScoped<
    IDatabaseHealthService,
    DatabaseHealthService>();

builder.Services.AddScoped<
    IDatabaseSchemaService,
    DatabaseSchemaService>();

builder.Services.AddScoped<
    ISqlValidationService,
    SqlValidationService>();

builder.Services.AddScoped<
    ISqlGenerationService,
    SqlGenerationService>();

builder.Services.AddScoped<
    IQueryExecutionService,
    QueryExecutionService>();

// Generate an OpenAPI document during development.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
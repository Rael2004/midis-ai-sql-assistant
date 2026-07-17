using MidisSqlAi.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Register controller support.
builder.Services.AddControllers();

// Register our database health service with dependency injection.
// When a controller requests IDatabaseHealthService,
// ASP.NET Core creates a DatabaseHealthService.
builder.Services.AddScoped<
    IDatabaseHealthService,
    DatabaseHealthService>();
builder.Services.AddScoped<
    IDatabaseSchemaService,
    DatabaseSchemaService>();
// Generate the OpenAPI description during development.
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
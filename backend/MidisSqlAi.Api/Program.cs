using MidisSqlAi.Api.Services;

const string FrontendCorsPolicy = "FrontendCors";

var builder = WebApplication.CreateBuilder(args);

// Add controller-based API support.
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

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

if (!app.Environment.IsEnvironment("Container"))
{
    app.UseHttpsRedirection();
}

app.UseCors(FrontendCorsPolicy);

app.MapControllers();

app.Run();
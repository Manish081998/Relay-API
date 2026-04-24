using Microsoft.AspNetCore.Builder;
using Relay.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Host.AddLogging();

// Add framework services
builder.Services.AddFrameworkServices(builder.Configuration);

// Add cross-cutting services
builder.Services.AddCrossCutting();

// Add infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Register modules
builder.Services.AddRelayModules();

var app = builder.Build();

// Configure middleware
app.UseMiddlewarePipeline();

app.MapControllers();
app.MapHealthChecks();

app.Run();

/// <summary>Exposed so integration tests can bootstrap the host via WebApplicationFactory.</summary>
public partial class Program { }

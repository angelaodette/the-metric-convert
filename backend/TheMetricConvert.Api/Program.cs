using TheMetricConvert.Api;
using System.Reflection;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Metric Convert API",
        Version = "v1",
        Description = "Learning-friendly metric conversions (units catalog + step-by-step conversions)."
    });

    // Pull XML doc comments into Swagger descriptions.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

app.UseCors(policy =>
{
    policy
        .AllowAnyHeader()
        .AllowAnyMethod()
        .WithOrigins(
            "http://localhost:4200",
            "http://127.0.0.1:4200");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ui =>
    {
        ui.DocumentTitle = "The Metric Convert — API Docs";
        ui.SwaggerEndpoint("/swagger/v1/swagger.json", "The Metric Convert API v1");
        ui.DisplayRequestDuration();
        ui.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        ui.DefaultModelExpandDepth(2);
        ui.DefaultModelsExpandDepth(-1);
        ui.EnableDeepLinking();

        // Light branding hook (we serve this from wwwroot).
        ui.InjectStylesheet("/swagger-ui/custom.css");
    });
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("Healthz")
    .WithTags("Utility")
    .WithSummary("Health check")
    .WithDescription("Simple liveness endpoint for local development and container probes.")
    .Produces(StatusCodes.Status200OK);

app.MapGet("/api/units", () => Results.Ok(UnitCatalog.All))
    .WithName("GetUnits")
    .WithTags("Units")
    .WithSummary("List supported units")
    .WithDescription("Returns the unit catalog used by the converter, including metric prefixes where applicable.")
    .Produces<IReadOnlyList<UnitDefinition>>(StatusCodes.Status200OK);

app.MapPost("/api/conversions", (ConvertRequest request) =>
    {
        // We return a structured result even on errors so the UI can show a friendly message.
        var result = UnitConverter.Convert(request);
        return result.IsOk
            ? Results.Ok(result)
            : Results.BadRequest(result);
    })
    .WithName("CreateConversion")
    .WithTags("Conversions")
    .WithSummary("Convert between units")
    .WithDescription("Converts a numeric value from one unit to another and returns step-by-step explanation for learning.")
    .Accepts<ConvertRequest>("application/json")
    .Produces<ConvertResult>(StatusCodes.Status200OK)
    .Produces<ConvertResult>(StatusCodes.Status400BadRequest);

// Temporary compatibility endpoint (remove once frontend uses /api/conversions)
app.MapPost("/api/convert", (ConvertRequest request) =>
    {
        // Redirects often drop the request body; keep behavior identical instead.
        var result = UnitConverter.Convert(request);
        return result.IsOk
            ? Results.Ok(result)
            : Results.BadRequest(result);
    })
    .WithName("ConvertUnitsLegacy")
    .WithTags("Conversions")
    .WithSummary("Legacy convert endpoint")
    .WithDescription("Deprecated alias for POST /api/conversions (kept temporarily for compatibility).")
    .Accepts<ConvertRequest>("application/json")
    .Produces<ConvertResult>(StatusCodes.Status200OK)
    .Produces<ConvertResult>(StatusCodes.Status400BadRequest);

app.Run();

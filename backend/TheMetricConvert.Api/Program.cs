using TheMetricConvert.Api;
using System.Reflection;
using Microsoft.OpenApi;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddEndpointsApiExplorer();

// Add PostgreSQL database context
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Host=localhost;Port=5432;Database=metric_convert;Username=postgres;Password=postgres";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString)
);

// Add authentication service
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "The Metric Convert API",
        Version = "v1",
        Description = "Learning-friendly metric conversions with user authentication (units catalog + step-by-step conversions)."
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

// Apply migrations automatically in development
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        try
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.Migrate();
        }
        catch (Exception ex)
        {
            app.Logger.LogError(ex, "Failed to apply database migrations");
        }
    }
}

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

// ====== Utility Endpoints ======

app.MapGet("/healthz", () => Results.Ok(new { status = "ok" }))
    .WithName("Healthz")
    .WithTags("Utility")
    .WithSummary("Health check")
    .WithDescription("Simple liveness endpoint for local development and container probes.")
    .Produces(StatusCodes.Status200OK);

// ====== Auth Endpoints ======

app.MapPost("/api/auth/register", async (IAuthService authService, RegisterRequest request) =>
    {
        try
        {
            var response = await authService.RegisterAsync(request);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse { Message = ex.Message, Code = "INVALID_INPUT" });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new ErrorResponse { Message = ex.Message, Code = "USER_EXISTS" });
        }
        catch (Exception ex)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    })
    .WithName("Register")
    .WithTags("Auth")
    .WithSummary("Register a new user")
    .WithDescription("Creates a new user account with email and password.")
    .Accepts<RegisterRequest>("application/json")
    .Produces<AuthResponse>(StatusCodes.Status200OK)
    .Produces<ErrorResponse>(StatusCodes.Status400BadRequest);

app.MapPost("/api/auth/login", async (IAuthService authService, LoginRequest request) =>
    {
        try
        {
            var response = await authService.LoginAsync(request);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new ErrorResponse { Message = ex.Message, Code = "INVALID_INPUT" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    })
    .WithName("Login")
    .WithTags("Auth")
    .WithSummary("Log in user")
    .WithDescription("Authenticates user with email and password, returns JWT access token and refresh token.")
    .Accepts<LoginRequest>("application/json")
    .Produces<AuthResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

app.MapPost("/api/auth/refresh", async (IAuthService authService, RefreshTokenRequest request) =>
    {
        try
        {
            var response = await authService.RefreshAccessTokenAsync(request.RefreshToken);
            return Results.Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }
    })
    .WithName("RefreshToken")
    .WithTags("Auth")
    .WithSummary("Refresh access token")
    .WithDescription("Uses a refresh token to obtain a new access token.")
    .Accepts<RefreshTokenRequest>("application/json")
    .Produces<AuthResponse>(StatusCodes.Status200OK)
    .Produces(StatusCodes.Status401Unauthorized);

// ====== Units Endpoints ======

app.MapGet("/api/units", () => Results.Ok(UnitCatalog.All))
    .WithName("GetUnits")
    .WithTags("Units")
    .WithSummary("List supported units")
    .WithDescription("Returns the unit catalog used by the converter, including metric prefixes where applicable.")
    .Produces<IReadOnlyList<UnitDefinition>>(StatusCodes.Status200OK);

// ====== Conversions Endpoints ======

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

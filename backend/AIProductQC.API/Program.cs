using System;
using System.Text.Json.Serialization;
using AIProductQC.API.Middleware;
using AIProductQC.Core.Interfaces;
using AIProductQC.Infrastructure.Data;
using AIProductQC.Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Database Context (SQL Server LocalDB)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("AIProductQC.Infrastructure")));

// 2. Register Application Services & HttpClient
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<IInspectionService, InspectionService>();
builder.Services.AddHttpClient<IGeminiVisionService, GeminiVisionService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Configure request multipart body length limit (10MB) to reject oversized uploads at pipeline level
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});

// 3. Add Controllers with JSON Enum & Circular Reference handling
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// 4. Add Swagger / OpenAPI Documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "AI Product Defect Inspection & Quality Control API",
        Version = "v1",
        Description = "Multimodal AI Quality Control and Product Defect Inspection Assistant API"
    });
});

// 5. Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "http://localhost:5173", "http://127.0.0.1:5173", "http://localhost:3000", "http://127.0.0.1:3000" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 6. Global Exception Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// 7. Configure HTTP Request Pipeline
// Enable Swagger UI (accessible at /swagger)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AI Product QC API v1");
    c.RoutePrefix = "swagger";
});

// Serve default file (index.html) and static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

// Single Page Application routing fallback: any non-file, non-API route serves index.html
app.MapFallbackToFile("index.html");

// 8. Auto-apply database migrations on startup (for seamless deployment on MonsterASP)
try
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (dbContext.Database.IsRelational())
    {
        dbContext.Database.Migrate();
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Automatic database migration skipped or failed on startup. Please verify SQL connection string.");
}

app.Run();

using System.Diagnostics;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Asp.Versioning.Builder;
using dotnet.Database;
using dotnet.Exceptions;
using dotnet.Extensions;
using dotnet.OpenApi;
using FluentValidation;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

string connectionString = builder.Configuration.GetConnectionString("database") ?? string.Empty;

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails(o =>
{
    o.CustomizeProblemDetails = context =>
    {
        context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
        context.ProblemDetails.Extensions.Add("requestId", context.HttpContext.TraceIdentifier);
        Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
        context.ProblemDetails.Extensions.Add("traceId", activity?.Id);
    };
});

#region API Versioning
builder.Services.AddApiVersioning(option =>
{
    option.DefaultApiVersion = new ApiVersion(1);
    option.ApiVersionReader = new UrlSegmentApiVersionReader();

}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});
#endregion

#region Swagger Configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureOptions<ConfigureSwaggerGenOptions>();
#endregion

//Registering Endpoints
builder.Services.AddEndpoints(Assembly.GetExecutingAssembly());

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var serverVersion = new MySqlServerVersion(new Version(8, 0, 36)); // Replace with your actual MySQL version
    options.UseMySql(connectionString, serverVersion);
});

builder.Services.RegisterServices();

builder.Services.AddCors(options =>
{
    options.AddPolicy("defaultCors", policy =>
    {
        var origins = builder.Configuration.GetSection("CorsPolicy:origins").Get<string[]>() ?? [];
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("defaultCors");

ApiVersionSet apiVersionSet = app.NewApiVersionSet()
.HasApiVersion(new ApiVersion(1))
.HasApiVersion(new ApiVersion(2))
.ReportApiVersions()
.Build();

// Define a route group using versioning in the URL path: "api/v{version}"
RouteGroupBuilder versionGroup = app.MapGroup("api/v{version:apiVersion}")
.WithApiVersionSet(apiVersionSet);

// Map your application endpoints to the version group above
// This likely calls extension methods like app.MapEndpoints() you defined to register actual API routes
app.MapEndpoints(versionGroup);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        IReadOnlyList<ApiVersionDescription> descriptions = app.DescribeApiVersions();
        foreach (var apiVersionDescription in descriptions)
        {
            var url = $"/swagger/{apiVersionDescription.GroupName}/swagger.json";
            var name = apiVersionDescription.GroupName.ToUpperInvariant();

            options.SwaggerEndpoint(url, name);
        }
    });
}
app.UseExceptionHandler();

app.UseHttpsRedirection();

app.Run();

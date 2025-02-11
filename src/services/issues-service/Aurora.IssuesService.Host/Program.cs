using System;
using System.IO;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aurora.IssuesService.Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        // Use DEV database during development.
        string issuesDatabasePath;
        if (builder.Environment.IsDevelopment())
        {
            issuesDatabasePath = Path.Combine("bin", "dev-issues-db.json");
        }
        else
        {
            const string auroraIssuesDbPathEnvVar = "AURORA_ISSUES_DB_PATH";
            issuesDatabasePath = Environment.GetEnvironmentVariable(auroraIssuesDbPathEnvVar) ??
                                 throw new InvalidOperationException($"Cannot read env var: {auroraIssuesDbPathEnvVar}");
        }

        builder.Services.AddSingleton<IIssuesStorage>(serviceProvider =>
        {
            var logger = serviceProvider.GetService<ILogger<IssuesStorage>>() ??
                         throw new InvalidOperationException($"Cannot get service: {typeof(ILogger<IssuesStorage>)}.");
            return new IssuesStorage(issuesDatabasePath, logger);
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.

        // TODO CORS settings should be properly set in production.
        // Set CORS to allow any origin.
        app.UseCors(corsPolicyBuilder =>
        {
            corsPolicyBuilder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });

        app.MapControllers();

        // Perform database upgrade
        app.Services.GetService<IIssuesStorage>();

        app.Run();
    }
}
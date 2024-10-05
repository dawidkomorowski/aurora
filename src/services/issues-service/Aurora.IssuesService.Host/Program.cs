using System.IO;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Aurora.IssuesService.Host
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddSingleton<IIssuesStorage>(new IssuesStorage(Path.Combine("bin", "dev-issues-db.json")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            if (app.Environment.IsDevelopment())
            {
                // Set CORS to allow any origin.
                app.UseCors(corsPolicyBuilder =>
                {
                    corsPolicyBuilder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            }

            app.MapControllers();

            app.Run();
        }
    }
}
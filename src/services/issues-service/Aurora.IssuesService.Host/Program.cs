using System.IO;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

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
            app.MapControllers();

            app.Run();
        }
    }
}
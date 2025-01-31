using Microsoft.AspNetCore.Builder;

namespace Aurora.Web.Host;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseStaticFiles();
        app.UseSpa(_ => { });

        app.Run();
    }
}
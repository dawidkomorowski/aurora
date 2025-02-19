using System;
using Aurora.IssuesService.Host;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Aurora.IssuesService.IntegrationTests;

internal sealed class IntegrationTestsWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _issuesDatabasePath;

    public IntegrationTestsWebApplicationFactory(string issuesDatabasePath)
    {
        _issuesDatabasePath = issuesDatabasePath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Production);

        Environment.SetEnvironmentVariable("AURORA_ISSUES_DB_PATH", _issuesDatabasePath);
    }
}
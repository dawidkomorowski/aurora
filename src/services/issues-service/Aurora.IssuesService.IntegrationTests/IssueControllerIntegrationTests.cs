using System;
using Aurora.IssuesService.Host;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.IO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Aurora.IssuesService.Host.Controllers;
using System.Net.Http;
using System.Text;

namespace Aurora.IssuesService.IntegrationTests;

public class IssueControllerIntegrationTests
{
    private string _temporaryDirectoryPath = null!;
    private IntegrationTestsWebApplicationFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var randomDirectoryName = Path.GetRandomFileName();
        _temporaryDirectoryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TempDir", randomDirectoryName);
        Directory.CreateDirectory(_temporaryDirectoryPath);

        var temporaryStorageFilePath = Path.Combine(_temporaryDirectoryPath, "dev-issues-db.json");

        _factory = new IntegrationTestsWebApplicationFactory(temporaryStorageFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_temporaryDirectoryPath, true);

        _factory.Dispose();
    }

    [Test]
    public async Task Test1()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        using var content = TestKit.CreateJsonContent(new CreateIssueRequest { Title = "Test" });
        var x = await client.PostAsync("api/issues", content);
        Console.WriteLine(x);
        Console.WriteLine(await x.Content.ReadAsStringAsync());
        x.EnsureSuccessStatusCode();

        var response = await client.GetAsync("api/issues");

        // Assert
        response.EnsureSuccessStatusCode();
        TestKit.AssertThatContentIsJson(response.Content);

        var issues = await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>();
        Assert.That(issues, Is.Not.Null);
        Assert.That(issues.Length, Is.EqualTo(0));
    }
}

internal static class TestKit
{
    public static StringContent CreateJsonContent(object obj)
    {
        return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }

    public static void AssertThatContentIsJson(HttpContent content)
    {
        Assert.That(content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        Assert.That(content.Headers.ContentType?.CharSet, Is.EqualTo("utf-8"));
    }
}

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
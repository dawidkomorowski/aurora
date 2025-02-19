using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using NUnit.Framework;

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
    public async Task GetAllIssues_ShouldReturnNoIssues_WhenDatabaseIsEmpty()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("api/issues");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var issues = await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>();
        Assert.That(issues, Is.Not.Null);
        Assert.That(issues, Is.Empty);
    }
}
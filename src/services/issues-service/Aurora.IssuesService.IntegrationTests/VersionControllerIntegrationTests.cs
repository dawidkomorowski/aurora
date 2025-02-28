using NUnit.Framework;
using System.IO;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;

namespace Aurora.IssuesService.IntegrationTests;

public sealed class VersionControllerIntegrationTests
{
    private string _temporaryDirectoryPath = null!;
    private TestIssueServiceFactory _factory = null!;

    [SetUp]
    public void SetUp()
    {
        var randomDirectoryName = Path.GetRandomFileName();
        _temporaryDirectoryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TempDir", randomDirectoryName);
        Directory.CreateDirectory(_temporaryDirectoryPath);

        var databasePath = Path.Combine(_temporaryDirectoryPath, "issue-service-db.json");

        _factory = new TestIssueServiceFactory(databasePath);
    }

    [TearDown]
    public void TearDown()
    {
        _factory.Dispose();

        Directory.Delete(_temporaryDirectoryPath, true);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task CreateVersion_ShouldReturn_BadRequest_GivenInvalidVersionName(string? versionName)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionRequest = new CreateVersionRequest
        {
            Name = versionName!
        };

        // Act
        using var content = TestKit.CreateJsonContent(createVersionRequest);
        using var response = await client.PostAsync("api/versions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsProblemJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["Name"][0], Is.EqualTo("The Name field is required."));
    }

    [TestCase("Test Version")]
    [TestCase("   Test Version")]
    [TestCase("Test Version   ")]
    [TestCase("   Test Version   ")]
    public async Task CreateVersion_ShouldReturn_BadRequest_GivenVersionNameThatAlreadyExists(string versionName)
    {
        // Arrange
        using var client = _factory.CreateClient();

        await TestKit.CreateVersion(client, "Test Version");

        var createVersionRequest = new CreateVersionRequest
        {
            Name = versionName
        };

        // Act
        using var content = TestKit.CreateJsonContent(createVersionRequest);
        using var response = await client.PostAsync("api/versions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["Name"][0], Is.EqualTo("Version with the same name already exists."));
    }
}
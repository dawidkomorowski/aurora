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

    [Test]
    public async Task CreateVersion_ShouldReturn_Created_AndCreateNewVersion()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionRequest = new CreateVersionRequest
        {
            Name = "Test Version"
        };

        // Assume
        var versionsBefore = await TestKit.GetAllVersions(client);
        Assert.That(versionsBefore, Is.Empty);

        // Act
        using var content = TestKit.CreateJsonContent(createVersionRequest);
        using var response = await client.PostAsync("api/versions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createVersionResponse = await response.Content.ReadFromJsonAsync<CreateVersionResponse>();
        Assert.That(createVersionResponse, Is.Not.Null);
        Assert.That(createVersionResponse.Id, Is.EqualTo(1));

        var versionsAfter = await TestKit.GetAllVersions(client);
        Assert.That(versionsAfter, Has.Length.EqualTo(1));

        var version = await TestKit.GetVersion(client, createVersionResponse.Id);
        Assert.That(version.Id, Is.EqualTo(1));
        Assert.That(version.Name, Is.EqualTo(createVersionRequest.Name));
    }

    [Test]
    public async Task CreateVersion_ShouldCreateMultipleVersions_GivenMultipleCreateRequests()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionRequest1 = new CreateVersionRequest
        {
            Name = "Test Version 1"
        };

        var createVersionRequest2 = new CreateVersionRequest
        {
            Name = "Test Version 2"
        };

        // Assume
        var versionsBefore = await TestKit.GetAllVersions(client);
        Assert.That(versionsBefore, Is.Empty);

        // Act
        using var content1 = TestKit.CreateJsonContent(createVersionRequest1);
        using var response1 = await client.PostAsync("api/versions", content1);

        using var content2 = TestKit.CreateJsonContent(createVersionRequest2);
        using var response2 = await client.PostAsync("api/versions", content2);

        // Assert
        Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response1.Content);
        TestKit.AssertThatContentIsJson(response2.Content);

        var createVersionResponse1 = await response1.Content.ReadFromJsonAsync<CreateVersionResponse>();
        var createVersionResponse2 = await response2.Content.ReadFromJsonAsync<CreateVersionResponse>();
        Assert.That(createVersionResponse1, Is.Not.Null);
        Assert.That(createVersionResponse2, Is.Not.Null);
        Assert.That(createVersionResponse1.Id, Is.EqualTo(1));
        Assert.That(createVersionResponse2.Id, Is.EqualTo(2));

        var versionsAfter = await TestKit.GetAllVersions(client);
        Assert.That(versionsAfter, Has.Length.EqualTo(2));

        var version1 = await TestKit.GetVersion(client, createVersionResponse1.Id);
        var version2 = await TestKit.GetVersion(client, createVersionResponse2.Id);

        Assert.That(version1.Id, Is.EqualTo(1));
        Assert.That(version1.Name, Is.EqualTo(createVersionRequest1.Name));

        Assert.That(version2.Id, Is.EqualTo(2));
        Assert.That(version2.Name, Is.EqualTo(createVersionRequest2.Name));
    }

    [TestCase("Test Version", "Test Version")]
    [TestCase("   Test Version", "Test Version")]
    [TestCase("Test Version   ", "Test Version")]
    [TestCase("   Test Version   ", "Test Version")]
    public async Task CreateVersion_ShouldReturn_Created_AndCreateNewVersion_WithTrimmedVersionName(string name, string expectedName)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionRequest = new CreateVersionRequest
        {
            Name = name
        };

        // Assume
        var versionsBefore = await TestKit.GetAllVersions(client);
        Assert.That(versionsBefore, Is.Empty);

        // Act
        using var content = TestKit.CreateJsonContent(createVersionRequest);
        using var response = await client.PostAsync("api/versions", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createVersionResponse = await response.Content.ReadFromJsonAsync<CreateVersionResponse>();
        Assert.That(createVersionResponse, Is.Not.Null);
        Assert.That(createVersionResponse.Id, Is.EqualTo(1));

        var versionsAfter = await TestKit.GetAllVersions(client);
        Assert.That(versionsAfter, Has.Length.EqualTo(1));

        var version = await TestKit.GetVersion(client, createVersionResponse.Id);
        Assert.That(version.Name, Is.EqualTo(expectedName));
    }

    [Test]
    public async Task GetAllVersions_ShouldReturn_OK_AndNoVersions_WhenDatabaseIsEmpty()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/versions");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var versions = await response.Content.ReadFromJsonAsync<VersionDetailsResponse[]>();
        Assert.That(versions, Is.Not.Null);
        Assert.That(versions, Is.Empty);
    }

    [Test]
    public async Task GetAllVersions_ShouldReturn_OK_AndAllVersions()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse1 = await TestKit.CreateVersion(client, "Test Version 1");
        var createVersionResponse2 = await TestKit.CreateVersion(client, "Test Version 2");

        // Act
        using var response = await client.GetAsync("api/versions");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var versions = await response.Content.ReadFromJsonAsync<VersionDetailsResponse[]>();
        Assert.That(versions, Is.Not.Null);
        Assert.That(versions, Has.Length.EqualTo(2));

        var version1 = versions[0];
        Assert.That(version1.Id, Is.EqualTo(createVersionResponse1.Id));
        Assert.That(version1.Name, Is.EqualTo("Test Version 1"));

        var version2 = versions[1];
        Assert.That(version2.Id, Is.EqualTo(createVersionResponse2.Id));
        Assert.That(version2.Name, Is.EqualTo("Test Version 2"));
    }

    [Test]
    public async Task GetVersion_ShouldReturn_NotFound_GivenVersionIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/versions/123");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetVersion_ShouldReturn_OK_AndVersionDetails()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse = await TestKit.CreateVersion(client, "Test Version");

        // Act
        using var response = await client.GetAsync($"api/versions/{createVersionResponse.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var version = await response.Content.ReadFromJsonAsync<VersionDetailsResponse>();
        Assert.That(version, Is.Not.Null);
        Assert.That(version.Id, Is.EqualTo(createVersionResponse.Id));
        Assert.That(version.Name, Is.EqualTo("Test Version"));
    }
}
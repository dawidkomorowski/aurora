using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using NUnit.Framework.Internal;

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

        var temporaryStorageFilePath = Path.Combine(_temporaryDirectoryPath, "issue-service-db.json");

        _factory = new IntegrationTestsWebApplicationFactory(temporaryStorageFilePath);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_temporaryDirectoryPath, true);

        _factory.Dispose();
    }

    [Test]
    public async Task CreateIssue_ShouldReturn_BadRequest_GivenVersionIdThatDoesNotExist()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createIssueRequest = new CreateIssueRequest
        {
            Title = "Test issue",
            Description = "Test issue description",
            VersionId = 123
        };

        // Act
        using var content = TestKit.CreateJsonContent(createIssueRequest);
        using var response = await client.PostAsync("api/issues", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["VersionId"][0], Is.EqualTo("Specified version is invalid."));
    }

    [Test]
    public async Task CreateIssue_ShouldReturn_Created_AndCreateNewIssue()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var client = _factory.CreateClient();

        var createIssueRequest = new CreateIssueRequest
        {
            Title = "Test issue",
            Description = "Test issue description"
        };

        // Assume
        var issuesBefore = await TestKit.GetAllIssues(client);
        Assert.That(issuesBefore, Is.Empty);

        // Act
        using var content = TestKit.CreateJsonContent(createIssueRequest);
        using var response = await client.PostAsync("api/issues", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createIssueResponse = await response.Content.ReadFromJsonAsync<CreateIssueResponse>();
        Assert.That(createIssueResponse, Is.Not.Null);
        Assert.That(createIssueResponse.Id, Is.EqualTo(1));

        var issuesAfter = await TestKit.GetAllIssues(client);
        Assert.That(issuesAfter, Has.Length.EqualTo(1));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issue.Id, Is.EqualTo(1));
        Assert.That(issue.Title, Is.EqualTo(createIssueRequest.Title));
        Assert.That(issue.Description, Is.EqualTo(createIssueRequest.Description));
        Assert.That(issue.Status, Is.EqualTo("Open"));
        Assert.That(issue.Version, Is.Null);
        Assert.That(issue.CreatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue.UpdatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue.CreatedDateTime, Is.EqualTo(issue.UpdatedDateTime));
    }

    [Test]
    public async Task CreateIssue_ShouldReturn_Created_AndCreateNewIssue_WithVersion()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var client = _factory.CreateClient();

        const string versionName = "Test version";
        var createVersionResponse = await TestKit.CreateVersion(client, versionName);

        var createIssueRequest = new CreateIssueRequest
        {
            Title = "Test issue",
            Description = "Test issue description",
            VersionId = createVersionResponse.Id
        };

        // Assume
        var issuesBefore = await TestKit.GetAllIssues(client);
        Assert.That(issuesBefore, Is.Empty);

        // Act
        using var content = TestKit.CreateJsonContent(createIssueRequest);
        using var response = await client.PostAsync("api/issues", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createIssueResponse = await response.Content.ReadFromJsonAsync<CreateIssueResponse>();
        Assert.That(createIssueResponse, Is.Not.Null);
        Assert.That(createIssueResponse.Id, Is.EqualTo(1));

        var issuesAfter = await TestKit.GetAllIssues(client);
        Assert.That(issuesAfter, Has.Length.EqualTo(1));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issue.Id, Is.EqualTo(1));
        Assert.That(issue.Title, Is.EqualTo(createIssueRequest.Title));
        Assert.That(issue.Description, Is.EqualTo(createIssueRequest.Description));
        Assert.That(issue.Status, Is.EqualTo("Open"));
        Assert.That(issue.Version, Is.Not.Null);
        Assert.That(issue.Version.Id, Is.EqualTo(createVersionResponse.Id));
        Assert.That(issue.Version.Name, Is.EqualTo(versionName));
        Assert.That(issue.CreatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue.UpdatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue.CreatedDateTime, Is.EqualTo(issue.UpdatedDateTime));
    }

    [Test]
    public async Task CreateIssue_ShouldCreateMultipleIssues_GivenMultipleCreateRequests()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var client = _factory.CreateClient();

        var createIssueRequest1 = new CreateIssueRequest
        {
            Title = "Test issue 1",
            Description = "Test issue description 1"
        };

        var createIssueRequest2 = new CreateIssueRequest
        {
            Title = "Test issue 2",
            Description = "Test issue description 2"
        };

        // Assume
        var issuesBefore = await TestKit.GetAllIssues(client);
        Assert.That(issuesBefore, Is.Empty);

        // Act
        using var content1 = TestKit.CreateJsonContent(createIssueRequest1);
        using var response1 = await client.PostAsync("api/issues", content1);

        using var content2 = TestKit.CreateJsonContent(createIssueRequest2);
        using var response2 = await client.PostAsync("api/issues", content2);

        // Assert
        Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response1.Content);
        TestKit.AssertThatContentIsJson(response2.Content);

        var createIssueResponse1 = await response1.Content.ReadFromJsonAsync<CreateIssueResponse>();
        var createIssueResponse2 = await response2.Content.ReadFromJsonAsync<CreateIssueResponse>();
        Assert.That(createIssueResponse1, Is.Not.Null);
        Assert.That(createIssueResponse2, Is.Not.Null);
        Assert.That(createIssueResponse1.Id, Is.EqualTo(1));
        Assert.That(createIssueResponse2.Id, Is.EqualTo(2));

        var issuesAfter = await TestKit.GetAllIssues(client);
        Assert.That(issuesAfter, Has.Length.EqualTo(2));

        var issue1 = await TestKit.GetIssue(client, createIssueResponse1.Id);
        var issue2 = await TestKit.GetIssue(client, createIssueResponse2.Id);
        Assert.That(issue1.Id, Is.EqualTo(1));
        Assert.That(issue1.Title, Is.EqualTo(createIssueRequest1.Title));
        Assert.That(issue1.Description, Is.EqualTo(createIssueRequest1.Description));
        Assert.That(issue1.Status, Is.EqualTo("Open"));
        Assert.That(issue1.Version, Is.Null);
        Assert.That(issue1.CreatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue1.UpdatedDateTime, Is.GreaterThanOrEqualTo(startTime));
        Assert.That(issue1.CreatedDateTime, Is.EqualTo(issue1.UpdatedDateTime));

        Assert.That(issue2.Id, Is.EqualTo(2));
        Assert.That(issue2.Title, Is.EqualTo(createIssueRequest2.Title));
        Assert.That(issue2.Description, Is.EqualTo(createIssueRequest2.Description));
        Assert.That(issue2.Status, Is.EqualTo("Open"));
        Assert.That(issue2.Version, Is.Null);
        Assert.That(issue2.CreatedDateTime, Is.GreaterThanOrEqualTo(issue1.CreatedDateTime));
        Assert.That(issue2.UpdatedDateTime, Is.GreaterThanOrEqualTo(issue1.UpdatedDateTime));
        Assert.That(issue2.CreatedDateTime, Is.EqualTo(issue2.UpdatedDateTime));
    }

    [Test]
    public async Task GetAllIssues_ShouldReturn_OK_AndNoIssues_WhenDatabaseIsEmpty()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/issues");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var issues = await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>();
        Assert.That(issues, Is.Not.Null);
        Assert.That(issues, Is.Empty);
    }

    // TODO Add tests for filtering issues by status and version once update tests are implemented.
    // TODO This test should include different statuses and versions for baseline - no filtering.
    [Test]
    public async Task GetAllIssues_ShouldReturn_OK_AndAllIssues()
    {
        // Arrange
        var client = _factory.CreateClient();

        var createVersionResponse = await TestKit.CreateVersion(client, "Test version 1");

        await TestKit.CreateIssue(client, "Test issue 1", "Test issue description 1", null);
        await TestKit.CreateIssue(client, "Test issue 2", "Test issue description 2", createVersionResponse.Id);
        await TestKit.CreateIssue(client, "Test issue 3", "Test issue description 3", null);

        // Act
        using var response = await client.GetAsync("api/issues");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var issues = await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>();
        Assert.That(issues, Is.Not.Null);
        Assert.That(issues, Has.Length.EqualTo(3));

        var issue1 = issues[0];
        Assert.That(issue1.Id, Is.EqualTo(1));
        Assert.That(issue1.Title, Is.EqualTo("Test issue 1"));
        Assert.That(issue1.Status, Is.EqualTo("Open"));
        Assert.That(issue1.Version, Is.Null);

        var issue2 = issues[1];
        Assert.That(issue2.Id, Is.EqualTo(2));
        Assert.That(issue2.Title, Is.EqualTo("Test issue 2"));
        Assert.That(issue2.Status, Is.EqualTo("Open"));
        Assert.That(issue2.Version, Is.Not.Null);
        Assert.That(issue2.Version.Id, Is.EqualTo(createVersionResponse.Id));
        Assert.That(issue2.Version.Name, Is.EqualTo("Test version 1"));

        var issue3 = issues[2];
        Assert.That(issue3.Id, Is.EqualTo(3));
        Assert.That(issue3.Title, Is.EqualTo("Test issue 3"));
        Assert.That(issue3.Status, Is.EqualTo("Open"));
        Assert.That(issue3.Version, Is.Null);
    }
}
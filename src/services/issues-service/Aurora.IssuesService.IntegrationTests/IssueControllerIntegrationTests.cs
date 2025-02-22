using System;
using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Aurora.IssuesService.IntegrationTests;

public class IssueControllerIntegrationTests
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
        Directory.Delete(_temporaryDirectoryPath, true);

        _factory.Dispose();
    }

    [Test]
    public async Task CreateIssue_ShouldReturn_BadRequest_GivenVersionIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

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
        using var client = _factory.CreateClient();

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
        using var client = _factory.CreateClient();

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
        using var client = _factory.CreateClient();

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
        using var client = _factory.CreateClient();

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
    [Test]
    public async Task GetAllIssues_ShouldReturn_OK_AndAllIssues()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse1 = await TestKit.CreateVersion(client, "Test version 1");
        var createVersionResponse2 = await TestKit.CreateVersion(client, "Test version 2");

        await TestKit.CreateIssue(client, "Test issue 1", "Test issue description 1", null);
        await TestKit.CreateIssue(client, "Test issue 2", "Test issue description 2", createVersionResponse1.Id);
        await TestKit.CreateIssue(client, "Test issue 3", "Test issue description 3", null);
        await TestKit.CreateIssue(client, "Test issue 4", "Test issue description 4", createVersionResponse2.Id);

        await TestKit.UpdateIssueStatus(client, 3, "In Progress");
        await TestKit.UpdateIssueStatus(client, 4, "Closed");

        // Act
        using var response = await client.GetAsync("api/issues");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var issues = await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>();
        Assert.That(issues, Is.Not.Null);
        Assert.That(issues, Has.Length.EqualTo(4));

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
        Assert.That(issue2.Version.Id, Is.EqualTo(createVersionResponse1.Id));
        Assert.That(issue2.Version.Name, Is.EqualTo("Test version 1"));

        var issue3 = issues[2];
        Assert.That(issue3.Id, Is.EqualTo(3));
        Assert.That(issue3.Title, Is.EqualTo("Test issue 3"));
        Assert.That(issue3.Status, Is.EqualTo("In Progress"));
        Assert.That(issue3.Version, Is.Null);

        var issue4 = issues[3];
        Assert.That(issue4.Id, Is.EqualTo(4));
        Assert.That(issue4.Title, Is.EqualTo("Test issue 4"));
        Assert.That(issue4.Status, Is.EqualTo("Closed"));
        Assert.That(issue4.Version, Is.Not.Null);
        Assert.That(issue4.Version.Id, Is.EqualTo(createVersionResponse2.Id));
        Assert.That(issue4.Version.Name, Is.EqualTo("Test version 2"));
    }

    [Test]
    public async Task GetIssue_ShouldReturn_NotFound_GivenIssueIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/issues/123");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetIssue_ShouldReturn_OK_AndIssueDetails()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse = await TestKit.CreateVersion(client, "Test version");
        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", createVersionResponse.Id);

        // Act
        using var response = await client.GetAsync($"api/issues/{createIssueResponse.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var issue = await response.Content.ReadFromJsonAsync<IssueDetailsResponse>();
        Assert.That(issue, Is.Not.Null);
        Assert.That(issue.Id, Is.EqualTo(createIssueResponse.Id));
        Assert.That(issue.Title, Is.EqualTo("Test issue"));
        Assert.That(issue.Description, Is.EqualTo("Test issue description"));
        Assert.That(issue.Status, Is.EqualTo("Open"));
        Assert.That(issue.Version, Is.Not.Null);
        Assert.That(issue.Version.Id, Is.EqualTo(createVersionResponse.Id));
        Assert.That(issue.Version.Name, Is.EqualTo("Test version"));
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_NotFound_GivenIssueIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Status = "Open"
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync("api/issues/123", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_BadRequest_GivenVersionIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", null);

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Status = "Open",
            VersionId = 123
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync($"api/issues/{createIssueResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["VersionId"][0], Is.EqualTo("Specified version is invalid."));
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_OK_AndUpdateIssue()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", null);
        var createdIssue = await TestKit.GetIssue(client, createIssueResponse.Id);

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Description = "Updated test issue description",
            Status = "Closed"
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync($"api/issues/{createIssueResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updateResponse = await response.Content.ReadFromJsonAsync<IssueDetailsResponse>();
        Assert.That(updateResponse, Is.Not.Null);
        Assert.That(updateResponse.Id, Is.EqualTo(createIssueResponse.Id));
        Assert.That(updateResponse.Title, Is.EqualTo(updateIssueRequest.Title));
        Assert.That(updateResponse.Description, Is.EqualTo(updateIssueRequest.Description));
        Assert.That(updateResponse.Status, Is.EqualTo(updateIssueRequest.Status));
        Assert.That(updateResponse.Version, Is.Null);
        Assert.That(updateResponse.CreatedDateTime, Is.EqualTo(createdIssue.CreatedDateTime));
        Assert.That(updateResponse.UpdatedDateTime, Is.GreaterThan(createdIssue.UpdatedDateTime));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        TestKit.AssertThatIssueDetailResponsesAreEqual(issue, updateResponse);
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_OK_AndSetVersionOnIssue_GivenIssueWithNoVersion()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse = await TestKit.CreateVersion(client, "Test version");
        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", null);
        var createdIssue = await TestKit.GetIssue(client, createIssueResponse.Id);

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Description = "Updated test issue description",
            Status = "Closed",
            VersionId = createVersionResponse.Id
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync($"api/issues/{createIssueResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updateResponse = await response.Content.ReadFromJsonAsync<IssueDetailsResponse>();
        Assert.That(updateResponse, Is.Not.Null);
        Assert.That(updateResponse.Id, Is.EqualTo(createIssueResponse.Id));
        Assert.That(updateResponse.Title, Is.EqualTo(updateIssueRequest.Title));
        Assert.That(updateResponse.Description, Is.EqualTo(updateIssueRequest.Description));
        Assert.That(updateResponse.Status, Is.EqualTo(updateIssueRequest.Status));
        Assert.That(updateResponse.Version, Is.Not.Null);
        Assert.That(updateResponse.Version.Id, Is.EqualTo(createVersionResponse.Id));
        Assert.That(updateResponse.Version.Name, Is.EqualTo("Test version"));
        Assert.That(updateResponse.CreatedDateTime, Is.EqualTo(createdIssue.CreatedDateTime));
        Assert.That(updateResponse.UpdatedDateTime, Is.GreaterThan(createdIssue.UpdatedDateTime));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        TestKit.AssertThatIssueDetailResponsesAreEqual(issue, updateResponse);
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_OK_AndChangeVersionOnIssue_GivenIssueWithVersion()
    {
        // Arrange

        using var client = _factory.CreateClient();
        var createVersionResponse1 = await TestKit.CreateVersion(client, "Test version 1");
        var createVersionResponse2 = await TestKit.CreateVersion(client, "Test version 2");
        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", createVersionResponse1.Id);
        var createdIssue = await TestKit.GetIssue(client, createIssueResponse.Id);

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Description = "Updated test issue description",
            Status = "Closed",
            VersionId = createVersionResponse2.Id
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync($"api/issues/{createIssueResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updateResponse = await response.Content.ReadFromJsonAsync<IssueDetailsResponse>();
        Assert.That(updateResponse, Is.Not.Null);
        Assert.That(updateResponse.Id, Is.EqualTo(createIssueResponse.Id));
        Assert.That(updateResponse.Title, Is.EqualTo(updateIssueRequest.Title));
        Assert.That(updateResponse.Description, Is.EqualTo(updateIssueRequest.Description));
        Assert.That(updateResponse.Status, Is.EqualTo(updateIssueRequest.Status));
        Assert.That(updateResponse.Version, Is.Not.Null);
        Assert.That(updateResponse.Version.Id, Is.EqualTo(createVersionResponse2.Id));
        Assert.That(updateResponse.Version.Name, Is.EqualTo("Test version 2"));
        Assert.That(updateResponse.CreatedDateTime, Is.EqualTo(createdIssue.CreatedDateTime));
        Assert.That(updateResponse.UpdatedDateTime, Is.GreaterThan(createdIssue.UpdatedDateTime));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        TestKit.AssertThatIssueDetailResponsesAreEqual(issue, updateResponse);
    }

    [Test]
    public async Task UpdateIssue_ShouldReturn_OK_AndRemoveVersionFromIssue_GivenIssueWithVersion()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createVersionResponse = await TestKit.CreateVersion(client, "Test version");
        var createIssueResponse = await TestKit.CreateIssue(client, "Test issue", "Test issue description", createVersionResponse.Id);
        var createdIssue = await TestKit.GetIssue(client, createIssueResponse.Id);

        var updateIssueRequest = new UpdateIssueRequest
        {
            Title = "Updated test issue",
            Description = "Updated test issue description",
            Status = "Closed",
            VersionId = null
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateIssueRequest);
        using var response = await client.PutAsync($"api/issues/{createIssueResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updateResponse = await response.Content.ReadFromJsonAsync<IssueDetailsResponse>();
        Assert.That(updateResponse, Is.Not.Null);
        Assert.That(updateResponse.Id, Is.EqualTo(createIssueResponse.Id));
        Assert.That(updateResponse.Title, Is.EqualTo(updateIssueRequest.Title));
        Assert.That(updateResponse.Description, Is.EqualTo(updateIssueRequest.Description));
        Assert.That(updateResponse.Status, Is.EqualTo(updateIssueRequest.Status));
        Assert.That(updateResponse.Version, Is.Null);
        Assert.That(updateResponse.CreatedDateTime, Is.EqualTo(createdIssue.CreatedDateTime));
        Assert.That(updateResponse.UpdatedDateTime, Is.GreaterThan(createdIssue.UpdatedDateTime));

        var issue = await TestKit.GetIssue(client, createIssueResponse.Id);
        TestKit.AssertThatIssueDetailResponsesAreEqual(issue, updateResponse);
    }
}
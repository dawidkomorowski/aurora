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
    }

    [Test]
    public async Task GetAllIssues_ShouldReturn_OK_NoIssues_WhenDatabaseIsEmpty()
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
}
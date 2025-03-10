using System.IO;
using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;

namespace Aurora.IssuesService.IntegrationTests;

public sealed class ChecklistControllerIntegrationTests
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
    public async Task CreateChecklist_ShouldReturn_BadRequest_GivenInvalidChecklistTitle(string? title)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = title!
        };

        // Act
        using var content = TestKit.CreateJsonContent(createChecklistRequest);
        using var response = await client.PostAsync("api/issues/1/checklists", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsProblemJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["Title"][0], Is.EqualTo("The Title field is required."));
    }

    [Test]
    public async Task CreateChecklist_ShouldReturn_NotFound_GivenIssueIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = "Checklist 1"
        };

        // Act
        using var content = TestKit.CreateJsonContent(createChecklistRequest);
        using var response = await client.PostAsync("api/issues/1/checklists", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreateChecklist_ShouldReturn_Created_AndCreateNewChecklist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = "Checklist 1"
        };

        // Assume
        var checklistsBefore = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        using var createChecklistContent = TestKit.CreateJsonContent(createChecklistRequest);
        using var createChecklistResponse = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", createChecklistContent);

        // Assert
        Assert.That(createChecklistResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(1));
        Assert.That(checklists[0].Id, Is.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklists[0].Items, Is.Empty);
    }

    [Test]
    public async Task CreateChecklist_ShouldCreateMultipleChecklists_GivenMultipleCreateRequests()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);

        var createChecklistRequest1 = new CreateChecklistRequest
        {
            Title = "Checklist 1"
        };

        var createChecklistRequest2 = new CreateChecklistRequest
        {
            Title = "Checklist 2"
        };

        // Assume
        var checklistsBefore = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        using var createChecklistContent1 = TestKit.CreateJsonContent(createChecklistRequest1);
        using var createChecklistResponse1 = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", createChecklistContent1);

        using var createChecklistContent2 = TestKit.CreateJsonContent(createChecklistRequest2);
        using var createChecklistResponse2 = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", createChecklistContent2);

        // Assert
        Assert.That(createChecklistResponse1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(createChecklistResponse2.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(2));

        Assert.That(checklists[0].Id, Is.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklists[0].Items, Is.Empty);

        Assert.That(checklists[1].Id, Is.EqualTo(2));
        Assert.That(checklists[1].Title, Is.EqualTo("Checklist 2"));
        Assert.That(checklists[1].Items, Is.Empty);
    }

    [TestCase("Test Checklist", "Test Checklist")]
    [TestCase("   Test Checklist", "Test Checklist")]
    [TestCase("Test Checklist   ", "Test Checklist")]
    [TestCase("   Test Checklist   ", "Test Checklist")]
    public async Task CreateChecklist_ShouldReturn_Created_AndCreateNewChecklist_WithTrimmedChecklistTitle(string title, string expectedTitle)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = title
        };

        // Assume
        var checklistsBefore = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        using var createChecklistContent = TestKit.CreateJsonContent(createChecklistRequest);
        using var createChecklistResponse = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", createChecklistContent);

        // Assert
        Assert.That(createChecklistResponse.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo(expectedTitle));
    }
}
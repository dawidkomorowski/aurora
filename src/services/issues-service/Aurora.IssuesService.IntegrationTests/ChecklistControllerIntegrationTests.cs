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
    public async Task CreateChecklist_ShouldReturn_Created_AndCreateNewChecklist_GivenIssueId()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var issueBefore = await TestKit.GetIssue(client, createIssueResponse.Id);

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = "Checklist 1"
        };

        // Assume
        var checklistsBefore = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        using var content = TestKit.CreateJsonContent(createChecklistRequest);
        using var response = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createChecklistResponse = await response.Content.ReadFromJsonAsync<CreateChecklistResponse>();
        Assert.That(createChecklistResponse, Is.Not.Null);
        Assert.That(createChecklistResponse.Id, Is.EqualTo(1));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(1));
        Assert.That(checklists[0].Id, Is.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklists[0].Items, Is.Empty);

        var issueAfter = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issueAfter.CreatedDateTime, Is.EqualTo(issueBefore.CreatedDateTime));
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
    }

    [Test]
    public async Task CreateChecklist_ShouldCreateMultipleChecklists_GivenMultipleCreateRequests()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var issueBefore = await TestKit.GetIssue(client, createIssueResponse.Id);

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
        using var content1 = TestKit.CreateJsonContent(createChecklistRequest1);
        using var response1 = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", content1);

        using var content2 = TestKit.CreateJsonContent(createChecklistRequest2);
        using var response2 = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", content2);

        // Assert
        Assert.That(response1.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        Assert.That(response2.StatusCode, Is.EqualTo(HttpStatusCode.Created));

        TestKit.AssertThatContentIsJson(response1.Content);
        TestKit.AssertThatContentIsJson(response2.Content);

        var createChecklistResponse1 = await response1.Content.ReadFromJsonAsync<CreateChecklistResponse>();
        Assert.That(createChecklistResponse1, Is.Not.Null);
        Assert.That(createChecklistResponse1.Id, Is.EqualTo(1));

        var createChecklistResponse2 = await response2.Content.ReadFromJsonAsync<CreateChecklistResponse>();
        Assert.That(createChecklistResponse2, Is.Not.Null);
        Assert.That(createChecklistResponse2.Id, Is.EqualTo(2));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(2));

        Assert.That(checklists[0].Id, Is.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklists[0].Items, Is.Empty);

        Assert.That(checklists[1].Id, Is.EqualTo(2));
        Assert.That(checklists[1].Title, Is.EqualTo("Checklist 2"));
        Assert.That(checklists[1].Items, Is.Empty);

        var issueAfter = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issueAfter.CreatedDateTime, Is.EqualTo(issueBefore.CreatedDateTime));
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
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
        using var content = TestKit.CreateJsonContent(createChecklistRequest);
        using var response = await client.PostAsync($"api/issues/{createIssueResponse.Id}/checklists", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        TestKit.AssertThatContentIsJson(response.Content);

        var createChecklistResponse = await response.Content.ReadFromJsonAsync<CreateChecklistResponse>();
        Assert.That(createChecklistResponse, Is.Not.Null);
        Assert.That(createChecklistResponse.Id, Is.EqualTo(1));

        var checklists = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklists, Has.Length.EqualTo(1));
        Assert.That(checklists[0].Title, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task GetAllChecklists_ShouldReturn_OK_AndNoChecklists_GivenIssueIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/issues/1/checklists");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var checklists = await response.Content.ReadFromJsonAsync<ChecklistResponse[]>();
        Assert.That(checklists, Is.Empty);
    }

    [Test]
    public async Task GetAllChecklists_ShouldReturn_OK_AndNoChecklists_GivenIssueIdThatHasNoChecklists()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);

        // Act
        using var response = await client.GetAsync($"api/issues/{createIssueResponse.Id}/checklists");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var checklists = await response.Content.ReadFromJsonAsync<ChecklistResponse[]>();
        Assert.That(checklists, Is.Empty);
    }

    [Test]
    public async Task GetAllChecklists_ShouldReturn_OK_AndAllChecklistsForIssue_GivenIssueIdWithChecklists()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse1 = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createIssueResponse2 = await TestKit.CreateIssue(client, "Issue 2", "Description 2", null);

        var createChecklistResponse1 = await TestKit.CreateChecklist(client, createIssueResponse1.Id, "Checklist 1");
        var createChecklistResponse2 = await TestKit.CreateChecklist(client, createIssueResponse1.Id, "Checklist 2");
        await TestKit.CreateChecklist(client, createIssueResponse2.Id, "Checklist 3");

        // Act
        using var response = await client.GetAsync($"api/issues/{createIssueResponse1.Id}/checklists");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var checklists = await response.Content.ReadFromJsonAsync<ChecklistResponse[]>();
        Assert.That(checklists, Has.Length.EqualTo(2));

        Assert.That(checklists[0].Id, Is.EqualTo(createChecklistResponse1.Id));
        Assert.That(checklists[0].Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklists[0].Items, Is.Empty);

        Assert.That(checklists[1].Id, Is.EqualTo(createChecklistResponse2.Id));
        Assert.That(checklists[1].Title, Is.EqualTo("Checklist 2"));
        Assert.That(checklists[1].Items, Is.Empty);
    }

    [Test]
    public async Task GetChecklist_ShouldReturn_NotFound_GivenChecklistIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.GetAsync("api/checklists/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetChecklist_ShouldReturn_OK_AndChecklist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createChecklistResponse = await TestKit.CreateChecklist(client, createIssueResponse.Id, "Checklist 1");

        // Act
        using var response = await client.GetAsync($"api/checklists/{createChecklistResponse.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var checklist = await response.Content.ReadFromJsonAsync<ChecklistResponse>();
        Assert.That(checklist, Is.Not.Null);
        Assert.That(checklist.Id, Is.EqualTo(createChecklistResponse.Id));
        Assert.That(checklist.Title, Is.EqualTo("Checklist 1"));
        Assert.That(checklist.Items, Is.Empty);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("   ")]
    public async Task UpdateChecklist_ShouldReturn_BadRequest_GivenInvalidChecklistTitle(string? title)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createChecklistResponse = await TestKit.CreateChecklist(client, createIssueResponse.Id, "Checklist 1");

        var updateChecklistRequest = new UpdateChecklistRequest
        {
            Title = title!
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateChecklistRequest);
        using var response = await client.PutAsync($"api/checklists/{createChecklistResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsProblemJson(response.Content);

        var validationProblemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.That(validationProblemDetails, Is.Not.Null);
        Assert.That(validationProblemDetails.Errors["Title"][0], Is.EqualTo("The Title field is required."));
    }

    [Test]
    public async Task UpdateChecklist_ShouldReturn_NotFound_GivenChecklistIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var updateChecklistRequest = new UpdateChecklistRequest
        {
            Title = "Checklist 1"
        };

        // Act
        using var content = TestKit.CreateJsonContent(updateChecklistRequest);
        using var response = await client.PutAsync("api/checklists/1", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateChecklist_ShouldReturn_OK_AndUpdateChecklist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createChecklistResponse = await TestKit.CreateChecklist(client, createIssueResponse.Id, "Checklist 1");
        var issueBefore = await TestKit.GetIssue(client, createIssueResponse.Id);

        var updateChecklistRequest = new UpdateChecklistRequest
        {
            Title = "Checklist 2"
        };

        // Assume
        var checklistBefore = await TestKit.GetChecklist(client, createChecklistResponse.Id);
        Assert.That(checklistBefore.Title, Is.EqualTo("Checklist 1"));

        // Act
        using var content = TestKit.CreateJsonContent(updateChecklistRequest);
        using var response = await client.PutAsync($"api/checklists/{createChecklistResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updatedChecklist = await response.Content.ReadFromJsonAsync<ChecklistResponse>();
        Assert.That(updatedChecklist, Is.Not.Null);
        Assert.That(updatedChecklist.Id, Is.EqualTo(createChecklistResponse.Id));
        Assert.That(updatedChecklist.Title, Is.EqualTo("Checklist 2"));
        Assert.That(updatedChecklist.Items, Is.Empty);

        var checklistAfter = await TestKit.GetChecklist(client, createChecklistResponse.Id);
        Assert.That(checklistAfter.Title, Is.EqualTo("Checklist 2"));

        var issueAfter = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issueAfter.CreatedDateTime, Is.EqualTo(issueBefore.CreatedDateTime));
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
    }

    [TestCase("Test Checklist", "Test Checklist")]
    [TestCase("   Test Checklist", "Test Checklist")]
    [TestCase("Test Checklist   ", "Test Checklist")]
    [TestCase("   Test Checklist   ", "Test Checklist")]
    public async Task UpdateChecklist_ShouldReturn_OK_AndUpdateChecklist_WithTrimmedChecklistTitle(string title, string expectedTitle)
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createChecklistResponse = await TestKit.CreateChecklist(client, createIssueResponse.Id, "Checklist 1");

        var updateChecklistRequest = new UpdateChecklistRequest
        {
            Title = title
        };

        // Assume
        var checklistBefore = await TestKit.GetChecklist(client, createChecklistResponse.Id);
        Assert.That(checklistBefore.Title, Is.EqualTo("Checklist 1"));

        // Act
        using var content = TestKit.CreateJsonContent(updateChecklistRequest);
        using var response = await client.PutAsync($"api/checklists/{createChecklistResponse.Id}", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        TestKit.AssertThatContentIsJson(response.Content);

        var updatedChecklist = await response.Content.ReadFromJsonAsync<ChecklistResponse>();
        Assert.That(updatedChecklist, Is.Not.Null);
        Assert.That(updatedChecklist.Id, Is.EqualTo(createChecklistResponse.Id));
        Assert.That(updatedChecklist.Title, Is.EqualTo(expectedTitle));
        Assert.That(updatedChecklist.Items, Is.Empty);

        var checklistAfter = await TestKit.GetChecklist(client, createChecklistResponse.Id);
        Assert.That(checklistAfter.Title, Is.EqualTo(expectedTitle));
    }

    [Test]
    public async Task DeleteChecklist_ShouldReturn_NotFound_GivenChecklistIdThatDoesNotExist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        using var response = await client.DeleteAsync("api/checklists/1");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task DeleteChecklist_ShouldReturn_NoContent_AndDeleteChecklist()
    {
        // Arrange
        using var client = _factory.CreateClient();

        var createIssueResponse = await TestKit.CreateIssue(client, "Issue 1", "Description 1", null);
        var createChecklistResponse = await TestKit.CreateChecklist(client, createIssueResponse.Id, "Checklist 1");
        var issueBefore = await TestKit.GetIssue(client, createIssueResponse.Id);

        // Assume
        var checklistsBefore = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsBefore, Has.Length.EqualTo(1));

        // Act
        using var response = await client.DeleteAsync($"api/checklists/{createChecklistResponse.Id}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var checklistsAfter = await TestKit.GetAllChecklists(client, createIssueResponse.Id);
        Assert.That(checklistsAfter, Is.Empty);

        var issueAfter = await TestKit.GetIssue(client, createIssueResponse.Id);
        Assert.That(issueAfter.CreatedDateTime, Is.EqualTo(issueBefore.CreatedDateTime));
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
    }
}
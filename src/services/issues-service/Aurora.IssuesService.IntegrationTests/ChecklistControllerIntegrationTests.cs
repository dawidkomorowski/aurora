using System.IO;
using System.Net;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
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
        // TODO Implement the test.

        // Arrange
        using var client = _factory.CreateClient();

        var createChecklistRequest = new CreateChecklistRequest
        {
            Title = title!
        };

        // Act
        using var content = TestKit.CreateJsonContent(createChecklistRequest);
        using var response = await client.PostAsync("api/checklists", content);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        TestKit.AssertThatContentIsProblemJson(response.Content);
    }
}
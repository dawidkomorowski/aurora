using System;
using System.IO;
using System.Linq;

namespace Aurora.IssuesService.DataStore.IntegrationTests;

public class IssuesStorageIntegrationTests
{
    private string _temporaryDirectoryPath = null!;
    private string _temporaryStorageFilePath = null!;

    [SetUp]
    public void SetUp()
    {
        var randomDirectoryName = Path.GetRandomFileName();
        _temporaryDirectoryPath = Path.Combine(TestContext.CurrentContext.TestDirectory, "TempDir", randomDirectoryName);
        Directory.CreateDirectory(_temporaryDirectoryPath);

        _temporaryStorageFilePath = Path.Combine(_temporaryDirectoryPath, "dev-issues-db.json");
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_temporaryDirectoryPath, true);
    }

    [Test]
    public void Constructor_CreatesNewStorageFile_WhenFileDoesNotExist()
    {
        // Arrange
        // Act
        _ = new IssuesStorage(_temporaryStorageFilePath);

        // Assert
        var fileExists = File.Exists(_temporaryStorageFilePath);
        Assert.That(fileExists, Is.True);
    }

    [Test]
    public void Create_ShouldCreateNewIssue()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);
        var createDto = new IssueCreateDto
        {
            Title = "First issue",
            Description = "This is the first issue.",
            Status = "Open"
        };

        // Assume
        var issuesBefore = issuesStorage.GetAllIssues();
        Assert.That(issuesBefore, Is.Empty);

        // Act
        var createdIssue = issuesStorage.CreateIssue(createDto);

        // Assert
        Assert.That(createdIssue.Id, Is.EqualTo(1));
        Assert.That(createdIssue.Title, Is.EqualTo(createDto.Title));
        Assert.That(createdIssue.Description, Is.EqualTo(createDto.Description));
        Assert.That(createdIssue.Status, Is.EqualTo(createDto.Status));
        Assert.That(createdIssue.CreatedDateTime, Is.GreaterThan(startTime));
        Assert.That(createdIssue.UpdatedDateTime, Is.GreaterThan(startTime));
        Assert.That(createdIssue.CreatedDateTime, Is.EqualTo(createdIssue.UpdatedDateTime));

        var issuesAfter = issuesStorage.GetAllIssues();
        Assert.That(issuesAfter, Has.Count.EqualTo(1));
        var issue1 = issuesAfter.Single();
        Assert.That(issue1, Is.EqualTo(createdIssue));
    }

    [Test]
    public void Create_ShouldCreateMultipleIssues_WhenCalledMultipleTimes()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);
        var createDto1 = new IssueCreateDto
        {
            Title = "First issue",
            Description = "This is the first issue.",
            Status = "Open"
        };

        var createDto2 = new IssueCreateDto
        {
            Title = "Second issue",
            Description = "This is the second issue.",
            Status = "In Progress"
        };

        var createDto3 = new IssueCreateDto
        {
            Title = "Third issue",
            Description = "This is the third issue.",
            Status = "Closed"
        };

        // Assume
        var issuesBefore = issuesStorage.GetAllIssues();
        Assert.That(issuesBefore, Is.Empty);

        // Act
        var createdIssue1 = issuesStorage.CreateIssue(createDto1);
        var createdIssue2 = issuesStorage.CreateIssue(createDto2);
        var createdIssue3 = issuesStorage.CreateIssue(createDto3);

        // Assert
        Assert.That(createdIssue1.Id, Is.EqualTo(1));
        Assert.That(createdIssue1.Title, Is.EqualTo(createDto1.Title));
        Assert.That(createdIssue1.Description, Is.EqualTo(createDto1.Description));
        Assert.That(createdIssue1.Status, Is.EqualTo(createDto1.Status));
        Assert.That(createdIssue1.CreatedDateTime, Is.GreaterThan(startTime));
        Assert.That(createdIssue1.UpdatedDateTime, Is.GreaterThan(startTime));
        Assert.That(createdIssue1.CreatedDateTime, Is.EqualTo(createdIssue1.UpdatedDateTime));

        Assert.That(createdIssue2.Id, Is.EqualTo(2));
        Assert.That(createdIssue2.Title, Is.EqualTo(createDto2.Title));
        Assert.That(createdIssue2.Description, Is.EqualTo(createDto2.Description));
        Assert.That(createdIssue2.Status, Is.EqualTo(createDto2.Status));
        Assert.That(createdIssue2.CreatedDateTime, Is.GreaterThan(createdIssue1.CreatedDateTime));
        Assert.That(createdIssue2.UpdatedDateTime, Is.GreaterThan(createdIssue1.UpdatedDateTime));
        Assert.That(createdIssue2.CreatedDateTime, Is.EqualTo(createdIssue2.UpdatedDateTime));

        Assert.That(createdIssue3.Id, Is.EqualTo(3));
        Assert.That(createdIssue3.Title, Is.EqualTo(createDto3.Title));
        Assert.That(createdIssue3.Description, Is.EqualTo(createDto3.Description));
        Assert.That(createdIssue3.Status, Is.EqualTo(createDto3.Status));
        Assert.That(createdIssue3.CreatedDateTime, Is.GreaterThan(createdIssue2.CreatedDateTime));
        Assert.That(createdIssue3.UpdatedDateTime, Is.GreaterThan(createdIssue2.UpdatedDateTime));
        Assert.That(createdIssue3.CreatedDateTime, Is.EqualTo(createdIssue3.UpdatedDateTime));

        var issuesAfter = issuesStorage.GetAllIssues();
        Assert.That(issuesAfter, Has.Count.EqualTo(3));

        var issue1 = issuesAfter.Single(i => i.Id == 1);
        Assert.That(issue1, Is.EqualTo(createdIssue1));

        var issue2 = issuesAfter.Single(i => i.Id == 2);
        Assert.That(issue2, Is.EqualTo(createdIssue2));

        var issue3 = issuesAfter.Single(i => i.Id == 3);
        Assert.That(issue3, Is.EqualTo(createdIssue3));
    }

    [Test]
    public void GetAllIssues_ShouldReturnNoIssues_WhenStorageFileDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        // Act
        var issues = issuesStorage.GetAllIssues();

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void GetAllIssues_ShouldReturnExistingIssues_WhenStorageFileAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);
        var createDto1 = new IssueCreateDto
        {
            Title = "First issue",
            Description = "This is the first issue.",
            Status = "Open"
        };

        var createDto2 = new IssueCreateDto
        {
            Title = "Second issue",
            Description = "This is the second issue.",
            Status = "In Progress"
        };

        var createDto3 = new IssueCreateDto
        {
            Title = "Third issue",
            Description = "This is the third issue.",
            Status = "Closed"
        };

        var createdIssue1 = issuesStorage.CreateIssue(createDto1);
        var createdIssue2 = issuesStorage.CreateIssue(createDto2);
        var createdIssue3 = issuesStorage.CreateIssue(createDto3);

        // Act
        var issuesStorage2 = new IssuesStorage(_temporaryStorageFilePath);
        var allIssues = issuesStorage2.GetAllIssues();

        // Assert
        Assert.That(allIssues, Has.Count.EqualTo(3));

        var issue1 = allIssues.Single(i => i.Id == 1);
        Assert.That(issue1, Is.EqualTo(createdIssue1));

        var issue2 = allIssues.Single(i => i.Id == 2);
        Assert.That(issue2, Is.EqualTo(createdIssue2));

        var issue3 = allIssues.Single(i => i.Id == 3);
        Assert.That(issue3, Is.EqualTo(createdIssue3));
    }
}
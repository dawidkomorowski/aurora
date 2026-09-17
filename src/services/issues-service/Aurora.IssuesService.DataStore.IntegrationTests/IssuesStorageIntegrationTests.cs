using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

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

        _temporaryStorageFilePath = Path.Combine(_temporaryDirectoryPath, "issue-service-db.json");
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
        _ = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Assert
        var fileExists = File.Exists(_temporaryStorageFilePath);
        Assert.That(fileExists, Is.True);
    }

    [Test]
    public void DeleteChecklistItem_ShouldThrowException_GivenChecklistItemThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentChecklistItemId = 123;

        // Act
        // Assert
        Assert.That(() => issuesStorage.DeleteChecklistItem(notExistentChecklistItemId), Throws.TypeOf<ChecklistItemNotFoundException>());
    }

    [Test]
    public void DeleteChecklistItem_ShouldDeleteExistingChecklistItem()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var issueCreateDto = new IssueCreateDto
        {
            Title = "Issue for checklist tests",
            Description = "This issue is used to test checklists feature.",
            Status = "In Progress"
        };
        var issueBefore = issuesStorage.CreateIssue(issueCreateDto);

        var checklistCreateDto = new ChecklistCreateDto
        {
            Title = "Checklist"
        };
        var checklist = issuesStorage.CreateChecklist(issueBefore.Id, checklistCreateDto);

        var createDto1 = new ChecklistItemCreateDto
        {
            Content = "Checklist item 1",
            IsChecked = true
        };

        var createDto2 = new ChecklistItemCreateDto
        {
            Content = "Checklist item 2",
            IsChecked = false
        };

        var createDto3 = new ChecklistItemCreateDto
        {
            Content = "Checklist item 3",
            IsChecked = true
        };

        var createdChecklistItem1 = issuesStorage.CreateChecklistItem(checklist.Id, createDto1);
        var createdChecklistItem2 = issuesStorage.CreateChecklistItem(checklist.Id, createDto2);
        var createdChecklistItem3 = issuesStorage.CreateChecklistItem(checklist.Id, createDto3);

        var issueBeforeDelete = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistItemsBefore = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsBefore, Has.Count.EqualTo(3));

        // Act
        issuesStorage.DeleteChecklistItem(createdChecklistItem2.Id);

        // Assert
        var checklistItemsAfter = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsAfter, Has.Count.EqualTo(2));

        var checklistItem1 = checklistItemsAfter.Single(ci => ci.Id == 1);
        Assert.That(checklistItem1, Is.EqualTo(createdChecklistItem1));

        var checklistItem3 = checklistItemsAfter.Single(ci => ci.Id == 3);
        Assert.That(checklistItem3, Is.EqualTo(createdChecklistItem3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeDelete.UpdatedDateTime));
    }

    [Test]
    public void StorageAccessIsThreadSafe()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Act
        var task1 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                issuesStorage.CreateIssue(GetRandomCreateIssueDto());
                _ = issuesStorage.GetAllIssues();
            }
        });

        var task2 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                issuesStorage.CreateIssue(GetRandomCreateIssueDto());
                _ = issuesStorage.GetAllIssues();
            }
        });

        var task3 = Task.Run(() =>
        {
            for (var i = 0; i < 1000; i++)
            {
                issuesStorage.CreateIssue(GetRandomCreateIssueDto());
                _ = issuesStorage.GetAllIssues();
            }
        });

        // Assert
        Task.WaitAll(task1, task2, task3);
        var issues = issuesStorage.GetAllIssues();
        Assert.That(issues, Has.Count.EqualTo(3000));
    }

    private static IssueCreateDto GetRandomCreateIssueDto()
    {
        var guid = Guid.NewGuid();
        return new IssueCreateDto
        {
            Title = $"Issue #{guid}",
            Description = $"This is the issue #{guid}.",
            Status = "Open"
        };
    }
}
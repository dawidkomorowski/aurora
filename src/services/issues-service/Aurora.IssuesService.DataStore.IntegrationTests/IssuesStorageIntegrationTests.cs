using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
    public void CreateIssue_ShouldCreateNewIssue()
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
        var issue = issuesAfter.Single();
        Assert.That(issue, Is.EqualTo(createdIssue));
    }

    [Test]
    public void CreateIssue_ShouldCreateMultipleIssues_WhenCalledMultipleTimes()
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

    [Test]
    public void GetIssue_ShouldThrowException_GivenIssueIdThatDoesNotExistInStorage()
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
        _ = issuesStorage.CreateIssue(createDto1);
        _ = issuesStorage.CreateIssue(createDto2);
        _ = issuesStorage.CreateIssue(createDto3);

        // Act
        // Assert
        Assert.That(() => _ = issuesStorage.GetIssue(12), Throws.TypeOf<IssueNotFoundException>());
    }

    [Test]
    public void GetIssue_ShouldReturnIssue_GivenIssueId()
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
        _ = issuesStorage.CreateIssue(createDto1);
        var createdIssue2 = issuesStorage.CreateIssue(createDto2);
        _ = issuesStorage.CreateIssue(createDto3);

        // Act
        var issue2 = issuesStorage.GetIssue(createdIssue2.Id);

        // Assert
        Assert.That(issue2, Is.EqualTo(createdIssue2));
    }

    [Test]
    public void UpdateIssue_ShouldThrowException_GivenIssueIdThatDoesNotExistInStorage()
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
        _ = issuesStorage.CreateIssue(createDto1);
        _ = issuesStorage.CreateIssue(createDto2);
        _ = issuesStorage.CreateIssue(createDto3);

        var updateDto = new IssueUpdateDto
        {
            Title = "Issue that does not exist",
            Description = "This is issue is not present in storage.",
            Status = "Unknown"
        };

        // Act
        // Assert
        Assert.That(() => _ = issuesStorage.UpdateIssue(12, updateDto), Throws.TypeOf<IssueNotFoundException>());
    }

    [Test]
    public void UpdateIssue_ShouldUpdateExistingIssue()
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
        _ = issuesStorage.CreateIssue(createDto1);
        var createdIssue2 = issuesStorage.CreateIssue(createDto2);
        _ = issuesStorage.CreateIssue(createDto3);

        var updateDto = new IssueUpdateDto
        {
            Title = "Updated second issue",
            Description = "This is the second issue that was updated.",
            Status = "Closed"
        };

        // Act
        var updatedIssue = issuesStorage.UpdateIssue(createdIssue2.Id, updateDto);

        // Assert
        Assert.That(updatedIssue.Id, Is.EqualTo(createdIssue2.Id));
        Assert.That(updatedIssue.Title, Is.EqualTo(updateDto.Title));
        Assert.That(updatedIssue.Description, Is.EqualTo(updateDto.Description));
        Assert.That(updatedIssue.Status, Is.EqualTo(updateDto.Status));
        Assert.That(updatedIssue.CreatedDateTime, Is.EqualTo(createdIssue2.CreatedDateTime));
        Assert.That(updatedIssue.UpdatedDateTime, Is.GreaterThan(createdIssue2.UpdatedDateTime));

        var issue2 = issuesStorage.GetIssue(createdIssue2.Id);
        Assert.That(issue2, Is.EqualTo(updatedIssue));
    }

    [Test]
    public void CreateVersion_ShouldCreateNewVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        // Assume
        var versionsBefore = issuesStorage.GetAllVersions();
        Assert.That(versionsBefore, Is.Empty);

        // Act
        var createdVersion = issuesStorage.CreateVersion(versionCreateDto);

        // Assert
        Assert.That(createdVersion.Id, Is.EqualTo(1));
        Assert.That(createdVersion.Name, Is.EqualTo(versionCreateDto.Name));

        var versionsAfter = issuesStorage.GetAllVersions();
        Assert.That(versionsAfter, Has.Count.EqualTo(1));
        var version = versionsAfter.Single();
        Assert.That(version, Is.EqualTo(createdVersion));
    }

    [Test]
    public void CreateVersion_ShouldCreateMultipleVersions_WhenCalledMultipleTimes()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        // Assume
        var versionsBefore = issuesStorage.GetAllVersions();
        Assert.That(versionsBefore, Is.Empty);

        // Act
        var createdVersion1 = issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);
        var createdVersion3 = issuesStorage.CreateVersion(versionCreateDto3);

        // Assert
        Assert.That(createdVersion1.Id, Is.EqualTo(1));
        Assert.That(createdVersion1.Name, Is.EqualTo(versionCreateDto1.Name));

        Assert.That(createdVersion2.Id, Is.EqualTo(2));
        Assert.That(createdVersion2.Name, Is.EqualTo(versionCreateDto2.Name));

        Assert.That(createdVersion3.Id, Is.EqualTo(3));
        Assert.That(createdVersion3.Name, Is.EqualTo(versionCreateDto3.Name));

        var versionsAfter = issuesStorage.GetAllVersions();
        Assert.That(versionsAfter, Has.Count.EqualTo(3));

        var version1 = versionsAfter.Single(v => v.Id == 1);
        Assert.That(version1, Is.EqualTo(createdVersion1));

        var version2 = versionsAfter.Single(v => v.Id == 2);
        Assert.That(version2, Is.EqualTo(createdVersion2));

        var version3 = versionsAfter.Single(v => v.Id == 3);
        Assert.That(version3, Is.EqualTo(createdVersion3));
    }

    [Test]
    public void GetAllVersions_ShouldReturnNoVersions_WhenStorageFileDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        // Act
        var versions = issuesStorage.GetAllVersions();

        // Assert
        Assert.That(versions, Is.Empty);
    }

    [Test]
    public void GetAllVersions_ShouldReturnExistingVersions_WhenStorageFileAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        var createdVersion1 = issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);
        var createdVersion3 = issuesStorage.CreateVersion(versionCreateDto3);

        // Act
        var issuesStorage2 = new IssuesStorage(_temporaryStorageFilePath);
        var allVersions = issuesStorage2.GetAllVersions();

        // Assert
        Assert.That(allVersions, Has.Count.EqualTo(3));

        var version1 = allVersions.Single(v => v.Id == 1);
        Assert.That(version1, Is.EqualTo(createdVersion1));

        var version2 = allVersions.Single(v => v.Id == 2);
        Assert.That(version2, Is.EqualTo(createdVersion2));

        var version3 = allVersions.Single(v => v.Id == 3);
        Assert.That(version3, Is.EqualTo(createdVersion3));
    }

    [Test]
    public void GetVersion_ShouldThrowException_GivenVersionIdThatDoesNotExistInStorage()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        _ = issuesStorage.CreateVersion(versionCreateDto1);
        _ = issuesStorage.CreateVersion(versionCreateDto2);
        _ = issuesStorage.CreateVersion(versionCreateDto3);

        // Act
        // Assert
        Assert.That(() => _ = issuesStorage.GetVersion(12), Throws.TypeOf<VersionNotFoundException>());
    }

    [Test]
    public void GetVersion_ShouldReturnVersion_GivenVersionId()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        _ = issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);
        _ = issuesStorage.CreateVersion(versionCreateDto3);

        // Act
        var version2 = issuesStorage.GetVersion(createdVersion2.Id);

        // Assert
        Assert.That(version2, Is.EqualTo(createdVersion2));
    }

    [Test]
    public void UpdateVersion_ShouldThrowException_GivenVersionIdThatDoesNotExistInStorage()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        _ = issuesStorage.CreateVersion(versionCreateDto1);
        _ = issuesStorage.CreateVersion(versionCreateDto2);
        _ = issuesStorage.CreateVersion(versionCreateDto3);

        var versionUpdateDto = new VersionUpdateDto
        {
            Name = "Version that does not exist"
        };

        // Act
        // Assert
        Assert.That(() => _ = issuesStorage.UpdateVersion(12, versionUpdateDto), Throws.TypeOf<VersionNotFoundException>());
    }

    [Test]
    public void UpdateVersion_ShouldUpdateExistingVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var versionCreateDto3 = new VersionCreateDto
        {
            Name = "Version 3"
        };

        _ = issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);
        _ = issuesStorage.CreateVersion(versionCreateDto3);

        var versionUpdateDto = new VersionUpdateDto
        {
            Name = "Updated version 2"
        };

        // Act
        var updatedVersion = issuesStorage.UpdateVersion(createdVersion2.Id, versionUpdateDto);

        // Assert
        Assert.That(updatedVersion.Id, Is.EqualTo(createdVersion2.Id));
        Assert.That(updatedVersion.Name, Is.EqualTo(versionUpdateDto.Name));

        var version2 = issuesStorage.GetVersion(createdVersion2.Id);
        Assert.That(version2, Is.EqualTo(updatedVersion));
    }

    [Test]
    public void StorageAccessIsThreadSafe()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath);

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
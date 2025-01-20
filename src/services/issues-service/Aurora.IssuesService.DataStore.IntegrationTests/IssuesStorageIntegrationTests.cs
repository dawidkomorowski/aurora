using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

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
        _ = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Assert
        var fileExists = File.Exists(_temporaryStorageFilePath);
        Assert.That(fileExists, Is.True);
    }

    [Test]
    public void CreateIssue_ShouldThrowException_GivenVersionThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
        var createDto = new IssueCreateDto
        {
            Title = "First issue",
            Description = "This is the first issue.",
            Status = "Open",
            VersionId = 123
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.CreateIssue(createDto), Throws.TypeOf<VersionNotFoundException>());
    }

    [Test]
    public void CreateIssue_ShouldCreateNewIssue()
    {
        // Arrange
        var startTime = DateTime.UtcNow;
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
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
        Assert.That(createdIssue.Version, Is.Null);

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
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
        Assert.That(createdIssue1.Version, Is.Null);

        Assert.That(createdIssue2.Id, Is.EqualTo(2));
        Assert.That(createdIssue2.Title, Is.EqualTo(createDto2.Title));
        Assert.That(createdIssue2.Description, Is.EqualTo(createDto2.Description));
        Assert.That(createdIssue2.Status, Is.EqualTo(createDto2.Status));
        Assert.That(createdIssue2.CreatedDateTime, Is.GreaterThan(createdIssue1.CreatedDateTime));
        Assert.That(createdIssue2.UpdatedDateTime, Is.GreaterThan(createdIssue1.UpdatedDateTime));
        Assert.That(createdIssue2.CreatedDateTime, Is.EqualTo(createdIssue2.UpdatedDateTime));
        Assert.That(createdIssue2.Version, Is.Null);

        Assert.That(createdIssue3.Id, Is.EqualTo(3));
        Assert.That(createdIssue3.Title, Is.EqualTo(createDto3.Title));
        Assert.That(createdIssue3.Description, Is.EqualTo(createDto3.Description));
        Assert.That(createdIssue3.Status, Is.EqualTo(createDto3.Status));
        Assert.That(createdIssue3.CreatedDateTime, Is.GreaterThan(createdIssue2.CreatedDateTime));
        Assert.That(createdIssue3.UpdatedDateTime, Is.GreaterThan(createdIssue2.UpdatedDateTime));
        Assert.That(createdIssue3.CreatedDateTime, Is.EqualTo(createdIssue3.UpdatedDateTime));
        Assert.That(createdIssue3.Version, Is.Null);

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
    public void CreateIssue_ShouldCreateNewIssue_WithVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var createdVersion = issuesStorage.CreateVersion(versionCreateDto);

        var createDto = new IssueCreateDto
        {
            Title = "First issue",
            Description = "This is the first issue.",
            Status = "Open",
            VersionId = createdVersion.Id
        };

        // Assume
        var issuesBefore = issuesStorage.GetAllIssues();
        Assert.That(issuesBefore, Is.Empty);

        // Act
        var createdIssue = issuesStorage.CreateIssue(createDto);

        // Assert
        Assert.That(createdIssue.Id, Is.EqualTo(1));
        Assert.That(createdIssue.Title, Is.EqualTo(createDto.Title));
        Assert.That(createdIssue.Version, Is.Not.Null);
        Assert.That(createdIssue.Version.Id, Is.EqualTo(createdVersion.Id));
        Assert.That(createdIssue.Version.Name, Is.EqualTo(createdVersion.Name));

        var issuesAfter = issuesStorage.GetAllIssues();
        Assert.That(issuesAfter, Has.Count.EqualTo(1));
        var issue = issuesAfter.Single();
        Assert.That(issue, Is.EqualTo(createdIssue));
    }

    [Test]
    public void GetAllIssues_ShouldReturnNoIssues_WhenStorageFileDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Act
        var issues = issuesStorage.GetAllIssues();

        // Assert
        Assert.That(issues, Is.Empty);
    }

    [Test]
    public void GetAllIssues_ShouldReturnExistingIssues_WhenStorageFileAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
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
        var issuesStorage2 = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
    public void UpdateIssue_ShouldThrowException_GivenVersionThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var createDto = new IssueCreateDto
        {
            Title = "Issue to update",
            Description = "This is will be updated.",
            Status = "In Progress"
        };

        var createdIssue = issuesStorage.CreateIssue(createDto);

        var updateDto = new IssueUpdateDto
        {
            Title = createDto.Title,
            Description = createDto.Description,
            Status = createDto.Status,
            VersionId = 123
        };

        // Act
        // Assert
        Assert.That(() => _ = issuesStorage.UpdateIssue(createdIssue.Id, updateDto), Throws.TypeOf<VersionNotFoundException>());
    }

    [Test]
    public void UpdateIssue_ShouldUpdateExistingIssue()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        Assert.That(updatedIssue.Version, Is.Null);

        var issue2 = issuesStorage.GetIssue(createdIssue2.Id);
        Assert.That(issue2, Is.EqualTo(updatedIssue));
    }

    [Test]
    public void UpdateIssue_ShouldSetVersion_GivenIssueWithNoVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var createdVersion = issuesStorage.CreateVersion(versionCreateDto);

        var createDto = new IssueCreateDto
        {
            Title = "Issue to update",
            Description = "This is will be updated.",
            Status = "In Progress"
        };

        var createdIssue = issuesStorage.CreateIssue(createDto);

        var updateDto = new IssueUpdateDto
        {
            Title = createDto.Title,
            Description = createDto.Description,
            Status = createDto.Status,
            VersionId = createdVersion.Id
        };

        // Assume
        Assert.That(createdIssue.Version, Is.Null);

        // Act
        var updatedIssue = issuesStorage.UpdateIssue(createdIssue.Id, updateDto);

        // Assert
        Assert.That(updatedIssue.Id, Is.EqualTo(createdIssue.Id));
        Assert.That(updatedIssue.Version, Is.Not.Null);
        Assert.That(updatedIssue.Version.Id, Is.EqualTo(createdVersion.Id));
        Assert.That(updatedIssue.Version.Name, Is.EqualTo(createdVersion.Name));

        var issue = issuesStorage.GetIssue(createdIssue.Id);
        Assert.That(issue, Is.EqualTo(updatedIssue));
    }

    [Test]
    public void UpdateIssue_ShouldSetNoVersion_GivenIssueWithVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var createdVersion = issuesStorage.CreateVersion(versionCreateDto);

        var issueCreateDto = new IssueCreateDto
        {
            Title = "Issue to update",
            Description = "This is will be updated.",
            Status = "In Progress",
            VersionId = createdVersion.Id
        };

        var createdIssue = issuesStorage.CreateIssue(issueCreateDto);

        var updateDto = new IssueUpdateDto
        {
            Title = issueCreateDto.Title,
            Description = issueCreateDto.Description,
            Status = issueCreateDto.Status,
            VersionId = null
        };

        // Assume
        Assert.That(createdIssue.Version, Is.Not.Null);
        Assert.That(createdIssue.Version.Id, Is.EqualTo(createdVersion.Id));
        Assert.That(createdIssue.Version.Name, Is.EqualTo(createdVersion.Name));

        // Act
        var updatedIssue = issuesStorage.UpdateIssue(createdIssue.Id, updateDto);

        // Assert
        Assert.That(updatedIssue.Id, Is.EqualTo(createdIssue.Id));
        Assert.That(updatedIssue.Version, Is.Null);

        var issue = issuesStorage.GetIssue(createdIssue.Id);
        Assert.That(issue, Is.EqualTo(updatedIssue));
    }

    [Test]
    public void UpdateIssue_ShouldSetDifferentVersion_GivenIssueWithVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        var createdVersion1 = issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);

        var issueCreateDto = new IssueCreateDto
        {
            Title = "Issue to update",
            Description = "This is will be updated.",
            Status = "In Progress",
            VersionId = createdVersion1.Id
        };

        var createdIssue = issuesStorage.CreateIssue(issueCreateDto);

        var updateDto = new IssueUpdateDto
        {
            Title = issueCreateDto.Title,
            Description = issueCreateDto.Description,
            Status = issueCreateDto.Status,
            VersionId = createdVersion2.Id
        };

        // Assume
        Assert.That(createdIssue.Version, Is.Not.Null);
        Assert.That(createdIssue.Version.Id, Is.EqualTo(createdVersion1.Id));
        Assert.That(createdIssue.Version.Name, Is.EqualTo(createdVersion1.Name));

        // Act
        var updatedIssue = issuesStorage.UpdateIssue(createdIssue.Id, updateDto);

        // Assert
        Assert.That(updatedIssue.Id, Is.EqualTo(createdIssue.Id));
        Assert.That(updatedIssue.Version, Is.Not.Null);
        Assert.That(updatedIssue.Version.Id, Is.EqualTo(createdVersion2.Id));
        Assert.That(updatedIssue.Version.Name, Is.EqualTo(createdVersion2.Name));

        var issue = issuesStorage.GetIssue(createdIssue.Id);
        Assert.That(issue, Is.EqualTo(updatedIssue));
    }

    [Test]
    public void CreateVersion_ShouldCreateNewVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
    public void CreateVersion_ShouldThrowException_WhenVersionWithTheSameNameAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        issuesStorage.CreateVersion(versionCreateDto);

        // Act
        // Assert
        Assert.That(() => issuesStorage.CreateVersion(versionCreateDto), Throws.TypeOf<VersionAlreadyExistsException>());
    }

    [Test]
    public void CreateVersion_ShouldCreateMultipleVersions_WhenCalledMultipleTimes()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Act
        var versions = issuesStorage.GetAllVersions();

        // Assert
        Assert.That(versions, Is.Empty);
    }

    [Test]
    public void GetAllVersions_ShouldReturnExistingVersions_WhenStorageFileAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage2 = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());
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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
    public void UpdateVersion_ShouldThrowException_WhenVersionWithTheSameNameAlreadyExists()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto1 = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var versionCreateDto2 = new VersionCreateDto
        {
            Name = "Version 2"
        };

        issuesStorage.CreateVersion(versionCreateDto1);
        var createdVersion2 = issuesStorage.CreateVersion(versionCreateDto2);

        var versionUpdateDto = new VersionUpdateDto
        {
            Name = "Version 1"
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.UpdateVersion(createdVersion2.Id, versionUpdateDto), Throws.TypeOf<VersionAlreadyExistsException>());
    }

    [Test]
    public void UpdateVersion_ShouldUpdateExistingVersion()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

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
    public void VersionNameAssignedToIssueIsUpdated_WhenVersionIsUpdated()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var versionCreateDto = new VersionCreateDto
        {
            Name = "Version 1"
        };

        var createdVersion = issuesStorage.CreateVersion(versionCreateDto);

        var issueCreateDto = new IssueCreateDto
        {
            Title = "Issue title",
            Description = "Issue description.",
            Status = "Open",
            VersionId = createdVersion.Id
        };

        var createdIssue = issuesStorage.CreateIssue(issueCreateDto);

        var versionUpdateDto = new VersionUpdateDto
        {
            Name = "Version 2"
        };

        // Assume
        Assert.That(createdIssue.Version, Is.Not.Null);
        Assert.That(createdIssue.Version.Id, Is.EqualTo(createdVersion.Id));
        Assert.That(createdIssue.Version.Name, Is.EqualTo(createdVersion.Name));

        // Act
        issuesStorage.UpdateVersion(createdVersion.Id, versionUpdateDto);

        // Assert
        var issue = issuesStorage.GetIssue(createdIssue.Id);
        Assert.That(issue.Version, Is.Not.Null);
        Assert.That(issue.Version.Id, Is.EqualTo(createdVersion.Id));
        Assert.That(issue.Version.Name, Is.EqualTo(versionUpdateDto.Name));
    }

    [Test]
    public void CreateChecklist_ShouldThrowException_GivenSpecifiedIssueDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentIssueId = 123;
        var createDto = new ChecklistCreateDto
        {
            Title = "Checklist title"
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.CreateChecklist(notExistentIssueId, createDto), Throws.TypeOf<IssueNotFoundException>());
    }

    [Test]
    public void CreateChecklist_ShouldCreateNewChecklistForSpecifiedIssue()
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

        var createDto = new ChecklistCreateDto
        {
            Title = "Checklist title"
        };

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        var createdChecklist = issuesStorage.CreateChecklist(issueBefore.Id, createDto);

        // Assert
        Assert.That(createdChecklist.Id, Is.EqualTo(1));
        Assert.That(createdChecklist.Title, Is.EqualTo(createDto.Title));

        var checklistsAfter = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsAfter, Has.Count.EqualTo(1));
        var checklist = checklistsAfter.Single();
        Assert.That(checklist, Is.EqualTo(createdChecklist));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
    }

    [Test]
    public void CreateChecklist_ShouldCreateMultipleChecklistsForSpecifiedIssue_WhenCalledMultipleTimes()
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

        var createDto1 = new ChecklistCreateDto
        {
            Title = "Checklist title 1"
        };

        var createDto2 = new ChecklistCreateDto
        {
            Title = "Checklist title 2"
        };

        var createDto3 = new ChecklistCreateDto
        {
            Title = "Checklist title 3"
        };

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Is.Empty);

        // Act
        var createdChecklist1 = issuesStorage.CreateChecklist(issueBefore.Id, createDto1);
        var createdChecklist2 = issuesStorage.CreateChecklist(issueBefore.Id, createDto2);
        var createdChecklist3 = issuesStorage.CreateChecklist(issueBefore.Id, createDto3);

        // Assert
        Assert.That(createdChecklist1.Id, Is.EqualTo(1));
        Assert.That(createdChecklist1.Title, Is.EqualTo(createDto1.Title));

        Assert.That(createdChecklist2.Id, Is.EqualTo(2));
        Assert.That(createdChecklist2.Title, Is.EqualTo(createDto2.Title));

        Assert.That(createdChecklist3.Id, Is.EqualTo(3));
        Assert.That(createdChecklist3.Title, Is.EqualTo(createDto3.Title));

        var checklistsAfter = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsAfter, Has.Count.EqualTo(3));

        var checklist1 = checklistsAfter.Single(c => c.Id == 1);
        Assert.That(checklist1, Is.EqualTo(createdChecklist1));

        var checklist2 = checklistsAfter.Single(c => c.Id == 2);
        Assert.That(checklist2, Is.EqualTo(createdChecklist2));

        var checklist3 = checklistsAfter.Single(c => c.Id == 3);
        Assert.That(checklist3, Is.EqualTo(createdChecklist3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBefore.UpdatedDateTime));
    }

    [Test]
    public void GetChecklist_ShouldThrowException_GivenChecklistIdThatDoesNotExistInStorage()
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

        var createDto1 = new ChecklistCreateDto
        {
            Title = "Checklist title 1"
        };

        var createDto2 = new ChecklistCreateDto
        {
            Title = "Checklist title 2"
        };

        var createDto3 = new ChecklistCreateDto
        {
            Title = "Checklist title 3"
        };


        _ = issuesStorage.CreateChecklist(issueBefore.Id, createDto1);
        _ = issuesStorage.CreateChecklist(issueBefore.Id, createDto2);
        _ = issuesStorage.CreateChecklist(issueBefore.Id, createDto3);

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Has.Count.EqualTo(3));

        // Act
        // Assert
        Assert.That(() => issuesStorage.GetChecklist(12), Throws.TypeOf<ChecklistNotFoundException>());
    }

    [Test]
    public void GetChecklist_ShouldReturnChecklist_GivenChecklistId()
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

        var createDto1 = new ChecklistCreateDto
        {
            Title = "Checklist title 1"
        };

        var createDto2 = new ChecklistCreateDto
        {
            Title = "Checklist title 2"
        };

        var createDto3 = new ChecklistCreateDto
        {
            Title = "Checklist title 3"
        };


        _ = issuesStorage.CreateChecklist(issueBefore.Id, createDto1);
        var createdChecklist2 = issuesStorage.CreateChecklist(issueBefore.Id, createDto2);
        _ = issuesStorage.CreateChecklist(issueBefore.Id, createDto3);

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Has.Count.EqualTo(3));

        // Act
        var checklist = issuesStorage.GetChecklist(createdChecklist2.Id);

        // Assert
        Assert.That(checklist, Is.EqualTo(createdChecklist2));
    }

    [Test]
    public void UpdateChecklist_ShouldThrowException_GivenChecklistThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentChecklistId = 123;
        var updateDto = new ChecklistUpdateDto
        {
            Title = "Checklist with updated title"
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.UpdateChecklist(notExistentChecklistId, updateDto), Throws.TypeOf<ChecklistNotFoundException>());
    }

    [Test]
    public void UpdateChecklist_ShouldUpdateExistingChecklist()
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

        var createDto1 = new ChecklistCreateDto
        {
            Title = "Checklist title 1"
        };

        var createDto2 = new ChecklistCreateDto
        {
            Title = "Checklist title 2"
        };

        var createDto3 = new ChecklistCreateDto
        {
            Title = "Checklist title 3"
        };

        var createdChecklist1 = issuesStorage.CreateChecklist(issueBefore.Id, createDto1);
        var createdChecklist2 = issuesStorage.CreateChecklist(issueBefore.Id, createDto2);
        var createdChecklist3 = issuesStorage.CreateChecklist(issueBefore.Id, createDto3);

        var updateDto = new ChecklistUpdateDto
        {
            Title = "Checklist with updated title"
        };

        var issueBeforeUpdate = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Has.Count.EqualTo(3));

        // Act
        var updatedChecklist2 = issuesStorage.UpdateChecklist(createdChecklist2.Id, updateDto);

        // Assert
        Assert.That(updatedChecklist2.Id, Is.EqualTo(createdChecklist2.Id));
        Assert.That(updatedChecklist2.Title, Is.EqualTo(updateDto.Title));

        var checklistsAfter = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsAfter, Has.Count.EqualTo(3));

        var checklist1 = checklistsAfter.Single(c => c.Id == createdChecklist1.Id);
        Assert.That(checklist1, Is.EqualTo(createdChecklist1));

        var checklist2 = checklistsAfter.Single(c => c.Id == createdChecklist2.Id);
        Assert.That(checklist2, Is.EqualTo(updatedChecklist2));

        var checklist3 = checklistsAfter.Single(c => c.Id == createdChecklist3.Id);
        Assert.That(checklist3, Is.EqualTo(createdChecklist3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeUpdate.UpdatedDateTime));
    }

    [Test]
    public void DeleteChecklist_ShouldThrowException_GivenChecklistThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentChecklistId = 123;

        // Act
        // Assert
        Assert.That(() => issuesStorage.DeleteChecklist(notExistentChecklistId), Throws.TypeOf<ChecklistNotFoundException>());
    }

    [Test]
    public void DeleteChecklist_ShouldDeleteExistingChecklist()
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

        var createDto1 = new ChecklistCreateDto
        {
            Title = "Checklist title 1"
        };

        var createDto2 = new ChecklistCreateDto
        {
            Title = "Checklist title 2"
        };

        var createDto3 = new ChecklistCreateDto
        {
            Title = "Checklist title 3"
        };

        var createdChecklist1 = issuesStorage.CreateChecklist(issueBefore.Id, createDto1);
        var createdChecklist2 = issuesStorage.CreateChecklist(issueBefore.Id, createDto2);
        var createdChecklist3 = issuesStorage.CreateChecklist(issueBefore.Id, createDto3);

        var issueBeforeDelete = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Has.Count.EqualTo(3));

        // Act
        issuesStorage.DeleteChecklist(createdChecklist2.Id);

        // Assert
        var checklistsAfter = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsAfter, Has.Count.EqualTo(2));

        var checklist1 = checklistsAfter.Single(c => c.Id == createdChecklist1.Id);
        Assert.That(checklist1, Is.EqualTo(createdChecklist1));

        var checklist3 = checklistsAfter.Single(c => c.Id == createdChecklist3.Id);
        Assert.That(checklist3, Is.EqualTo(createdChecklist3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeDelete.UpdatedDateTime));
    }

    [Test]
    public void DeleteChecklist_ShouldDeleteRelatedChecklistItems()
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

        issuesStorage.CreateChecklistItem(checklist.Id, createDto1);
        issuesStorage.CreateChecklistItem(checklist.Id, createDto2);
        issuesStorage.CreateChecklistItem(checklist.Id, createDto3);

        var issueBeforeDelete = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistsBefore = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsBefore, Has.Count.EqualTo(1));

        var checklistItemsBefore = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsBefore, Has.Count.EqualTo(3));

        // Act
        issuesStorage.DeleteChecklist(checklist.Id);

        // Assert
        var checklistsAfter = issuesStorage.GetAllChecklists(issueBefore.Id);
        Assert.That(checklistsAfter, Is.Empty);

        var checklistItemsAfter = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsAfter, Is.Empty);

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeDelete.UpdatedDateTime));
    }

    [Test]
    public void CreateChecklistItem_ShouldThrowException_GivenChecklistThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentChecklistId = 123;
        var createDto = new ChecklistItemCreateDto
        {
            Content = "Checklist item",
            IsChecked = true
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.CreateChecklistItem(notExistentChecklistId, createDto), Throws.TypeOf<ChecklistNotFoundException>());
    }

    [Test]
    public void CreateChecklistItem_ShouldCreateNewChecklistItem_GivenSpecifiedChecklist()
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

        var createDto = new ChecklistItemCreateDto
        {
            Content = "Checklist item",
            IsChecked = true
        };

        var issueBeforeCreate = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistItemsBefore = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsBefore, Is.Empty);

        // Act
        var createdChecklistItem = issuesStorage.CreateChecklistItem(checklist.Id, createDto);

        // Assert
        Assert.That(createdChecklistItem.Id, Is.EqualTo(1));
        Assert.That(createdChecklistItem.Content, Is.EqualTo(createDto.Content));
        Assert.That(createdChecklistItem.IsChecked, Is.EqualTo(createDto.IsChecked));

        var checklistItemsAfter = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsAfter, Has.Count.EqualTo(1));

        var checklistItem = checklistItemsAfter.Single(ci => ci.Id == 1);
        Assert.That(checklistItem, Is.EqualTo(createdChecklistItem));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeCreate.UpdatedDateTime));
    }

    [Test]
    public void CreateChecklistItem_ShouldCreateMultipleChecklistItemsForSpecifiedChecklist_WhenCalledMultipleTimes()
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

        var issueBeforeCreate = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistItemsBefore = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsBefore, Is.Empty);

        // Act
        var createdChecklistItem1 = issuesStorage.CreateChecklistItem(checklist.Id, createDto1);
        var createdChecklistItem2 = issuesStorage.CreateChecklistItem(checklist.Id, createDto2);
        var createdChecklistItem3 = issuesStorage.CreateChecklistItem(checklist.Id, createDto3);

        // Assert
        Assert.That(createdChecklistItem1.Id, Is.EqualTo(1));
        Assert.That(createdChecklistItem1.Content, Is.EqualTo(createDto1.Content));
        Assert.That(createdChecklistItem1.IsChecked, Is.EqualTo(createDto1.IsChecked));

        Assert.That(createdChecklistItem2.Id, Is.EqualTo(2));
        Assert.That(createdChecklistItem2.Content, Is.EqualTo(createDto2.Content));
        Assert.That(createdChecklistItem2.IsChecked, Is.EqualTo(createDto2.IsChecked));

        Assert.That(createdChecklistItem3.Id, Is.EqualTo(3));
        Assert.That(createdChecklistItem3.Content, Is.EqualTo(createDto3.Content));
        Assert.That(createdChecklistItem3.IsChecked, Is.EqualTo(createDto3.IsChecked));

        var checklistItemsAfter = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsAfter, Has.Count.EqualTo(3));

        var checklistItem1 = checklistItemsAfter.Single(ci => ci.Id == 1);
        Assert.That(checklistItem1, Is.EqualTo(createdChecklistItem1));

        var checklistItem2 = checklistItemsAfter.Single(ci => ci.Id == 2);
        Assert.That(checklistItem2, Is.EqualTo(createdChecklistItem2));

        var checklistItem3 = checklistItemsAfter.Single(ci => ci.Id == 3);
        Assert.That(checklistItem3, Is.EqualTo(createdChecklistItem3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeCreate.UpdatedDateTime));
    }

    [Test]
    public void UpdateChecklistItem_ShouldThrowException_GivenChecklistItemThatDoesNotExist()
    {
        // Arrange
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        var notExistentChecklistItemId = 123;
        var updateDto = new ChecklistItemUpdateDto
        {
            Content = "Checklist item",
            IsChecked = true
        };

        // Act
        // Assert
        Assert.That(() => issuesStorage.UpdateChecklistItem(notExistentChecklistItemId, updateDto), Throws.TypeOf<ChecklistItemNotFoundException>());
    }

    [Test]
    public void UpdateChecklistItem_ShouldUpdateExistingChecklistItem()
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

        var updateDto = new ChecklistItemUpdateDto
        {
            Content = "Checklist item with updated content",
            IsChecked = true
        };

        var issueBeforeUpdate = issuesStorage.GetIssue(issueBefore.Id);

        // Assume
        var checklistItemsBefore = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsBefore, Has.Count.EqualTo(3));

        // Act
        var updatedChecklistItem2 = issuesStorage.UpdateChecklistItem(createdChecklistItem2.Id, updateDto);

        // Assert
        Assert.That(updatedChecklistItem2.Id, Is.EqualTo(createdChecklistItem2.Id));
        Assert.That(updatedChecklistItem2.Content, Is.EqualTo(updateDto.Content));
        Assert.That(updatedChecklistItem2.IsChecked, Is.EqualTo(updateDto.IsChecked));

        var checklistItemsAfter = issuesStorage.GetAllChecklistItems(checklist.Id);
        Assert.That(checklistItemsAfter, Has.Count.EqualTo(3));

        var checklistItem1 = checklistItemsAfter.Single(ci => ci.Id == 1);
        Assert.That(checklistItem1, Is.EqualTo(createdChecklistItem1));

        var checklistItem2 = checklistItemsAfter.Single(ci => ci.Id == 2);
        Assert.That(checklistItem2, Is.EqualTo(updatedChecklistItem2));

        var checklistItem3 = checklistItemsAfter.Single(ci => ci.Id == 3);
        Assert.That(checklistItem3, Is.EqualTo(createdChecklistItem3));

        var issueAfter = issuesStorage.GetIssue(issueBefore.Id);
        Assert.That(issueAfter.UpdatedDateTime, Is.GreaterThan(issueBeforeUpdate.UpdatedDateTime));
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
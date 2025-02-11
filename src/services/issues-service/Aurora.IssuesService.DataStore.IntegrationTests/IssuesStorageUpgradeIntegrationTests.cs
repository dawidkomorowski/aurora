using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Aurora.IssuesService.DataStore.IntegrationTests;

public class IssuesStorageUpgradeIntegrationTests
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
    public void UpgradeFromVersion1()
    {
        // Arrange
        CopyTestFile("v1-issues-db.json");

        // Act
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Assert
        var databaseVersion = GetDatabaseVersion(_temporaryStorageFilePath);
        Assert.That(databaseVersion, Is.EqualTo(3));

        var issues = issuesStorage.GetAllIssues();
        Assert.That(issues, Has.Count.EqualTo(2));

        foreach (var issue in issues)
        {
            Assert.That(issue.Version, Is.Null);

            var checklists = issuesStorage.GetAllChecklists(issue.Id);
            Assert.That(checklists, Is.Empty);
        }

        var versions = issuesStorage.GetAllVersions();
        Assert.That(versions, Is.Empty);
    }

    [Test]
    public void UpgradeFromVersion2()
    {
        // Arrange
        CopyTestFile("v2-issues-db.json");

        // Act
        var issuesStorage = new IssuesStorage(_temporaryStorageFilePath, new NullLogger<IssuesStorage>());

        // Assert
        var databaseVersion = GetDatabaseVersion(_temporaryStorageFilePath);
        Assert.That(databaseVersion, Is.EqualTo(3));

        var issues = issuesStorage.GetAllIssues();
        Assert.That(issues, Has.Count.EqualTo(18));

        foreach (var issue in issues)
        {
            var checklists = issuesStorage.GetAllChecklists(issue.Id);
            Assert.That(checklists, Is.Empty);
        }

        var versions = issuesStorage.GetAllVersions();
        Assert.That(versions, Has.Count.EqualTo(18));
    }

    private void CopyTestFile(string fileName)
    {
        File.Copy(Path.Combine("TestFiles", fileName), _temporaryStorageFilePath);
    }

    private static int GetDatabaseVersion(string filePath)
    {
        return JsonDocument.Parse(File.ReadAllText(filePath)).RootElement.GetProperty("Version").GetInt32();
    }
}
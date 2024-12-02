using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Aurora.IssuesService.DataStore;

public interface IIssuesStorage
{
    IssueReadDto CreateIssue(IssueCreateDto issueCreateDto);
    IReadOnlyCollection<IssueReadDto> GetAllIssues();
    IssueReadDto GetIssue(int id);
    IssueReadDto UpdateIssue(int id, IssueUpdateDto issueUpdateDto);

    VersionReadDto CreateVersion(VersionCreateDto versionCreateDto);
    IReadOnlyCollection<VersionReadDto> GetAllVersions();
    VersionReadDto GetVersion(int id);
    VersionReadDto UpdateVersion(int id, VersionUpdateDto versionUpdateDto);

    ChecklistReadDto CreateChecklist(int issueId, ChecklistCreateDto checklistCreateDto);
    IReadOnlyCollection<ChecklistReadDto> GetAllChecklists(int issueId);
    ChecklistReadDto UpdateChecklist(int id, ChecklistUpdateDto checklistUpdateDto);
    void DeleteChecklist(int id);

    ChecklistItemReadDto CreateChecklistItem(int checklistId, ChecklistItemCreateDto checklistItemCreateDto);
    IReadOnlyCollection<ChecklistItemReadDto> GetAllChecklistItems(int checklistId);
    void DeleteChecklistItem(int id);
}

public sealed class IssueNotFoundException() : Exception("Issue not found.");

public sealed class VersionNotFoundException() : Exception("Version not found.");

public sealed class VersionAlreadyExistsException() : Exception("Version already exists.");

public sealed class ChecklistNotFound() : Exception("Checklist not found.");

public sealed class ChecklistItemNotFound() : Exception("Checklist item not found.");

public sealed class IssuesStorage : IIssuesStorage
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public IssuesStorage(string filePath, ILogger<IssuesStorage> logger)
    {
        _filePath = filePath;

        if (File.Exists(_filePath))
        {
            if (StorageUpgrade.IsUpgradeRequired(filePath))
            {
                logger.LogInformation("Upgrading existing database.");
                StorageUpgrade.PerformUpgrade(filePath);
                logger.LogInformation("Database upgrade complete.");
            }
            else
            {
                logger.LogInformation("Existing database is up to date.");
            }
        }
        else
        {
            logger.LogInformation("Creating new database.");

            var issuesDatabase = new IssuesDatabase
            {
                Version = StorageUpgrade.CurrentVersion,
                Issues = [],
                Versions = [],
                Checklists = [],
                ChecklistItems = []
            };
            WriteDatabaseFile(issuesDatabase);
        }
    }

    public IssueReadDto CreateIssue(IssueCreateDto issueCreateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();

            var utcNow = DateTime.UtcNow;
            var nextId = issuesDatabase.Issues.Count != 0 ? issuesDatabase.Issues.Max(i => i.Id) + 1 : 1;

            int? versionId = null;
            if (issueCreateDto.VersionId.HasValue)
            {
                versionId = issuesDatabase.GetVersion(issueCreateDto.VersionId.Value).Id;
            }

            var newIssue = new DbIssue
            {
                Id = nextId,
                Title = issueCreateDto.Title,
                Description = issueCreateDto.Description,
                Status = issueCreateDto.Status,
                CreatedDateTime = utcNow,
                UpdatedDateTime = utcNow,
                VersionId = versionId
            };

            issuesDatabase.Issues.Add(newIssue);

            WriteDatabaseFile(issuesDatabase);

            return newIssue.ToReadDto(issuesDatabase);
        }
    }

    public IReadOnlyCollection<IssueReadDto> GetAllIssues()
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.Issues.Select(i => i.ToReadDto(issuesDatabase)).ToArray();
        }
    }

    public IssueReadDto GetIssue(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.GetIssue(id).ToReadDto(issuesDatabase);
        }
    }

    public IssueReadDto UpdateIssue(int id, IssueUpdateDto issueUpdateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var issueToUpdate = issuesDatabase.GetIssue(id);
            var index = issuesDatabase.Issues.IndexOf(issueToUpdate);

            int? versionId = null;
            if (issueUpdateDto.VersionId.HasValue)
            {
                versionId = issuesDatabase.GetVersion(issueUpdateDto.VersionId.Value).Id;
            }

            issueToUpdate = new DbIssue
            {
                Id = issueToUpdate.Id,
                Title = issueUpdateDto.Title,
                Description = issueUpdateDto.Description,
                Status = issueUpdateDto.Status,
                CreatedDateTime = issueToUpdate.CreatedDateTime,
                UpdatedDateTime = DateTime.UtcNow,
                VersionId = versionId
            };

            issuesDatabase.Issues[index] = issueToUpdate;

            WriteDatabaseFile(issuesDatabase);

            return issueToUpdate.ToReadDto(issuesDatabase);
        }
    }

    public VersionReadDto CreateVersion(VersionCreateDto versionCreateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();

            if (!issuesDatabase.IsVersionNameUnique(versionCreateDto.Name))
            {
                throw new VersionAlreadyExistsException();
            }

            var nextId = issuesDatabase.Versions.Count != 0 ? issuesDatabase.Versions.Max(i => i.Id) + 1 : 1;

            var newVersion = new DbVersion
            {
                Id = nextId,
                Name = versionCreateDto.Name
            };

            issuesDatabase.Versions.Add(newVersion);

            WriteDatabaseFile(issuesDatabase);

            return newVersion.ToReadDto();
        }
    }

    public IReadOnlyCollection<VersionReadDto> GetAllVersions()
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.Versions.Select(i => i.ToReadDto()).ToArray();
        }
    }

    public VersionReadDto GetVersion(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.GetVersion(id).ToReadDto();
        }
    }

    public VersionReadDto UpdateVersion(int id, VersionUpdateDto versionUpdateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();

            if (!issuesDatabase.IsVersionNameUnique(versionUpdateDto.Name))
            {
                throw new VersionAlreadyExistsException();
            }

            var versionToUpdate = issuesDatabase.GetVersion(id);
            var index = issuesDatabase.Versions.IndexOf(versionToUpdate);

            versionToUpdate = new DbVersion
            {
                Id = versionToUpdate.Id,
                Name = versionUpdateDto.Name
            };

            issuesDatabase.Versions[index] = versionToUpdate;

            WriteDatabaseFile(issuesDatabase);

            return versionToUpdate.ToReadDto();
        }
    }

    public ChecklistReadDto CreateChecklist(int issueId, ChecklistCreateDto checklistCreateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();

            var nextId = issuesDatabase.Checklists.Count != 0 ? issuesDatabase.Checklists.Max(c => c.Id) + 1 : 1;

            var newChecklist = new DbChecklist
            {
                Id = nextId,
                IssueId = issueId,
                Title = checklistCreateDto.Title
            };

            issuesDatabase.Checklists.Add(newChecklist);
            issuesDatabase.BumpIssueUpdatedDateTime(issueId);

            WriteDatabaseFile(issuesDatabase);

            return newChecklist.ToReadDto();
        }
    }

    public IReadOnlyCollection<ChecklistReadDto> GetAllChecklists(int issueId)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.Checklists.Where(c => c.IssueId == issueId).Select(c => c.ToReadDto()).ToArray();
        }
    }

    public ChecklistReadDto UpdateChecklist(int id, ChecklistUpdateDto checklistUpdateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var checklistToUpdate = issuesDatabase.GetChecklist(id);
            var index = issuesDatabase.Checklists.IndexOf(checklistToUpdate);

            checklistToUpdate = new DbChecklist
            {
                Id = checklistToUpdate.Id,
                IssueId = checklistToUpdate.IssueId,
                Title = checklistUpdateDto.Title
            };

            issuesDatabase.Checklists[index] = checklistToUpdate;
            issuesDatabase.BumpIssueUpdatedDateTime(checklistToUpdate.IssueId);

            WriteDatabaseFile(issuesDatabase);

            return checklistToUpdate.ToReadDto();
        }
    }

    // TODO It should remove all related checklist items. How can it be tested?
    public void DeleteChecklist(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var checklistToDelete = issuesDatabase.GetChecklist(id);

            issuesDatabase.Checklists.Remove(checklistToDelete);
            issuesDatabase.BumpIssueUpdatedDateTime(checklistToDelete.IssueId);

            WriteDatabaseFile(issuesDatabase);
        }
    }

    public ChecklistItemReadDto CreateChecklistItem(int checklistId, ChecklistItemCreateDto checklistItemCreateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var checklist = issuesDatabase.GetChecklist(checklistId);

            var nextId = issuesDatabase.ChecklistItems.Count != 0 ? issuesDatabase.ChecklistItems.Max(ci => ci.Id) + 1 : 1;

            var newChecklistItem = new DbChecklistItem
            {
                Id = nextId,
                ChecklistId = checklistId,
                Content = checklistItemCreateDto.Content,
                IsChecked = checklistItemCreateDto.IsChecked
            };

            issuesDatabase.ChecklistItems.Add(newChecklistItem);
            issuesDatabase.BumpIssueUpdatedDateTime(checklist.IssueId);

            WriteDatabaseFile(issuesDatabase);

            return newChecklistItem.ToReadDto();
        }
    }

    public IReadOnlyCollection<ChecklistItemReadDto> GetAllChecklistItems(int checklistId)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.ChecklistItems.Where(ci => ci.ChecklistId == checklistId).Select(ci => ci.ToReadDto()).ToArray();
        }
    }

    public void DeleteChecklistItem(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var checklistItemToDelete = issuesDatabase.GetChecklistItem(id);
            var checklist = issuesDatabase.GetChecklist(checklistItemToDelete.ChecklistId);

            issuesDatabase.ChecklistItems.Remove(checklistItemToDelete);
            issuesDatabase.BumpIssueUpdatedDateTime(checklist.IssueId);

            WriteDatabaseFile(issuesDatabase);
        }
    }

    private IssuesDatabase ReadDatabaseFile()
    {
        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<IssuesDatabase>(json) ?? throw new InvalidOperationException("Cannot read database file.");
    }

    private void WriteDatabaseFile(IssuesDatabase issuesDatabase)
    {
        var json = JsonSerializer.Serialize(issuesDatabase);
        File.WriteAllText(_filePath, json);
    }

    private sealed class IssuesDatabase
    {
        public int Version { get; init; }
        public required List<DbIssue> Issues { get; init; }
        public required List<DbVersion> Versions { get; init; }
        public required List<DbChecklist> Checklists { get; init; }
        public required List<DbChecklistItem> ChecklistItems { get; init; }

        public DbIssue GetIssue(int id)
        {
            return Issues.SingleOrDefault(i => i.Id == id) ?? throw new IssueNotFoundException();
        }

        public void BumpIssueUpdatedDateTime(int id)
        {
            var issueToUpdate = GetIssue(id);
            var issueIndex = Issues.IndexOf(issueToUpdate);

            issueToUpdate = new DbIssue
            {
                Id = issueToUpdate.Id,
                Title = issueToUpdate.Title,
                Description = issueToUpdate.Description,
                Status = issueToUpdate.Status,
                CreatedDateTime = issueToUpdate.CreatedDateTime,
                UpdatedDateTime = DateTime.UtcNow,
                VersionId = issueToUpdate.VersionId
            };

            Issues[issueIndex] = issueToUpdate;
        }

        public DbVersion GetVersion(int id)
        {
            return Versions.SingleOrDefault(v => v.Id == id) ?? throw new VersionNotFoundException();
        }

        public bool IsVersionNameUnique(string versionName)
        {
            return Versions.All(v => v.Name != versionName);
        }

        public DbChecklist GetChecklist(int id)
        {
            return Checklists.SingleOrDefault(c => c.Id == id) ?? throw new ChecklistNotFound();
        }

        public DbChecklistItem GetChecklistItem(int id)
        {
            return ChecklistItems.SingleOrDefault(ci => ci.Id == id) ?? throw new ChecklistItemNotFound();
        }
    }

    private sealed class DbIssue
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedDateTime { get; init; }
        public required DateTime UpdatedDateTime { get; init; }
        public int? VersionId { get; init; }

        public IssueReadDto ToReadDto(IssuesDatabase issuesDatabase) => new()
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Status = Status,
            CreatedDateTime = CreatedDateTime,
            UpdatedDateTime = UpdatedDateTime,
            Version = VersionId.HasValue ? issuesDatabase.GetVersion(VersionId.Value).ToReadDto() : null
        };
    }

    private sealed class DbVersion
    {
        public required int Id { get; init; }
        public required string Name { get; init; }

        public VersionReadDto ToReadDto() => new()
        {
            Id = Id,
            Name = Name
        };
    }

    private sealed class DbChecklist
    {
        public required int Id { get; init; }
        public required int IssueId { get; init; }
        public required string Title { get; init; }

        public ChecklistReadDto ToReadDto() => new()
        {
            Id = Id,
            Title = Title
        };
    }

    private sealed class DbChecklistItem
    {
        public required int Id { get; init; }
        public required int ChecklistId { get; init; }
        public required string Content { get; init; }
        public required bool IsChecked { get; init; }

        public ChecklistItemReadDto ToReadDto() => new()
        {
            Id = Id,
            Content = Content,
            IsChecked = IsChecked
        };
    }
}
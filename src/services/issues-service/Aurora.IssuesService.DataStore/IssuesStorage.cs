﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Aurora.IssuesService.DataStore;

// TODO #37
// TODO Version property on Issue should be reference to actual Version. This Could be tested by changing existing version and checking version name on an issue.
// TODO Should it be required for version name to be unique?
// TODO When existing database is used by the storage it should be automatically upgraded.

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
}

public sealed class IssueNotFoundException() : Exception("Issue not found.");

public sealed class VersionNotFoundException() : Exception("Version not found.");

public sealed class IssuesStorage : IIssuesStorage
{
    private readonly string _filePath;
    private readonly object _lock = new();

    public IssuesStorage(string filePath)
    {
        _filePath = filePath;

        if (!File.Exists(_filePath))
        {
            var issuesDatabase = new IssuesDatabase
            {
                Version = 1,
                Issues = [],
                Versions = []
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
            return issuesDatabase.Issues.SingleOrDefault(i => i.Id == id)?.ToReadDto(issuesDatabase) ?? throw new IssueNotFoundException();
        }
    }

    public IssueReadDto UpdateIssue(int id, IssueUpdateDto issueUpdateDto)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var issueToUpdate = issuesDatabase.Issues.SingleOrDefault(i => i.Id == id) ?? throw new IssueNotFoundException();
            var index = issuesDatabase.Issues.IndexOf(issueToUpdate);

            var utcNow = DateTime.UtcNow;

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
                UpdatedDateTime = utcNow,
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
            var versionToUpdate = issuesDatabase.Versions.SingleOrDefault(i => i.Id == id) ?? throw new VersionNotFoundException();
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

        public DbVersion GetVersion(int id)
        {
            return Versions.SingleOrDefault(v => v.Id == id) ?? throw new VersionNotFoundException();
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
}
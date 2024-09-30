using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Aurora.IssuesService.DataStore;

public sealed class IssuesStorage
{
    private readonly string _filePath;

    public IssuesStorage(string filePath)
    {
        _filePath = filePath;

        if (!File.Exists(_filePath))
        {
            var issuesDatabase = new IssuesDatabase
            {
                Version = 1,
                Issues = []
            };
            WriteDatabaseFile(issuesDatabase);
        }
    }

    public IssueReadDto CreateIssue(IssueCreateDto issueCreateDto)
    {
        var issuesDatabase = ReadDatabaseFile();

        var utcNow = DateTime.UtcNow;
        var nextId = issuesDatabase.Issues.Count != 0 ? issuesDatabase.Issues.Max(i => i.Id) + 1 : 1;

        var newIssue = new DbIssue
        {
            Id = nextId,
            Title = issueCreateDto.Title,
            Description = issueCreateDto.Description,
            Status = issueCreateDto.Status,
            CreatedDateTime = utcNow,
            UpdatedDateTime = utcNow
        };

        issuesDatabase.Issues.Add(newIssue);

        WriteDatabaseFile(issuesDatabase);

        return newIssue.ToReadDto();
    }

    public IReadOnlyCollection<IssueReadDto> GetAllIssues()
    {
        var issuesDatabase = ReadDatabaseFile();

        return issuesDatabase.Issues.Select(i => i.ToReadDto()).ToArray();
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
    }

    private sealed class DbIssue
    {
        public required int Id { get; init; }
        public required string Title { get; init; }
        public required string Description { get; init; }
        public required string Status { get; init; }
        public required DateTime CreatedDateTime { get; init; }
        public required DateTime UpdatedDateTime { get; init; }

        public IssueReadDto ToReadDto() => new()
        {
            Id = Id,
            Title = Title,
            Description = Description,
            Status = Status,
            CreatedDateTime = CreatedDateTime,
            UpdatedDateTime = UpdatedDateTime
        };
    }
}

public sealed record IssueCreateDto
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
}

public sealed record IssueReadDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedDateTime { get; init; }
    public required DateTime UpdatedDateTime { get; init; }
}
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
    void DeleteIssue(int id);
}

public sealed class IssueNotFoundException() : Exception("Issue not found.");

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
                Issues = []
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
    }

    public IReadOnlyCollection<IssueReadDto> GetAllIssues()
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.Issues.Select(i => i.ToReadDto()).ToArray();
        }
    }

    public IssueReadDto GetIssue(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            return issuesDatabase.Issues.SingleOrDefault(i => i.Id == id)?.ToReadDto() ?? throw new IssueNotFoundException();
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

            issueToUpdate = new DbIssue
            {
                Id = issueToUpdate.Id,
                Title = issueUpdateDto.Title,
                Description = issueUpdateDto.Description,
                Status = issueUpdateDto.Status,
                CreatedDateTime = issueToUpdate.CreatedDateTime,
                UpdatedDateTime = utcNow
            };

            issuesDatabase.Issues[index] = issueToUpdate;

            WriteDatabaseFile(issuesDatabase);

            return issueToUpdate.ToReadDto();
        }
    }

    public void DeleteIssue(int id)
    {
        lock (_lock)
        {
            var issuesDatabase = ReadDatabaseFile();
            var issueToDelete = issuesDatabase.Issues.SingleOrDefault(i => i.Id == id) ?? throw new IssueNotFoundException();
            issuesDatabase.Issues.Remove(issueToDelete);
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
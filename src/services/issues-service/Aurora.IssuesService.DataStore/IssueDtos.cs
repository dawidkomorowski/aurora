using System;

namespace Aurora.IssuesService.DataStore;

public sealed record IssueCreateDto
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public int? VersionId { get; init; }
}

public sealed record IssueReadDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required DateTime CreatedDateTime { get; init; }
    public required DateTime UpdatedDateTime { get; init; }
    public VersionReadDto? Version { get; init; }
}

public sealed record IssueUpdateDto
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public int? VersionId { get; init; }
}
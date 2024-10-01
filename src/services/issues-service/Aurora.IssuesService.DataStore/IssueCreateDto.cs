namespace Aurora.IssuesService.DataStore;

public sealed record IssueCreateDto
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
}
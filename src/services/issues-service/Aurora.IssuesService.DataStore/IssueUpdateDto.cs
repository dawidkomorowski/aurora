namespace Aurora.IssuesService.DataStore;

public sealed record IssueUpdateDto
{
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
}
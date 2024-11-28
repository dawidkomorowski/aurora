namespace Aurora.IssuesService.DataStore;

public sealed record ChecklistCreateDto
{
    public required string Title { get; init; }
}

public sealed record ChecklistReadDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
}
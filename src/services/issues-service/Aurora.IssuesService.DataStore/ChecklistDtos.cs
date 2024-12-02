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

public sealed record ChecklistUpdateDto
{
    public required string Title { get; init; }
}

public sealed record ChecklistItemCreateDto
{
    public required string Content { get; init; }
    public required bool IsChecked { get; init; }
}

public sealed record ChecklistItemReadDto
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required bool IsChecked { get; init; }
}

public sealed record ChecklistItemUpdateDto
{
    public required string Content { get; init; }
    public required bool IsChecked { get; init; }
}
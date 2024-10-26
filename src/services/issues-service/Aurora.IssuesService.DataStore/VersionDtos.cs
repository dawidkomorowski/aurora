namespace Aurora.IssuesService.DataStore;

public sealed record VersionCreateDto
{
    public required string Name { get; init; }
}

public sealed record VersionReadDto
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public sealed record VersionUpdateDto
{
    public required string Name { get; init; }
}
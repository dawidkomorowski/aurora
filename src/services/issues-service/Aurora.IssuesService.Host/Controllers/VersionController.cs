using System.ComponentModel.DataAnnotations;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.IssuesService.Host.Controllers;

// TODO Issues Service needs to provide API for creating and editing Versions.
// TODO - API for creating new versions is introduced
// TODO - API for editing existing versions is introduced
// TODO - Version must be non empty string.
// TODO - Version must be non white space only string.
// TODO - Version string must be trimmed from white spaces.

public sealed class VersionDetailsResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public sealed class CreateVersionRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Name { get; set; } = string.Empty;
}

public sealed class CreateVersionResponse
{
    public required int Id { get; init; }
}

[ApiController]
[Route("api/versions")]
public sealed class VersionController : ControllerBase
{
    private readonly IIssuesStorage _issuesStorage;

    public VersionController(IIssuesStorage issuesStorage)
    {
        _issuesStorage = issuesStorage;
    }

    [HttpGet("{id:int}")]
    public Results<NotFound, Ok<VersionDetailsResponse>> Get(int id)
    {
        try
        {
            var versionReadDto = _issuesStorage.GetVersion(id);
            var versionDetailsResponse = new VersionDetailsResponse
            {
                Id = versionReadDto.Id,
                Name = versionReadDto.Name
            };
            return TypedResults.Ok(versionDetailsResponse);
        }
        catch (VersionNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    [HttpPost]
    public Results<BadRequest, Created<CreateVersionResponse>> Create(CreateVersionRequest createVersionRequest)
    {
        var versionCreateDto = new VersionCreateDto
        {
            Name = createVersionRequest.Name
        };

        var versionReadDto = _issuesStorage.CreateVersion(versionCreateDto);

        var uri = Url.Action(nameof(Get), new { id = versionReadDto.Id });
        var createVersionResponse = new CreateVersionResponse { Id = versionReadDto.Id };
        return TypedResults.Created(uri, createVersionResponse);
    }
}
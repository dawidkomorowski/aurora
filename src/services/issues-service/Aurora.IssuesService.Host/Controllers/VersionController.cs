using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.IssuesService.Host.Controllers;

// TODO Issues Service needs to provide API for creating and editing Versions.
// TODO - API for creating new versions is introduced
// TODO - API for editing existing versions is introduced
// TODO - Version must be non empty string.
// TODO - Version must be non white space only string.
// TODO - Version string must be trimmed from white spaces.

[ApiController]
[Route("api/versions")]
public sealed class VersionController : ControllerBase
{
    private readonly IIssuesStorage _issuesStorage;

    public VersionController(IIssuesStorage issuesStorage)
    {
        _issuesStorage = issuesStorage;
    }
}
using System.Collections.Generic;
using System.Linq;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Aurora.IssuesService.Host.Controllers;

public sealed class IssueOverviewDto
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
}

[ApiController]
[Route("issues")]
public class IssuesController : ControllerBase
{
    private readonly ILogger<IssuesController> _logger;
    private readonly IIssuesStorage _issuesStorage;

    public IssuesController(ILogger<IssuesController> logger, IIssuesStorage issuesStorage)
    {
        _logger = logger;
        _issuesStorage = issuesStorage;
    }


    [HttpGet]
    public IReadOnlyCollection<IssueOverviewDto> GetAll()
    {
        _logger.LogInformation("GetAll invoked.");

        var issues = _issuesStorage.GetAllIssues();
        var result = issues.Select(i => new IssueOverviewDto
        {
            Id = i.Id,
            Title = i.Title,
            Description = i.Description,
            Status = i.Status
        }).ToArray();

        return result;
    }
}
using System.Collections.Generic;
using System.Linq;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.IssuesService.Host.Controllers;

public sealed class ChecklistResponse
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required ChecklistItemResponse[] Items { get; init; }
}

public sealed class ChecklistItemResponse
{
    public required int Id { get; init; }
    public required string Content { get; init; }
    public required bool IsChecked { get; init; }
}

[ApiController]
[Route("api")]
public sealed class ChecklistController : ControllerBase
{
    private readonly IIssuesStorage _issuesStorage;

    public ChecklistController(IIssuesStorage issuesStorage)
    {
        _issuesStorage = issuesStorage;
    }


    [HttpGet("issues/{issueId:int}/checklists")]
    public IEnumerable<ChecklistResponse> GetAll(int issueId)
    {
        var checklists = _issuesStorage.GetAllChecklists(issueId);

        var result = checklists.Select(c =>
        {
            var items = _issuesStorage.GetAllChecklistItems(c.Id)
                .Select(ci =>
                    new ChecklistItemResponse
                    {
                        Id = ci.Id,
                        Content = ci.Content,
                        IsChecked = ci.IsChecked
                    }
                ).ToArray();

            return new ChecklistResponse
            {
                Id = c.Id,
                Title = c.Title,
                Items = items
            };
        });

        return result;
    }
}
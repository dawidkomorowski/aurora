using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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
[Route("api/issues/{issueId:int}/checklists")]
public sealed class ChecklistController : ControllerBase
{
    private readonly ILogger<ChecklistController> _logger;

    public ChecklistController(ILogger<ChecklistController> logger)
    {
        _logger = logger;
    }


    [HttpGet]
    public IEnumerable<ChecklistResponse> GetAll(int issueId)
    {
        _logger.LogInformation("Issue ID: {issueId}", issueId);

        var checklist1 = new ChecklistResponse
        {
            Id = 1,
            Title = "Checklist from API",
            Items =
            [
                new ChecklistItemResponse
                {
                    Id = 1,
                    Content = "Item 1 from API",
                    IsChecked = true
                },
                new ChecklistItemResponse
                {
                    Id = 2,
                    Content = "Item 2 from API",
                    IsChecked = false
                },
                new ChecklistItemResponse
                {
                    Id = 3,
                    Content = "Item 3 from API",
                    IsChecked = true
                }
            ]
        };

        var checklist2 = new ChecklistResponse
        {
            Id = 2,
            Title = "Acceptance criteria",
            Items =
            [
                new ChecklistItemResponse
                {
                    Id = 4,
                    Content = "Implement feature.",
                    IsChecked = true
                },
                new ChecklistItemResponse
                {
                    Id = 5,
                    Content = "Add unit tests.",
                    IsChecked = true
                },
                new ChecklistItemResponse
                {
                    Id = 6,
                    Content = "Merge to main branch.",
                    IsChecked = false
                },
                new ChecklistItemResponse
                {
                    Id = 7,
                    Content = "Manual testing.",
                    IsChecked = false
                },
                new ChecklistItemResponse
                {
                    Id = 8,
                    Content = "Deploy to DEV.",
                    IsChecked = false
                }
            ]
        };

        return [checklist1, checklist2];
    }
}
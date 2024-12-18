using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
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

public sealed class CreateChecklistRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;
}

public sealed class UpdateChecklistRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;
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
        return checklists.Select(Convert);
    }

    [HttpPost("issues/{issueId:int}/checklists")]
    public Results<BadRequest<ValidationProblemDetails>, NotFound, Created> CreateChecklist(int issueId, CreateChecklistRequest createChecklistRequest)
    {
        try
        {
            var checklistCreateDto = new ChecklistCreateDto
            {
                Title = createChecklistRequest.Title.Trim()
            };

            _issuesStorage.CreateChecklist(issueId, checklistCreateDto);

            return TypedResults.Created();
        }
        catch (IssueNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    [HttpPut("checklists/{id:int}")]
    public Results<BadRequest<ValidationProblemDetails>, NotFound, Ok<ChecklistResponse>> UpdateChecklist(int id, UpdateChecklistRequest updateChecklistRequest)
    {
        try
        {
            var checklistUpdateDto = new ChecklistUpdateDto
            {
                Title = updateChecklistRequest.Title.Trim()
            };

            var updatedChecklist = _issuesStorage.UpdateChecklist(id, checklistUpdateDto);

            return TypedResults.Ok(Convert(updatedChecklist));
        }
        catch (ChecklistNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (IssueNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    [HttpDelete("checklists/{id:int}")]
    public Results<NotFound, NoContent> DeleteChecklist(int id)
    {
        try
        {
            _issuesStorage.DeleteChecklist(id);

            return TypedResults.NoContent();
        }
        catch (ChecklistNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (IssueNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private ChecklistResponse Convert(ChecklistReadDto checklistReadDto)
    {
        var items = _issuesStorage.GetAllChecklistItems(checklistReadDto.Id)
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
            Id = checklistReadDto.Id,
            Title = checklistReadDto.Title,
            Items = items
        };
    }
}
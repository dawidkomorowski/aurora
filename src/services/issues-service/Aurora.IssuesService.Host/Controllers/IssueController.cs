﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Aurora.IssuesService.DataStore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Aurora.IssuesService.Host.Controllers;

public sealed class VersionResponse
{
    public required int Id { get; init; }
    public required string Name { get; init; }
}

public sealed class GetAllFilters
{
    public const int NoVersionId = 0;

    public string? Status { get; set; }
    public int? VersionId { get; set; }
}

public sealed class IssueOverviewResponse
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Status { get; init; }
    public VersionResponse? Version { get; init; }
}

public sealed class IssueDetailsResponse
{
    public required int Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public VersionResponse? Version { get; init; }
    public required DateTime CreatedDateTime { get; init; }
    public required DateTime UpdatedDateTime { get; init; }
}

public sealed class CreateIssueRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public int? VersionId { get; set; }
}

public sealed class CreateIssueResponse
{
    public required int Id { get; init; }
}

public sealed class UpdateIssueRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    [Required(AllowEmptyStrings = false)]
    public string Status { get; set; } = string.Empty;

    public int? VersionId { get; set; }
}

[ApiController]
[Route("api/issues")]
public sealed class IssueController : ControllerBase
{
    private readonly IIssuesStorage _issuesStorage;

    public IssueController(IIssuesStorage issuesStorage)
    {
        _issuesStorage = issuesStorage;
    }

    [HttpGet]
    public IEnumerable<IssueOverviewResponse> GetAll([FromQuery] GetAllFilters filters)
    {
        var issues = _issuesStorage.GetAllIssues().AsEnumerable();

        if (filters.Status is not null)
        {
            issues = issues.Where(i => i.Status == filters.Status);
        }

        if (filters.VersionId.HasValue)
        {
            issues = filters.VersionId.Value == GetAllFilters.NoVersionId
                ? issues.Where(i => i.Version is null)
                : issues.Where(i => i.Version?.Id == filters.VersionId);
        }

        var result = issues.Select(i => new IssueOverviewResponse
        {
            Id = i.Id,
            Title = i.Title,
            Status = i.Status,
            Version = Convert(i.Version)
        });

        return result;
    }

    [HttpGet("{id:int}")]
    public Results<NotFound, Ok<IssueDetailsResponse>> Get(int id)
    {
        try
        {
            var issue = _issuesStorage.GetIssue(id);
            return TypedResults.Ok(Convert(issue));
        }
        catch (IssueNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    [HttpPost]
    public Results<BadRequest<ValidationProblemDetails>, Created<CreateIssueResponse>> Create(CreateIssueRequest createIssueRequest)
    {
        try
        {
            var issueCreateDto = new IssueCreateDto
            {
                Title = createIssueRequest.Title,
                Description = createIssueRequest.Description,
                Status = "Open",
                VersionId = createIssueRequest.VersionId
            };

            var issueReadDto = _issuesStorage.CreateIssue(issueCreateDto);

            var uri = Url.Action(nameof(Get), new { id = issueReadDto.Id });
            var createIssueResponse = new CreateIssueResponse { Id = issueReadDto.Id };
            return TypedResults.Created(uri, createIssueResponse);
        }
        catch (VersionNotFoundException)
        {
            return BadRequest_VersionIdIsInvalid();
        }
    }

    [HttpPut("{id:int}")]
    public Results<BadRequest<ValidationProblemDetails>, NotFound, Ok<IssueDetailsResponse>> Update(int id, UpdateIssueRequest updateIssueRequest)
    {
        try
        {
            var issueUpdateDto = new IssueUpdateDto
            {
                Title = updateIssueRequest.Title,
                Description = updateIssueRequest.Description,
                Status = updateIssueRequest.Status,
                VersionId = updateIssueRequest.VersionId
            };
            var issueReadDto = _issuesStorage.UpdateIssue(id, issueUpdateDto);

            return TypedResults.Ok(Convert(issueReadDto));
        }
        catch (VersionNotFoundException)
        {
            return BadRequest_VersionIdIsInvalid();
        }
        catch (IssueNotFoundException)
        {
            return TypedResults.NotFound();
        }
    }

    private static IssueDetailsResponse Convert(IssueReadDto issueReadDto)
    {
        return new IssueDetailsResponse
        {
            Id = issueReadDto.Id,
            Title = issueReadDto.Title,
            Description = issueReadDto.Description,
            Status = issueReadDto.Status,
            Version = Convert(issueReadDto.Version),
            CreatedDateTime = issueReadDto.CreatedDateTime,
            UpdatedDateTime = issueReadDto.UpdatedDateTime
        };
    }

    private static VersionResponse? Convert(VersionReadDto? versionReadDto)
    {
        if (versionReadDto is null)
        {
            return null;
        }

        return new VersionResponse
        {
            Id = versionReadDto.Id,
            Name = versionReadDto.Name
        };
    }

    private BadRequest<ValidationProblemDetails> BadRequest_VersionIdIsInvalid()
    {
        ModelState.AddModelError("VersionId", "Specified version is invalid.");
        return TypedResults.BadRequest(ProblemDetailsFactory.CreateValidationProblemDetails(HttpContext, ModelState));
    }
}
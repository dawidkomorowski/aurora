using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Aurora.IssuesService.Host.Controllers;
using NUnit.Framework;

namespace Aurora.IssuesService.IntegrationTests;

internal static class TestKit
{
    public static StringContent CreateJsonContent(object obj)
    {
        return new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
    }

    public static void AssertThatContentIsJson(HttpContent content)
    {
        Assert.That(content.Headers.ContentType?.MediaType, Is.EqualTo("application/json"));
        Assert.That(content.Headers.ContentType?.CharSet, Is.EqualTo("utf-8"));
    }

    public static async Task<CreateIssueResponse> CreateIssue(HttpClient client, string title, string description, int? versionId)
    {
        var createIssueRequest = new CreateIssueRequest
        {
            Title = title,
            Description = description,
            VersionId = versionId
        };
        using var content = CreateJsonContent(createIssueRequest);
        using var response = await client.PostAsync("api/issues", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateIssueResponse>() ?? throw UnexpectedContent();
    }

    public static async Task<IssueOverviewResponse[]> GetAllIssues(HttpClient client)
    {
        using var response = await client.GetAsync("api/issues");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IssueOverviewResponse[]>() ?? throw UnexpectedContent();
    }

    public static async Task<IssueDetailsResponse> GetIssue(HttpClient client, int issueId)
    {
        using var response = await client.GetAsync($"api/issues/{issueId}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<IssueDetailsResponse>() ?? throw UnexpectedContent();
    }

    public static async Task<CreateVersionResponse> CreateVersion(HttpClient client, string name)
    {
        var createVersionRequest = new CreateVersionRequest
        {
            Name = name
        };
        using var content = CreateJsonContent(createVersionRequest);
        using var response = await client.PostAsync("api/versions", content);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateVersionResponse>() ?? throw UnexpectedContent();
    }

    private static InvalidOperationException UnexpectedContent() => new("Unexpected content.");
}
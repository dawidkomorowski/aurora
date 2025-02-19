using System.Net.Http;
using System.Text;
using System.Text.Json;
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
}
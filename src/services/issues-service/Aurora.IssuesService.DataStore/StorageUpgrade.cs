using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Aurora.IssuesService.DataStore;

public sealed class UpgradeException() : Exception("Upgrade exception.");

internal static class StorageUpgrade
{
    public const int CurrentVersion = 3;

    public static bool IsUpgradeRequired(string filePath)
    {
        return GetDatabaseVersion(filePath) != CurrentVersion;
    }

    public static void PerformUpgrade(string filePath)
    {
        if (GetDatabaseVersion(filePath) == 1)
        {
            UpgradeFromVersion1ToVersion2(filePath);
        }

        if (GetDatabaseVersion(filePath) == 2)
        {
            UpgradeFromVersion2ToVersion3(filePath);
        }

        if (GetDatabaseVersion(filePath) != CurrentVersion)
        {
            throw new UpgradeException();
        }
    }

    private static int GetDatabaseVersion(string filePath)
    {
        return JsonDocument.Parse(File.ReadAllText(filePath)).RootElement.GetProperty("Version").GetInt32();
    }

    private static void UpgradeFromVersion1ToVersion2(string filePath)
    {
        var database = JsonNode.Parse(File.ReadAllText(filePath)) ?? throw new UpgradeException();

        var issuesNode = database.Root["Issues"];
        if (issuesNode is null)
        {
            throw new UpgradeException();
        }

        var issues = issuesNode.AsArray();
        foreach (var issue in issues)
        {
            if (issue is null)
            {
                throw new UpgradeException();
            }

            issue["VersionId"] = null;
        }

        database.Root["Versions"] = new JsonArray();
        database.Root["Version"] = 2;

        File.WriteAllText(filePath, database.ToJsonString());
    }

    private static void UpgradeFromVersion2ToVersion3(string filePath)
    {
        var database = JsonNode.Parse(File.ReadAllText(filePath)) ?? throw new UpgradeException();

        database.Root["Checklists"] = new JsonArray();
        database.Root["ChecklistItems"] = new JsonArray();
        database.Root["Version"] = 3;

        File.WriteAllText(filePath, database.ToJsonString());
    }
}
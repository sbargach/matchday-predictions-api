using System.Data;
using System.Text.RegularExpressions;
using Microsoft.Data.SqlClient;

namespace MatchdayPredictions.Api.IntegrationTests.Infrastructure;

public sealed class DatabaseMigrator
{
    private readonly string _connectionString;

    public DatabaseMigrator(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task ApplyMigrationsAsync()
    {
        var dbProjectPath = LocateDatabaseProject();
        var tableScripts = LoadTableScripts(dbProjectPath);
        var storedProcScripts = LoadStoredProcedureScripts(dbProjectPath);

        foreach (var script in tableScripts.Concat(storedProcScripts))
        {
            await ExecuteBatchesAsync(script);
        }
    }

    private static string LocateDatabaseProject()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);

        while (dir is not null)
        {
            var solutionFile = dir.GetFiles("MatchdayPredictions.sln").FirstOrDefault();
            if (solutionFile != null)
            {
                var dbPath = Path.Combine(dir.FullName, "MatchdayPredictions.Database");
                if (!Directory.Exists(dbPath))
                {
                    throw new InvalidOperationException("Database project folder not found.");
                }

                return dbPath;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Solution root not found while locating database project.");
    }

    private static IEnumerable<string> LoadTableScripts(string dbProjectPath)
    {
        var tablesDir = Path.Combine(dbProjectPath, "dbo", "tables");

        var orderedTables = new[]
        {
            "Users.sql",
            "Leagues.sql",
            "LeagueMatches.sql",
            "Predictions.sql",
            "LeagueMembers.sql"
        };

        foreach (var tableFile in orderedTables)
        {
            var path = Path.Combine(tablesDir, tableFile);
            yield return File.ReadAllText(path);
        }
    }

    private static IEnumerable<string> LoadStoredProcedureScripts(string dbProjectPath)
    {
        var procsDir = Path.Combine(dbProjectPath, "dbo", "storedprocedures");
        var files = Directory.GetFiles(procsDir, "*.sql").OrderBy(f => f);

        foreach (var file in files)
        {
            yield return File.ReadAllText(file);
        }
    }

    private async Task ExecuteBatchesAsync(string script)
    {
        var batches = Regex.Split(script, @"^\s*GO\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        foreach (var batch in batches)
        {
            var trimmed = batch.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                continue;
            }

            await using var command = connection.CreateCommand();
            command.CommandType = CommandType.Text;
            command.CommandText = trimmed;
            await command.ExecuteNonQueryAsync();
        }
    }
}

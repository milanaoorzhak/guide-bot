using LinqToDB.Data;

namespace GuideBot.Infrastructure.DataAccess;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(
        IDataContextFactory<GuideDataContext> dataContextFactory,
        CancellationToken cancellationToken = default)
    {
        using var dataContext = dataContextFactory.CreateDataContext();

        foreach (var scriptPath in GetScriptPaths())
        {
            var sql = await File.ReadAllTextAsync(scriptPath, cancellationToken);
            foreach (var statement in SplitSqlStatements(sql))
            {
                await dataContext.ExecuteAsync(statement, cancellationToken);
            }
        }
    }

    private static IEnumerable<string> GetScriptPaths()
    {
        var baseDirectory = AppContext.BaseDirectory;
        yield return Path.Combine(baseDirectory, "database", "schema.sql");
        yield return Path.Combine(baseDirectory, "database", "seed.sql");
    }

    private static IEnumerable<string> SplitSqlStatements(string sql)
    {
        return sql
            .Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Where(statement => !string.IsNullOrWhiteSpace(statement));
    }
}

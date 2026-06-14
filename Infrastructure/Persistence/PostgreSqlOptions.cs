namespace NICK2._0_core.Infrastructure.Persistence;

public sealed class PostgreSqlOptions
{
    public const string SectionName = "PostgreSql";

    public string Host { get; init; } = "localhost";

    public int Port { get; init; } = 5432;

    public string Database { get; init; } = "nick2";

    public string Username { get; init; } = "postgres";

    public string Password { get; init; } = "postgres";
}

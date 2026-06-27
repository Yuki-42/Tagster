using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc/>
public class PgEnvironmentRepo(IDbConnectionProvider con) : IEnvironmentRepo
{
    
    private const string Cmd = "SELECT * FROM protected.environment WHERE key = @Key";

    /// <inheritdoc/>
    public EnvironmentDbm? Get(string key)
    {
        // Get connection 
        using NpgsqlConnection db = con.GetSync<NpgsqlConnection>();
        
        return db.QueryFirstOrDefault<EnvironmentDbm>(Cmd, new { Key = key });
    }

    /// <inheritdoc/>
    public async Task<EnvironmentDbm?> GetAsync(string key)
    {
        // Get connection
        await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();
        
        return await db.QueryFirstOrDefaultAsync<EnvironmentDbm>(Cmd, new { Key = key });
    }
}
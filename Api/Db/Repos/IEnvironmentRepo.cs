using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Repo for accessing db-stored environment variables.
/// </summary>
public interface IEnvironmentRepo
{
    /// <summary>
    /// Gets an environment value by key.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Environment variable if found.</returns>
    EnvironmentDbm? Get(string key);
    
    /// <summary>
    /// Gets an environment value by key.
    /// </summary>
    /// <param name="key">Key.</param>
    /// <returns>Environment variable if found.</returns>
    Task<EnvironmentDbm?> GetAsync(string key);
}
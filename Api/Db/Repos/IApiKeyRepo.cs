using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// API Key repository.
/// </summary>
public interface IApiKeyRepo
{
	/// <summary>
	/// Get API key from DB by key value.
	/// </summary>
	/// <param name="keyValue">User provided API key.</param>
	/// <returns>Key DBM if found, null if none.</returns>
	Task<ApiKeyDbm?> Get(string keyValue);

}
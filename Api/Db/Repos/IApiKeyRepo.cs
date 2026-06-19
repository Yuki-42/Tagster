using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// API Key repository.
/// </summary>
public interface IApiKeyRepo
{
	/// <summary>
	/// Get API key by key value.
	/// </summary>
	/// <param name="keyValue">User provided API key.</param>
	/// <returns>Key DBM if found, null if none.</returns>
	Task<ApiKeyDbm?> Get(string keyValue);
	
	/// <summary>
	/// Get API key by ID.
	/// </summary>
	/// <param name="id">API key ID.</param>
	/// <returns>Key DBM if found, null if none.</returns>
	Task<ApiKeyDbm?> Get(Guid id);
	
	/// <summary>
	/// Creates an API key registry in the database. 
	/// </summary>
	/// <param name="apiKey">The API key to create.</param>
	/// <returns>The created API key.</returns>
	Task<ApiKeyDbm> Insert(InsertApiKeyDbm apiKey);
	
	
}
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
	/// <param name="includeInactive">Whether to include inactive keys in the query. Default is false.</param>
	/// <returns>Key DBM if found, null if none.</returns>
	Task<ApiKeyDbm?> Get(string keyValue, bool includeInactive = false);

	/// <summary>
	/// Get API key by ID.
	/// </summary>
	/// <param name="id">API key ID.</param>
	/// <param name="includeInactive">Whether to include inactive keys in the query. Default is false.</param>
	/// <returns>Key DBM if found, null if none.</returns>
	Task<ApiKeyDbm?> Get(Guid id, bool includeInactive = false);

	/// <summary>
	/// Creates an API key registry in the database. 
	/// </summary>
	/// <param name="ob">The API key to create.</param>
	/// <returns>The created API key.</returns>
	Task<ApiKeyDbm> Insert(InsertApiKeyDbm ob);

	/// <summary>
	/// Updates an existing API key.
	/// </summary>
	/// <param name="ob">Api key with updated fields.</param>
	/// <returns>Updated API key model.</returns>
	Task<ApiKeyDbm> Update(UpdateApiKeyDbm ob);
}
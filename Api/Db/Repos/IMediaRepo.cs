using Api.Db.Models;

namespace Api.Db.Repos;

public interface IMediaRepo
{
	/// <summary>
	/// Get media by ID.
	/// </summary>
	/// <param name="id">Media ID.</param>
	/// <returns>Media if found, null if none.</returns>
	Task<MediaDbm?> Get(Guid id);
}
using Api.Db.Models;

namespace Api.Db.Repos.Impml;

/// <inheritdoc/>
public class PgMediaRepo : IMediaRepo
{
	/// <inheritdoc/>
	public Task<MediaDbm?> Get(Guid id)
	{
		throw new NotImplementedException();
	}
}
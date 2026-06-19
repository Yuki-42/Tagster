using System.Data.Common;

namespace Api.Db;

/// <inheritdoc />
public class PgDbConnectionProvider(Config allConfig) : IDbConnectionProvider
{
	private Config.DatabaseModel _config = allConfig.Database;

	/// <inheritdoc />
	public T Get<T>() where T : DbConnection
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public Task<T> GetAsync<T>() where T : DbConnection
	{
		throw new NotImplementedException();
	}
}
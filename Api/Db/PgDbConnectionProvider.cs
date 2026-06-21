using System.Data.Common;
using Npgsql;

namespace Api.Db;

/// <inheritdoc />
public class PgDbConnectionProvider : IDbConnectionProvider
{
	private readonly string _connectionString;

	/// <summary>
	/// Initializes a new instance of the <see cref="PgDbConnectionProvider"/> class with the specified configuration.
	/// </summary>
	/// <param name="allConfig">Database configuration.</param>
	public PgDbConnectionProvider(Config allConfig)
	{
		Config.DatabaseModel config = allConfig.Database;

		// Build the connection string
		_connectionString =
			$"Host={config.Host};Port={config.Port};Database={config.Name};Username={config.Credentials["Main"].Username};Password={config.Credentials["Main"].Password};";
	}

	/// <inheritdoc />
	public async Task<T> Get<T>() where T : DbConnection
	{
		// Ensure the requested type is NpgsqlConnection
		if (typeof(T) != typeof(NpgsqlConnection)) throw new InvalidOperationException($"{nameof(PgDbConnectionProvider)} only supports {nameof(NpgsqlConnection)}");

		// Create and return a new NpgsqlConnection
		NpgsqlConnection con = new(_connectionString);
		await con.OpenAsync();

		return (T)(DbConnection)con;
	}
}
using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc />
public class PgApiKeyRepo(IDbConnectionProvider con) : IApiKeyRepo
{
	/// <inheritdoc />
	public async Task<ApiKeyDbm?> Get(string keyValue, bool includeInactive = false)
	{
		// Get db connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Command text
		const string cmd = "SELECT * FROM public.api_keys WHERE key_value = @KeyValue";

		// Execute
		return await db.QueryFirstOrDefaultAsync<ApiKeyDbm>(cmd, new { KeyValue = keyValue });
	}

	/// <inheritdoc />
	public async Task<ApiKeyDbm?> Get(Guid id, bool includeInactive = false)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public async Task<ApiKeyDbm> Insert(InsertApiKeyDbm ob)
	{
		// Get db connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Command text
		const string cmd = "INSERT INTO public.api_keys (key_value, user_id, issued, expires, user_agent, ip_address, friendly_name) VALUES " +
		                   "(@KeyValue, @UserId, @Issued, @Expires, @UserAgent, @IpAddress, @FriendlyName) RETURNING *";

		return await db.QueryFirstAsync<ApiKeyDbm>(cmd, ob);
	}

	/// <inheritdoc />
	public async Task<ApiKeyDbm> Update(UpdateApiKeyDbm ob)
	{
		throw new NotImplementedException();
	}
}
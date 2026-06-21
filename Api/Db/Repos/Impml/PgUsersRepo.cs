using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc />
public class PgUsersRepo(IDbConnectionProvider con) : IUsersRepo
{
	/// <inheritdoc />
	public async Task<UserDbm?> Get(Guid id)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc />
	public async Task<UserDbm?> Get(string email)
	{
		// Get db connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Create command
		const string cmd = "SELECT * FROM public.users WHERE email = @Email";

		return await db.QueryFirstOrDefaultAsync<UserDbm>(cmd, db);
	}

	/// <inheritdoc />
	public async Task<UserDbm> Insert(InsertUserDbm user)
	{
		// Get db connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Create command
		const string cmd = "INSERT INTO public.users (username, email, password) VALUES " +
		                   "(@Username, @Email, @Password) RETURNING *";

		// Execute command
		return await db.QueryFirstAsync<UserDbm>(cmd, db);
	}
}
using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc />
public class PgTagRepo(IDbConnectionProvider con) : ITagRepo
{
	/// <inheritdoc />
	public async Task<IList<TagDbm>> Get(int pg, int count)
	{
		// Ensure sane numbers
		if (pg <= 0 || count <= 0) return [];

		// Get db
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		const string cmd = "SELECT * FROM public.tags OFFSET @Offset LIMIT @Limit";

		return (await db.QueryMultipleAsync(cmd, new { Offset = count * pg, Limit = count }))
			.Read<TagDbm>()
			.ToList();
	}

	/// <inheritdoc />
	public async Task<TagDbm?> Get(string name)
	{
		// Get db
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		const string cmd = "SELECT * FROM public.tags WHERE name = @Name";

		return await db.QueryFirstOrDefaultAsync<TagDbm>(cmd, new { Name = name });
	}

	/// <inheritdoc />
	public async Task<TagDbm?> Get(Guid id)
	{
		// Get db
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		const string cmd = "SELECT * FROM public.tags WHERE id = @Id";

		return await db.QueryFirstOrDefaultAsync<TagDbm>(cmd, new { Id = id });
	}

	/// <inheritdoc />
	public async Task<TagDbm> Insert(InsertTagDbm ob)
	{
		// Get db
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		const string cmd = "INSERT INTO public.tags (name, description, colour) VALUES (@Name, @Description, @Colour) RETURNING *";

		return await db.QueryFirstAsync<TagDbm>(cmd, ob);
	}
}
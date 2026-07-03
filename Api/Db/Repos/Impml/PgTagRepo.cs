using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc />
public class PgTagRepo(IDbConnectionProvider con) : ITagRepo
{
	/// <summary>
	/// Command text for inserting a tag. Used elsewhere and centralised here.
	/// </summary>
	public const string InsertTagCommand = "INSERT INTO public.tags (name, description, colour) VALUES (@Name, @Description, @Colour) RETURNING *";

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

		return await db.QueryFirstAsync<TagDbm>(InsertTagCommand, ob);
	}

	/// <inheritdoc />
	public async Task Delete(Guid id)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();
		const string cmd = "DELETE FROM public.tags WHERE id = @Id";
		await db.ExecuteAsync(cmd, new { Id = id });
	}
}
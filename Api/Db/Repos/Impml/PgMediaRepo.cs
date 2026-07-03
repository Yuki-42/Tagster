using Api.Db.Models;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc/>
public class PgMediaRepo(IDbConnectionProvider con) : IMediaRepo
{
	public const string InsertMediaCommand = "INSERT INTO public.media (media_type, captured, time_captured, original_name, width, height, file_size, rating) VALUES " +
	                                         "(@MediaType, @Captured, @TimeCaptured, @OriginalName, @Width, @Height, @FileSize, @Rating)";

	public const string AttachTagCommand = "INSERT INTO public.media_tags (media_id, tag_id) VALUES (@MediaId, @TagId) RETURNING *";
	private const string RemoveTagCommand = "DELETE FROM public.media_tags WHERE id = @Id";

	/// <inheritdoc/>
	public async Task<MediaDbm?> Get(Guid id)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		return await db.QueryFirstOrDefaultAsync<MediaDbm>("SELECT * FROM public.media WHERE id = @Id", new {Id = id});
	}

	/// <inheritdoc/>
	public async Task<MediaDbm> Insert(InsertMediaDbm dbm)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		return await db.QueryFirstAsync<MediaDbm>(InsertMediaCommand, dbm);
	}

	/// <inheritdoc/>
	public Task<MediaDbm> Update(MediaDbm dbm)
	{
		throw new NotImplementedException();
	}

	#region Tags

	/// <inheritdoc/>
	public async Task AttachTag(Guid mediaId, Guid tagId)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		await db.ExecuteAsync(AttachTagCommand, new { MediaId = mediaId, TagId = tagId });
	}

	/// <inheritdoc/>
	public async Task AttachTag(MediaDbm media, TagDbm tag)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		await db.ExecuteAsync(AttachTagCommand, new { MediaId = media.Id, TagId = tag.Id });
	}

	/// <inheritdoc/>
	public async Task RemoveTag(Guid mediaId, Guid tagId)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		await db.ExecuteAsync(RemoveTagCommand, new { MediaId = mediaId, TagId = tagId });
	}

	/// <inheritdoc/>
	public async Task RemoveTag(MediaDbm media, TagDbm tag)
	{
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		await db.ExecuteAsync(RemoveTagCommand, new { MediaId = media.Id, TagId = tag.Id });
	}

	#endregion
}
using Api.Db.Models;
using Api.Db.Models.Ingest;
using Dapper;
using Npgsql;

namespace Api.Db.Repos.Impml;

/// <inheritdoc/>
public class PgIngestRepo(IDbConnectionProvider con) : IIngestRepo
{
	private const string CreateIngestSessionCmd = "INSERT INTO ingests.sessions (name) VALUES (@Name) RETURNING *";
	private const string AttachTagCommand = "INSERT INTO ingests.ingest_tags (tag_id, session_id) VALUES (@TagId, @SessionId) RETURNING *";
	private const string AttachMediaCommand = "INSERT INTO ingests.ingest_media_items (media_id, session_id) VALUES (@MediaId, @SessionId) RETURNING *";

	/// <inheritdoc/>
	public Task<IngestSessionDbm?> Get(Guid id)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public Task<IngestSessionDbm?> Get(string name)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public async Task<IngestSessionDbm> Create(InsertIngestSessionDbm ingestDbm)
	{
		// Get db
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		return await db.QueryFirstAsync<IngestSessionDbm>(CreateIngestSessionCmd, ingestDbm);
	}

	/// <inheritdoc/>
	public async Task<IngestSessionDbm> Create(InsertIngestDbm ingestDbm)
	{
		// Ensure that all tags in the media items are accounted for in the tags
		if (
			ingestDbm.MediaItems.SelectMany(i => i.Tags)
			.Distinct()
			.Any(tagName =>
					ingestDbm.Tags.All(t => t.Name != tagName) // Enumerate over all tags
			)
		)
			throw new InvalidOperationException();

		// Get connection
		await using NpgsqlConnection db = await con.Get<NpgsqlConnection>();

		// Setup connection to fail safe
		await using NpgsqlTransaction trans = await db.BeginTransactionAsync();

		IngestSessionDbm sessionDbm;

		try
		{
			// First create the actual session
			 sessionDbm = await db.QueryFirstAsync<IngestSessionDbm>(CreateIngestSessionCmd, new { ingestDbm.Name }, trans);

			// Create all tags
			Dictionary<string, TagDbm> tags = new();

			// ReSharper disable once LoopCanBeConvertedToQuery  - Converting to query breaks async
			foreach (InsertTagDbm insertTagDbm in ingestDbm.Tags)
			{
				// Create tag
				TagDbm tag = await db.QueryFirstAsync<TagDbm>(PgTagRepo.InsertTagCommand, insertTagDbm, trans);

				// Link tag to ingest
				await db.ExecuteAsync(AttachTagCommand, new { TagId = tag.Id, SessionId = sessionDbm.Id });

				tags.Add(insertTagDbm.Name, tag);
			}

			// Create all media items
			foreach (InsertIngestDbm.IngestBulkMediaItem bulkMediaItem in ingestDbm.MediaItems)
			{
				// Create the media item first
				MediaDbm mediaDbm = await db.QueryFirstAsync<MediaDbm>(PgMediaRepo.InsertMediaCommand, bulkMediaItem, trans);

				// Link media item to ingest
				await db.ExecuteAsync(AttachMediaCommand, new { MediaId = mediaDbm.Id, SessionId = sessionDbm.Id });

				// Add tags to media item
				foreach (string tagName in bulkMediaItem.Tags)
				{
					await db.ExecuteAsync(PgMediaRepo.AttachTagCommand, new { MediaId = mediaDbm.Id, TagId = tags[tagName].Id });
				}
			}
		}
		catch (NpgsqlException)
		{
			await trans.RollbackAsync();
			throw;
		}

		await trans.CommitAsync();

		return sessionDbm;
	}

	/// <inheritdoc/>
	public async Task<IngestMediaItemDbm> AttachMedia(Guid ingestId, Guid mediaId, bool? exists)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public async Task<IngestTagDbm> AttachTag(Guid ingestId, Guid tagId)
	{
		throw new NotImplementedException();
	}

	/// <inheritdoc/>
	public async Task<IngestMediaItemDbm> Update(IngestMediaItemDbm dbm)
	{
		throw new NotImplementedException();
	}
}
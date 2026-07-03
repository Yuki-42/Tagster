using Api.Db.Models;
using Api.Db.Models.Ingest;

namespace Api.Db.Repos;

/// <summary>
/// Ingests repo.
/// </summary>
public interface IIngestRepo
{
	/// <summary>
	/// Get an ingest session.
	/// </summary>
	/// <param name="id">Ingest ID.</param>
	/// <returns>Ingest session.</returns>
	Task<IngestSessionDbm?> Get(Guid id);

	/// <summary>
	/// Get an ingest session.
	/// </summary>
	/// <param name="name">Ingest session name.</param>
	/// <returns>Ingest session.</returns>
	Task<IngestSessionDbm?> Get(string name);


	/// <summary>
	/// Create a new ingest session in the db.
	/// NOTE: Does not create media items or tags.
	/// </summary>
	/// <param name="ingestDbm">DBM of needed ingest session.</param>
	/// <returns>Created ingest session.</returns>
	Task<IngestSessionDbm> Create(InsertIngestSessionDbm ingestDbm);

	/// <summary>
	/// Create a new ingest session in the db and create related media and tags.
	/// </summary>
	/// <param name="ingestDbm">Whole ingest DBM.</param>
	/// <returns>Created ingest session.</returns>
	Task<IngestSessionDbm> Create(InsertIngestDbm ingestDbm);

	#region Attach

	/// <summary>
	/// Attach a media item to an ingest manually.
	/// </summary>
	/// <param name="ingestId">Ingest ID.</param>
	/// <param name="mediaId">Media ID.</param>
	/// <param name="exists">If the file is marked to exist in the filesystem yet.</param>
	/// <returns>Created ingest media item dbm.</returns>
	Task<IngestMediaItemDbm> AttachMedia(Guid ingestId, Guid mediaId, bool? exists = false);

	/// <summary>
	/// Attach a tag to an ingest manually.
	/// </summary>
	/// <param name="ingestId">Ingest ID.</param>
	/// <param name="tagId">Tag ID.</param>
	/// <returns>Created ingest-tag link.</returns>
	Task<IngestTagDbm> AttachTag(Guid ingestId, Guid tagId);

	#endregion


	/// <summary>
	/// Update an ingest media item.
	/// </summary>
	/// <returns>Updated media ingest item.</returns>
	Task<IngestMediaItemDbm> Update(IngestMediaItemDbm dbm);
}
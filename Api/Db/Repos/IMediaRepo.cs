using Api.Db.Models;

namespace Api.Db.Repos;

/// <summary>
/// Media repository.
/// </summary>
public interface IMediaRepo
{
	/// <summary>
	/// Get media by ID.
	/// </summary>
	/// <param name="id">Media ID.</param>
	/// <returns>Media if found, null if none.</returns>
	Task<MediaDbm?> Get(Guid id);

	/// <summary>
	/// Insert a new media entry.
	/// </summary>
	/// <returns>Created media.</returns>
	Task<MediaDbm> Insert(InsertMediaDbm dbm);

	/// <summary>
	/// Update an existing media row.
	/// </summary>
	/// <returns>Updated media.</returns>
	Task<MediaDbm> Update(MediaDbm dbm);

	#region Tag Management

	/// <summary>
	/// Attach a tag to a piece of media.
	/// </summary>
	Task AttachTag(Guid mediaId, Guid tagId);

	/// <inheritdoc cref="AttachTag(Guid, Guid)"/>
	Task AttachTag(MediaDbm media, TagDbm tag);

	/// <summary>
	/// Remove a tag from a piece of media.
	/// </summary>
	Task RemoveTag(Guid mediaId, Guid tagId);

	/// <inheritdoc cref="RemoveTag(Guid, Guid)"/>
	Task RemoveTag(MediaDbm media, TagDbm tag);

	#endregion

}
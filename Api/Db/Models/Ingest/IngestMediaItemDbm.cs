namespace Api.Db.Models.Ingest;

/// <summary>
/// Individual ingest session media item.
/// </summary>
public class IngestMediaItemDbm : BaseDbm
{
	/// <summary>
	/// Session ID this media item is tied to.
	/// </summary>
	public required Guid SessionId { get; init; }

	/// <summary>
	/// Actual media ID.
	/// </summary>
	public required Guid MediaId { get; init; }

	/// <summary>
	/// The state of the media item in the filesystem. Files are bulk-uploaded via a tailscale tunnel and marked completed by the ingest client.
	/// </summary>
	public required bool Exists { get; set; }
}

/// <summary>
/// DBM for inserting a new ingest media item.
/// </summary>
public class InsertIngestMediaItem
{
	/// <inheritdoc cref="IngestMediaItemDbm.SessionId" />
	public required Guid SessionId { get; init; }

	/// <inheritdoc cref="IngestMediaItemDbm.MediaId" />
	public required Guid MediaId { get; init; }

	/// <inheritdoc cref="IngestMediaItemDbm.Exists" />
	public bool Exists { get; set; } = false;
}
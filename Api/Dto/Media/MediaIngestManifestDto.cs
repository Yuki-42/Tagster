namespace Api.Dto.Media;

/// <summary>
/// DTO for a bulk media ingest session creation.
/// </summary>
public class CreateMediaIngestSession
{
	/// <summary>
	/// Name for this ingest.
	/// </summary>
	public required string IngestName { get; init; }

	/// <summary>
	/// Initial items already included in the server media directory.
	/// </summary>
	public required IList<IngestItemDto> InitialItems { get; init; }

	// ReSharper disable once ClassNeverInstantiated.Global
	/// <summary>
	/// Model for a single media item included in an ingest.
	/// </summary>
	public class IngestItemDto
	{
		/// <summary>
		/// Client-generated media item ID.
		/// </summary>
		public required Guid Id { get; init; }

		/// <summary>
		/// Tags applied to this media item.
		/// </summary>
		public required IEnumerable<Guid> Tags { get; init; }

		/// <summary>
		/// Optional media rating.
		/// </summary>
		public short? Rating { get; init; } = null;
	}
}

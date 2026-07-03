namespace Api.Dto.Media.Ingest;

/// <summary>
/// DTO for a bulk ingest session creation.
/// </summary>
public class CreateIngestSession
{
	/// <summary>
	/// Name for this ingest.
	/// </summary>
	public required string IngestName { get; init; }

	/// <summary>
	/// Initial items already included in the server media directory.
	/// </summary>
	public required IList<IngestItemDto> InitialItems { get; init; }

	/// <summary>
	/// Tags to be created with this ingest.
	/// </summary>
	public required IList<CreateTagDto> InitialTags { get; init; }

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
		/// Tags applied to this media item by tag name.
		/// </summary>
		public required IEnumerable<string> Tags { get; init; }

		/// <summary>
		/// Optional media rating.
		/// </summary>
		public short? Rating { get; init; } = null;
	}
}

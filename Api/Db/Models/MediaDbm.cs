namespace Api.Db.Models;

public class MediaDbm : BaseDbm
{
	public required string OriginalName { get; init; }
	public required FileType FileType { get; init; }
	public required long Size { get; init; }
	public MediaDimensions? Dimensions { get; init; }
}

public record MediaDimensions
{
	public required int Width { get; init; }
	public required int Height { get; init; }
}

/// <summary>
/// Basic file type.
/// </summary>
public enum FileType
{
	/// <summary>
	/// Web-renderable image file type.
	/// </summary>
	Image,

	/// <summary>
	/// Web-renderable video file type.
	/// </summary>
	Video,

	/// <summary>
	/// Generic/catchall binary file type.
	/// </summary>
	Binary
}
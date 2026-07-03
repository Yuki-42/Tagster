namespace Api.Dto.Media;

/// <summary>
/// Generic DTO for all media objects.
/// </summary>
public class MediaDto
{
	/// <summary>
	/// Media item ID.
	/// </summary>
	public required Guid Id { get; init; }

	/// <summary>
	/// Original date of capture.
	/// </summary>
	public required DateOnly Captured { get; init; }

	/// <summary>
	/// Type of this media.
	/// </summary>
	public required MediaType MediaType { get; init; }

	/// <summary>
	/// Optional time of original capture.
	/// </summary>
	public TimeOnly? TimeCaptured { get; init; }

	/// <summary>
	/// Information about this media file.
	/// </summary>
	public required MediaFileInfo FileInfo { get; init; }

	/// <summary>
	/// Star rating of this media item.
	/// </summary>
	public int? Rating { get; init; }

	/// <summary>
	/// Tags attached to this image.
	/// </summary>
	public required IList<TagDto> Tags { get; init; }
}

/// <summary>
/// Media dimensions representation.
/// </summary>
public class MediaFileInfo
{
	/// <summary>
	/// Media width.
	/// </summary>
	public required int Width { get; init; }

	/// <summary>
	/// Media height.
	/// </summary>
	public required int Height { get; init; }

	/// <summary>
	/// Original file name before import.
	/// </summary>
	public required string OriginalName { get; init; }

	/// <summary>
	/// Size of the file in bytes.
	/// </summary>
	public required ulong FileSize { get; init; }
}

/// <summary>
/// Supported media types.
/// </summary>
public enum MediaType
{
	/// <summary>
	/// A still image. Can be in any web-compatible format.
	/// </summary>
	Still,

	/// <summary>
	/// Video file. This will need special handling with NGINX.
	/// </summary>
	Video,

	/// <summary>
	/// Animated image file. Essentially just a gif or animated WebP.
	/// </summary>
	AnimatedImage
}

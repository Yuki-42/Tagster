using Api.Dto.Media;

namespace Api.Db.Models;

/// <summary>
/// Database model for a media entity.
/// </summary>
public class MediaDbm : BaseDbm
{
	/// <inheritdoc cref="MediaDto.MediaType"/>
	public required MediaType FileType { get; init; }

	/// <inheritdoc cref="MediaDto.Captured"/>
	public required DateOnly Captured { get; init; }

	/// <inheritdoc cref="MediaDto.TimeCaptured"/>
	public TimeOnly? TimeCaptured { get; init; }

	/// <inheritdoc cref="MediaDto.MediaType"/>
	public required MediaType MediaType { get; init; }

	#region Encoded In MediaFileInfo

	/// <inheritdoc cref="MediaFileInfo.OriginalName"/>
	public required string OriginalName { get; init; }

	/// <inheritdoc cref="MediaFileInfo.Width"/>
	public required int Width { get; init; }

	/// <inheritdoc cref="MediaFileInfo.Height"/>
	public required int Height { get; init; }

	/// <inheritdoc cref="MediaFileInfo.FileSize"/>
	public required ulong FileSize { get; init; }

	#endregion
}
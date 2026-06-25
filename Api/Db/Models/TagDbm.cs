using Api.Dto;
using Api.Dto.Media;

namespace Api.Db.Models;

/// <summary>
/// Database model for a media tag.
/// </summary>
[MappedObject(typeof(TagDto))]
public class TagDbm: BaseDbm
{
	/// <inheritdoc cref="TagDto.Name"/>
	public required string Name { get; set; }

	/// <inheritdoc cref="TagDto.Colour"/>
	public string? Colour { get; set; } = null;

	/// <inheritdoc cref="TagDto.Description"/>
	public string? Description { get; init; }
}

/// <summary>
/// Database model used for creating new tags.
/// </summary>
public class InsertTagDbm
{
	/// <inheritdoc cref="TagDto.Name"/>
	public required string Name { get; set; }

	/// <inheritdoc cref="TagDto.Colour"/>
	public string? Colour { get; set; } = null;

	/// <inheritdoc cref="TagDto.Description"/>
	public string? Description { get; init; }
}
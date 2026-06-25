using Api.Db.Models;

namespace Api.Dto.Media;

/// <summary>
/// DTO representing a media tag.
/// </summary>
public class TagDto
{
	/// <summary>
	/// Tag ID.
	/// </summary>
	public required Guid Id { get; init; }

	/// <summary>
	/// Tag name.
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Tag colour as hexadecimal.
	/// </summary>
	public string? Colour { get; set; } = null;

	/// <summary>
	/// Tag description.
	/// </summary>
	public string? Description { get; init; }
}

/// <summary>
/// DTO used to create a tag.
/// </summary>
[MappedObject(typeof(InsertTagDbm))]
public class CreateTagDto
{
	/// <inheritdoc cref="TagDto.Name" />
	public required string Name { get; set; }

	/// <inheritdoc cref="TagDto.Colour" />
	public string? Colour { get; set; } = null;

	/// <inheritdoc cref="TagDto.Description" />
	public string? Description { get; init; }
}
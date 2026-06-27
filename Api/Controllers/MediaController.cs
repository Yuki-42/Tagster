using Api.Db.Models;
using Api.Db.Repos;
using Api.Dto;
using Api.Dto.Media;
using Api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Endpoint for viewing and editing media.
/// </summary>
[ApiController]
public class MediaController : ControllerBase
{
	private readonly ILogger<MediaController> _logger;

	private readonly IMediaRepo _media;
	private readonly ITagRepo _tags;
	private readonly IAuditLogRepo _audit;

	/// <inheritdoc cref="MediaController" />
	public MediaController(ILogger<MediaController> logger, IMediaRepo media, ITagRepo tags, IAuditLogRepo audit)
	{
		_logger = logger;
		_media = media;
		_tags = tags;
		_audit = audit;
	}

	/// <summary>
	/// Gets media information by ID.
	/// </summary>
	/// <param name="id">Media ID.</param>
	/// <response code="200">Returns the media information.</response>
	/// <response code="404">If the media is not found.</response>
	[HttpGet("/{id:guid}")]
	public async Task<ActionResult<MediaDto>> GetMedia(Guid id)
	{
		// Attempt to find the media with the given ID.
		MediaDbm? media = await _media.Get(id);
		if (media is null) return NotFound();

		return DtoMapper.Map<MediaDbm, MediaDto>(media);
	}

	#region Tag Managment

	/// <summary>
	/// Lists existing tags in alphabetical order.
	/// </summary>
	/// <param name="pg">Page number.</param>
	/// <param name="count">Number of items returned per page. Maximum of 500.</param>
	[HttpGet("/tags/")]
	[AllowAnonymous]
	public async Task<ActionResult<IList<TagDto>>> ListTags([FromQuery] int pg = 0, [FromQuery] int count = 100)
	{
		// Ensure count does not exceed 500
		if (count is <= 0 or > 500) return BadRequest("Count out of bounds.");

		// Get tags from db
		IList<TagDbm> tagDbms = await _tags.Get(pg, count);

		// Convert dbms to dtos
		return tagDbms.Select(DtoMapper.Map<TagDbm, TagDto>).ToList();
	}

	/// <summary>
	/// Creates a new tag.
	/// </summary>
	/// <param name="dto">Information required to create a tag.</param>
	[ApiKey(ApiKeyPermissions.CreateTag)]
	[HttpPost("/tags")]
	public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagDto dto)
	{
		// Ensure tag name does not already exist
		TagDbm? tag = await _tags.Get(dto.Name);

		if (tag != null) return Conflict("Tag already exists");

		return DtoMapper.Map<TagDbm, TagDto>(
			await _tags.Insert(
				DtoMapper.Map<CreateTagDto, InsertTagDbm>(dto)
			)
		);
	}

	/// <summary>
	/// Delete a tag. 
	/// </summary>
	/// <param name="id">Tag ID to delete.</param>
	[HttpDelete("/tags/{id:guid}")]
	[ApiKey(ApiKeyPermissions.DeleteTag)]
	public async Task<ActionResult> DeleteTag(Guid id)
	{
		// Check if tag exists
		if (await _tags.Get(id) is null) return NotFound();
		
		// Delete tag 
		await _tags.Delete(id);

		return Ok();
	}

	#endregion
}
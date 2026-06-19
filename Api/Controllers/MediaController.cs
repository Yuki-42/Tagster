using Api.Db.Models;
using Api.Db.Repos;
using Api.Dto;
using Api.Dto.Media;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Endpoint for viewing and editing media.
/// </summary>
[ApiController]
public class MediaController(IMediaRepo mediaRepo) : ControllerBase
{
    /// <summary>
    /// Gets media information by ID.
    /// </summary>
    /// <param name="id">Media ID.</param>
    /// <response code="200">Returns the media information.</response>
    /// <response code="404">If the media is not found.</response>
    /// <returns>The media information if found.</returns>
    [HttpGet("/{id:guid}")]
    public async Task<ActionResult<MediaDto>> GetMedia(Guid id)
    {
        // Attempt to find the media with the given ID.
        MediaDbm? media = await mediaRepo.Get(id);

        if (media is null) return NotFound();
        
        // Attempt to map
        MediaDto mediaDto;
        try
        {
            mediaDto = DtoMapper.Map<MediaDbm, MediaDto>(media);
        }
        catch (Exception ex)
        {
            // Handle mapping errors
            return BadRequest("Error occurred while mapping media data.");
        }

        return Ok(mediaDto);
    }
}
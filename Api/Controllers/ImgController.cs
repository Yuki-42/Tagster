using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Endpoint used for retrieving images and correctly adding required metadata.
/// </summary>
public class ImgController : ControllerBase
{
	/// <summary>
	/// Gets an original quality image in an embeddable format.
	/// </summary>
	/// <param name="id">Media ID.</param>
	/// <returns>Media.</returns>
	[HttpGet("/original/{id:guid}")]
	public async Task<ActionResult> GetOriginal(Guid id)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// Gets a thumbnail quality image if exists in an embeddable format.
	/// </summary>
	/// <param name="id">Media ID.</param>
	/// <returns>Media.</returns>
	[HttpGet("/thumbnail/{id:guid}")]
	public async Task<ActionResult> GetThumbnail(Guid id)
	{
		throw new NotImplementedException();
	}

}
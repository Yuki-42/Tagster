using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Api.Filters;

/// <summary>
/// Ensures that the user is not authenticated. This is used to prevent authenticated users from accessing endpoints that should only be accessible to unauthenticated users, such as the signup endpoint.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NotAuthenticatedAttribute : Attribute, IAsyncActionFilter
{
	/// <summary>
	/// Checks for an API key in the request header and blocks access if an API key is present.
	/// </summary>
	/// <param name="context">The action executing context.</param>
	/// <param name="next">The action execution delegate.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		// Try and retrieve the API key from the headder
		if (context.HttpContext.Request.Headers.TryGetValue("key", out _))
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status403Forbidden
			};

		return next();
	}
}
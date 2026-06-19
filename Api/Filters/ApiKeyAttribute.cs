using Api.Db;
using Api.Db.Models;
using Api.Db.Repos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace Api.Filters;

/// <summary>
/// Ensures that a user attempting to access the endpoint has a valid API key and the API key is valid for this request.
/// </summary>
/// <param name="permissions">Permissions required to access this endpoint.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class ApiKeyAttribute(ApiKeyPermissions permissions) : Attribute, IAsyncActionFilter
{
	/// <summary>
	/// Checks for a valid API key in the request header and ensures it has the required permissions to access this endpoint. 
	/// </summary>
	/// <param name="context">The action executing context.</param>
	/// <param name="next">The action execution delegate.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
	{
		// Try and retrieve API key from header
		if (!context.HttpContext.Request.Headers.TryGetValue("key", out StringValues keyString) || keyString == "")
		{
			goto NotAuthenticated;
		}

		// Get api keys repo
		IApiKeyRepo keyRepo = context.HttpContext.RequestServices.GetRequiredService<IApiKeyRepo>();

		ApiKeyDbm? key = await keyRepo.Get(keyString!);

		if (key is null) goto NotAuthenticated;

		// Ensure key has required permissions flags
		if ((key.Permissions & permissions) != permissions) goto NotAllowed;

		// Key has required permissions, allow execution to proceed after logging key usage
		await context.HttpContext.RequestServices.GetRequiredService<IAuditLogsRepo>().Create(new CreateAuditLogDbo
		{
			TableName = DbNaming.TblApiKeys,
			ActionType = AuditActionType.Access,
			RowId = key.Id,
			UserId = key.UserId
		});


		await next();
		return;  // Prevent execution from continuing to fail states

		NotAuthenticated:
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status401Unauthorized
			};

			return;

		NotAllowed:
			context.Result = new ContentResult
			{
				StatusCode = StatusCodes.Status403Forbidden
			};
			return;
	}
}
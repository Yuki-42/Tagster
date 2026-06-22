using System.Text;
using System.Text.Json;
using Api.Db;
using Api.Db.Models;
using Api.Db.Repos;
using Api.Dto;
using Api.Dto.Auth;
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
		string errorMessage;

		// Try and retrieve API key from header
		if (!context.HttpContext.Request.Headers.TryGetValue("key", out StringValues keyString) || keyString == "")
		{
			errorMessage = "No API key provided";
			goto NotAuthenticated;
		}

		// Try and convert key string into a sane value 
		ApiKeyDto? netKey = JsonSerializer.Deserialize<ApiKeyDto>(Encoding.UTF8.GetString(Convert.FromBase64String(keyString!)));

		if (netKey is null)
		{
			errorMessage = "Invalid API key format";
			goto NotAuthenticated;
		}

		// Get api keys repo
		IApiKeyRepo keyRepo = context.HttpContext.RequestServices.GetRequiredService<IApiKeyRepo>();
		IAuditLogRepo logRepo = context.HttpContext.RequestServices.GetRequiredService<IAuditLogRepo>();

		ApiKeyDbm? dbKey = await keyRepo.Get(netKey.Signature);

		if (dbKey is null)
		{
			errorMessage = "API key not found";
			goto NotAuthenticated;
		}

		// Ensure that both versions of the key match
		if (dbKey.KeyValue != netKey.Signature ||
		    dbKey.Issued != netKey.Issued ||
		    dbKey.Expires != netKey.Expires)
		{
			_ = logRepo.Create(new CreateAuditLogDbo
			{
				TableName = DbHelpers.TblApiKeys,
				ActionType = AuditActionType.Access,
				RowId = dbKey.Id,
				UserId = dbKey.UserId,
				Comment = "Invalid API key data. Potential key spoof attempt."
			});
			errorMessage = "API key is invalid";
			goto NotAuthenticated;
		}

		// Ensure hidden attributes match correctly (for now this is just the user agent header which is incredibly easy to spoof
		// For this reason, we do not expose the user agent header for the DTO 
		if (dbKey.UserAgent != context.HttpContext.Request.Headers.UserAgent)
		{
			_ = logRepo.Create(new CreateAuditLogDbo
			{
				TableName = DbHelpers.TblApiKeys,
				ActionType = AuditActionType.Access,
				RowId = dbKey.Id,
				UserId = dbKey.UserId,
				Comment = $"Attempted use of this key on incorrect user agent ({context.HttpContext.Request.Headers.UserAgent})"
			});
			errorMessage = "No valid API key for this device";
			goto NotAuthenticated;
		}

		// Ensure key is not expired
		if (dbKey.Expires <= DateTime.UtcNow)
		{
			// Give 1 week of "API key is expired" messages before we delete the API key and start giving "API key not found" messages
			if (dbKey.Expires.AddDays(7) <= DateTime.UtcNow)
			{
				errorMessage = "API key is expired";
				goto NotAuthenticated;
			}

			// Delete key from db to prevent further use and return not found to client to prevent key enumeration attacks
			UpdateApiKeyDbm updateModel = DtoMapper.Map<ApiKeyDbm, UpdateApiKeyDbm>(dbKey);
			updateModel.IsActive = false;

			_ = logRepo.Create(new CreateAuditLogDbo
			{
				TableName = DbHelpers.TblApiKeys,
				ActionType = AuditActionType.Edit,
				RowId = dbKey.Id,
				UserId = dbKey.UserId,
				Comment = "Marked key as inactive"
			});

			await keyRepo.Update(updateModel);
			errorMessage = "API key not found";
			goto NotAuthenticated;
		}

		// Ensure key has required permissions flags
		if ((dbKey.Permissions & permissions) != permissions)
		{
			_ = logRepo.Create(new CreateAuditLogDbo
			{
				TableName = DbHelpers.TblApiKeys,
				ActionType = AuditActionType.Edit,
				RowId = dbKey.Id,
				UserId = dbKey.UserId,
				Comment = $"Attempted to use unprivileged API key for perms req {permissions}"
			});
			errorMessage = "Insufficient permissions";
			goto NotAllowed;
		}

		// Key has required permissions, allow execution to proceed after logging key usage
		_ = logRepo.Create(new CreateAuditLogDbo
		{
			TableName = DbHelpers.TblApiKeys,
			ActionType = AuditActionType.Access,
			RowId = dbKey.Id,
			UserId = dbKey.UserId
		});

		await next();
		return; // Prevent execution from continuing to fail states

		NotAuthenticated:
		context.Result = new ContentResult
		{
			StatusCode = StatusCodes.Status401Unauthorized,
			Content = errorMessage
		};

		return;

		NotAllowed:
		context.Result = new ContentResult
		{
			StatusCode = StatusCodes.Status403Forbidden,
			Content = errorMessage
		};
	}

}
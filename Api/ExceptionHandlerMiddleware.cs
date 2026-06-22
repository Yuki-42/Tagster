using Api.Dto;
using Npgsql;

namespace Api;

/// <summary>
/// Exception handling middleware.
/// </summary>
public class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger) : IMiddleware
{
	/// <inheritdoc />
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		try
		{
			await next(context);
		}
		catch (Exception ex)
		{
			await HandleException(context, ex);
		}
	}

	private Task HandleException(HttpContext context, Exception ex)
	{
		int status;
		string message;

		switch (ex)
		{
			case NpgsqlException:
				status = StatusCodes.Status503ServiceUnavailable;
				message = "DB Errored during request.";

				logger.LogCritical("DB Error\n{Ex}", ex.ToString());
				break;
			case MapperException:
				status = StatusCodes.Status500InternalServerError;
				message = "Mapper errored during conversion.";

				logger.LogCritical("Mapper Error\n{Ex}", ex.ToString());
				break;
			default:
				status = StatusCodes.Status500InternalServerError;
				message = "Unhandled server exception. Logged.";

				logger.LogError("Unhandled exception\n{Ex}", ex.ToString());
				break;
		}

		context.Response.Clear();
		context.Response.StatusCode = status;
		context.Response.ContentType = "application/json";

		return context.Response.WriteAsJsonAsync(message);
	}
}
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Api.Filters;

/// <summary>
///
/// </summary>
public class RequireApiKeyOperationFilter : IOperationFilter
{
	private const string SchemeName = "ApiKey";

	/// <inheritdoc />
	public void Apply(OpenApiOperation operation, OperationFilterContext context)
	{
		// Ensure that the method has the API Key attribute
		if (context.MethodInfo.GetCustomAttributes(typeof(ApiKeyAttribute), false).Length == 0) return;

		operation.Security ??= new List<OpenApiSecurityRequirement>();

		OpenApiSecurityRequirement requirement = new()
		{
			[new OpenApiSecuritySchemeReference ("ApiKey")] = []
		};

		operation.Security.Add(requirement);
	}
}
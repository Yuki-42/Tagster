using System.Security.Cryptography;
using Api;
using Api.Db;
using Api.Db.Repos;
using Api.Db.Repos.Impml;
using EchoLib.Configuration;
using Microsoft.OpenApi;

// Configure dapper
Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Build config (in multiple steps for debugging)
IConfiguration configProvider = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddIniFile("secrets.ini").Build();
Config config = ConfigBuilder.Build<Config>(configProvider);
builder.Services.AddSingleton(config);

// Check if the path specified for RSA private key is present, if not, generate one
FileInfo keyFile = new(config.AuthRequirements.RsaPrivateKeyPath);
if (!keyFile.Exists) File.WriteAllText(keyFile.FullName, RSA.Create().ExportRSAPrivateKeyPem());

// Add controllers
builder.Services.AddControllers();
builder.Services.AddScoped<ExceptionHandlerMiddleware>();

// Add database
builder.Services.AddScoped<IDbConnectionProvider, PgDbConnectionProvider>();
builder.Services.AddScoped<IAuditLogRepo, PgAuditLogRepo>();
builder.Services.AddScoped<IApiKeyRepo, PgApiKeyRepo>();
builder.Services.AddScoped<IUserRepo, PgUserRepo>();
builder.Services.AddScoped<IMediaRepo, PgMediaRepo>();
builder.Services.AddScoped<ITagRepo, PgTagRepo>();

// Add api explorer and swagger ect
string docsFile = Path.Combine(AppContext.BaseDirectory, "Api.xml");

/*
builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tagster API", Version = "v1" });
	options.IncludeXmlComments(docsFile);

	options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
	{
		Name = "X-Api-Key",
		Description = "Enter API key in header `X-Api-Key`",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey
	});

	options.AddSecurityRequirement(o =>
	{

		return new OpenApiSecurityRequirement
		{
			{ new OpenApiSecuritySchemeReference("ApiKey"), [] }
		};
	});
});
*/

builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
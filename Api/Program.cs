using System.Security.Cryptography;
using Api;
using Api.Db;
using Api.Db.Repos;
using Api.Db.Repos.Impml;
using EchoLib.Configuration;
using Microsoft.OpenApi;
using OtpNet;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Build config (in multiple steps for debugging)
IConfiguration configProvider = new ConfigurationBuilder().AddJsonFile("appsettings.json").AddIniFile("secrets.ini").Build();
Config config = ConfigBuilder.Build<Config>(configProvider);
builder.Services.AddSingleton(config);

// Check if the path specified for RSA private key is present, if not, generate one
FileInfo keyFile = new(config.AuthRequirements.RsaPrivateKeyPath);
if (!keyFile.Exists)
{
	File.WriteAllText(keyFile.FullName, RSA.Create().ExportRSAPrivateKeyPem());
}

// Add controllers
builder.Services.AddControllers();

// Add database
builder.Services.AddScoped<IDbConnectionProvider, PgDbConnectionProvider>();
builder.Services.AddScoped<IAuditLogsRepo, PgAuditLogsRepo>();
builder.Services.AddScoped<IApiKeyRepo, PgApiKeyRepo>();
builder.Services.AddScoped<IUsersRepo, PgUsersRepo>();

// Add api explorer and swagger ect
string docsFile = Path.Combine(AppContext.BaseDirectory, "Api.xml");

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo { Title = "Tagster API", Version = "v1" });
	options.IncludeXmlComments(docsFile);

	options.AddSecurityDefinition("key", new OpenApiSecurityScheme
	{
		Description = "Api key",
		Name = "key",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey
	});

	// TODO: fix why the fuck the security requirements aren't working ffs
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
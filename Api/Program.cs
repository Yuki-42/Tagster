using Api;
using Api.Db;
using Api.Db.Repos;
using Api.Db.Repos.Impml;
using EchoLib.Configuration;
using Microsoft.OpenApi;
using OtpNet;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Build config
Config config = ConfigBuilder.Build<Config>(new ConfigurationBuilder()
	.AddJsonFile("appsettings.json")
	.AddEnvironmentVariables()
	.Build());

builder.Services.AddSingleton(config);

// Add controllers
builder.Services.AddControllers();

// Add database
builder.Services.AddScoped<IDbConnectionProvider, PgDbConnectionProvider>();
builder.Services.AddScoped<IMediaRepo, PgMediaRepo>();

// Add api explorer and swagger ect
string docsFile = Path.Combine(AppContext.BaseDirectory, "Api.xml");

builder.Services.AddSwaggerGen(options =>
{
	options.SwaggerDoc("v1", new OpenApiInfo {Title = "Tagster API", Version = "v1"});
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
};

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

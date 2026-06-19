using System.Text.Json;
using EchoLib.Configuration.Attributes;

namespace Api;

/// <summary>
/// Application configuration information container.
/// </summary>
public class Config
{
	public static JsonSerializerOptions JsonDefaultOptions { get; } = new(JsonSerializerDefaults.Web);
	
	/// <summary>
	/// Explicit JSON serializer settings for absolute minified text.
	/// </summary>
	public static JsonSerializerOptions JsonMinOptions{get; } = new ()
	{
		WriteIndented = false,
		AllowTrailingCommas = false
	};
	
	[ConfigProperty]
	public DatabaseModel Database { get; init; } = null!;
	
	[ConfigProperty]
	public AuthRequirementsModel AuthRequirements { get; init; } = null!;
	
	#region ConfigModels

	[ConfigModel]
	public class AuthRequirementsModel
	{
		public required int PasswordLength { get; init; }
		[ConfigSecret] public required string OtpSecret { get; init; }
		public required int SessionLifeDays { get; init; }
	}

	[ConfigModel]
	public class DatabaseModel
	{
		public required string Host { get; init; }
		public required int Port { get; init; }
		public required string Name { get; init; }

		public required Dictionary<string, CredentialSet> Credentials { get; init; }
	}

	[ConfigModel]
	public abstract class CredentialSet
	{
		public required string Username { get; init; }
		[ConfigSecret] public required string Password { get; init; }
	}

	#endregion
}
using System.Text.Json;
using EchoLib.Configuration.Attributes;
// ReSharper disable ClassNeverInstantiated.Global

namespace Api;

/// <summary>
/// Application configuration information container.
/// </summary>
public class Config
{
	/// <summary>
	/// Default JSON serializer settings.
	/// </summary>
	public static JsonSerializerOptions JsonDefaultOptions { get; } = new(JsonSerializerDefaults.Web);
	
	/// <summary>
	/// Explicit JSON serializer settings for absolute minified text.
	/// </summary>
	public static JsonSerializerOptions JsonMinOptions{get; } = new ()
	{
		WriteIndented = false,
		AllowTrailingCommas = false
	};
	
	/// <summary>
	/// Database configuration information.
	/// </summary>
	[ConfigProperty]
	public DatabaseModel Database { get; init; } = null!;
	
	/// <summary>
	/// Authentication and crypto related configuration information.
	/// </summary>
	[ConfigProperty]
	public AuthRequirementsModel AuthRequirements { get; init; } = null!;
	
	#region ConfigModels

	/// <summary>
	/// Authentication and crypto related configuration model.
	/// </summary>
	[ConfigModel]
	public class AuthRequirementsModel
	{
		/// <summary>
		/// Minimum account password length. 
		/// </summary>
		public required int PasswordLength { get; init; }
		/// <summary>
		/// Server owner/administrator OTP secret.
		/// </summary>
		[ConfigSecret] public required string OtpSecret { get; init; }
		/// <summary>
		/// Length of API key lifespan before re-authentication is required.
		/// </summary>
		public required int SessionLifeDays { get; init; }
	}

	/// <summary>
	/// Database configuration model.
	/// </summary>
	[ConfigModel]
	public class DatabaseModel
	{
		/// <summary>
		/// Database IP/resolvable hostname.
		/// </summary>
		public required string Host { get; init; }
		/// <summary>
		/// Database server port.
		/// </summary>
		public required int Port { get; init; }
		/// <summary>
		/// Database name. This is the software-defined datastore within your DB engine and not the network information
		/// about the host machine.
		/// </summary>
		public required string Name { get; init; }

		/// <summary>
		/// Database credentials set. Dict key is the name of the purpose the credentials pair is intended to be used for.
		/// e.g. Main is for general API operations, whereas Audit is used for privileged actions such as editing existing
		/// records within the Audit Logs table.  
		/// </summary>
		public required Dictionary<string, CredentialSet> Credentials { get; init; }
	}

	/// <summary>
	/// Specific credentials pair for database server. Tied to specific use case.
	/// </summary>
	[ConfigModel]
	public class CredentialSet
	{
		/// <summary>
		/// Account/role username.
		/// </summary>
		public required string Username { get; init; }
		/// <summary>
		/// Account/role password.
		/// </summary>
		[ConfigSecret] public required string Password { get; init; }
	}

	#endregion
}
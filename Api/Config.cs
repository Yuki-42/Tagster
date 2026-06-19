using EchoLib.Configuration.Attributes;

namespace Api;

public class Config
{
	[ConfigProperty]
	public DatabaseModel Database { get; init; } = null!;

	#region ConfigModels

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
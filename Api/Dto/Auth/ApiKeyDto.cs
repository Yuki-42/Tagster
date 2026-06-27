using System.Text;
using System.Text.Json;

namespace Api.Dto.Auth;

/// <summary>
/// Exposed API key components.
/// </summary>
public class ApiKeyDto
{
	/// <summary>
	/// The API key signature.
	/// </summary>
	public required string Signature { get; init; }

	#region Presign Encoded Properties

	/// <summary>
	/// Timestamp of API key issuance.
	/// </summary>
	public required DateTime Issued { get; init; }

	/// <summary>
	/// API key expiry timestamp.
	/// </summary>
	public required DateTime Expires { get; init; }

	#endregion

	/// <summary>
	/// User-defined alias for the key. 
	/// </summary>
	public string? FriendlyName { get; init; }

	/// <summary>
	/// Decodes an *incomplete* key DTO from the encoded key model.
	/// </summary>
	/// <param name="key">Encoded key string.</param>
	/// <returns>Key model.</returns>
	public static ApiKeyDto? FromString(string key)
	{
		return JsonSerializer.Deserialize<ApiKeyDto>(
			Encoding.UTF8.GetString(
				Convert.FromBase64String(key)
				)
			);
	}

	/// <summary>
	/// Converts key model to frontend compatible encoded key string. 
	/// </summary>
	/// <returns>Encoded key string.</returns>
	public override string ToString()
	{
		return Convert.ToBase64String(
			Encoding.UTF8.GetBytes(
				JsonSerializer.Serialize(new { Signature, Issued, Expires }, Config.JsonMinOptions)
			)
		);
	}
}
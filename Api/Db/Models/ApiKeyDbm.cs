namespace Api.Db.Models;


/// <summary>
/// DBM for API key.
/// </summary>
public class ApiKeyDbm : BaseDbm
{
	/// <summary>
	/// Signed key data issued to user as identifier.
	/// </summary>
	public required string KeyValue { get; init; }

	// Encoded properties are properties of the API key that are included in the json string that is signed and issued.
	#region Encoded Properties

	/// <summary>
	/// ID of the user this API key belongs to.
	/// </summary>
	public required Guid UserId { get; init; }

	/// <summary>
	/// Timestamp this key was issued at.
	/// </summary>
	public required DateTime Issued { get; init; }

	/// <summary>
	/// Timestamp this key expires.
	/// </summary>
	public required DateTime Expires { get; init; }

	/// <summary>
	/// User agent this key is tied to.
	/// </summary>
	public required string UserAgent { get; init; }

	/// <summary>
	/// Api permissions flags.
	/// </summary>
	public required ApiKeyPermissions Permissions { get; set; }

	#endregion

	/// <summary>
	/// Friendly name for this API key used in the frontend for key management.
	/// </summary>
	public string? FriendlyName { get; init; }
}

/// <summary>
/// Permissions for actions a provided API key can be used to perform.
/// </summary>
[Flags]
public enum ApiKeyPermissions
{
	/// <summary>
	/// Key can be used to upload individual files.
	/// </summary>
	UploadFile,

	/// <summary>
	/// Key can be used to edit existing files.
	/// </summary>
	EditFile,

	/// <summary>
	/// Key can be used to share files.
	/// </summary>
	ShareFile,

	/// <summary>
	/// Key can be used to create other API keys for other purposes. Should not be given to applications without good reason.
	/// </summary>
	ManageApiKeys,

	/// <summary>
	/// Key can be used to delete files from disk. DANGEROUS.
	/// </summary>
	DeleteFile,
}
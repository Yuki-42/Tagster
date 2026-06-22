using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Api.Dto;
using Api.Dto.Auth;

namespace Api.Db.Models;

/// <summary>
/// DBM for API key.
/// </summary>
[MappedObject(typeof(ApiKeyDbm), typeof(UpdateApiKeyDbm))]
public class ApiKeyDbm : BaseDbm
{
	/// <summary>
	/// Signed key data issued to user as identifier.
	/// </summary>
	public required string KeyValue { get; init; }

	/// <summary>
	/// ID of the user this API key belongs to.
	/// </summary>
	public required Guid UserId { get; init; }

	/// <summary>
	/// Signature of
	/// </summary>
	public required string Signature { get; init; }

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
	/// IP Address associated with this API key.
	/// </summary>
	public required string IpAddress { get; init; }

	/// <summary>
	/// Api permissions flags.
	/// </summary>
	public required ApiKeyPermissions Permissions { get; init; }

	/// <summary>
	/// Friendly name for this API key used in the frontend for key management.
	/// </summary>
	public string? FriendlyName { get; init; }

	/// <summary>
	/// If the API key is considered "active". Inactive keys are not deleted from the database but are ignored in
	/// authentication flows.
	/// </summary>
	/// <remarks>
	/// This was done to ensure relational information about the key is retained for potential use in security auditing
	/// and breach investigations. Deleting the key would remove this information and make it more difficult to
	/// investigate potential breaches.
	///
	/// Yes this is overkill for a fucking photo sharing app. Do I care? No.
	/// </remarks>
	public required bool IsActive { get; set; }
}

/// <summary>
/// Model used for adding API Keys to the database.
/// </summary>
public class InsertApiKeyDbm
{
	/// <inheritdoc cref="ApiKeyDbm"/>
	public required string KeyValue { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public required Guid UserId { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public required DateTime Issued { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public required DateTime Expires { get; init; }

	/// <inheritdoc cref="ApiKeyDbm" />
	public required ApiKeyPermissions Permissions { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public required string UserAgent { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public required string IpAddress { get; init; }

	/// <inheritdoc cref="ApiKeyDbm"/>
	public string? FriendlyName { get; init; }
}

/// <summary>
/// Model used for editing existing API keys.
/// </summary>
public class UpdateApiKeyDbm
{
	/// <summary>
	/// Id of the key to edit.
	/// </summary>
	public required Guid Id { get; init; }

	/// <summary>
	/// Friendly name for the key.
	/// </summary>
	public string? FriendlyName { get; set; }

	/// <summary>
	/// If the key is active or not.
	/// </summary>
	public bool? IsActive { get; set; }
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

	/// <summary>
	/// Admin role. Supersedes any other role checks. 
	/// </summary>
	Admin
}
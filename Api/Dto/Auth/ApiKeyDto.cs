namespace Api.Dto.Auth;

/// <summary>
/// Exposed API key components.
/// </summary>
public class ApiKeyDto
{
    public required string KeyValue {get; init; }
    
    public required Guid UserId { get; init; }
    public required DateTime Issued { get; init; }
    public required DateTime Expires { get; init; }
    public string? FriendlyName {get; init; }
}
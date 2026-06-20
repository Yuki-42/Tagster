namespace Api.Dto.Auth;

/// <summary>
/// DTO for login (Create Session).
/// </summary>
public class CreateSessionDto
{
    /// <summary>
    /// Email address associated with intended account.
    /// </summary>
    public required string Email {get; init; }
    
    /// <summary>
    /// Password for associated account.
    /// </summary>
    public required string Password { get; init; }
    
    /// <summary>
    /// Friendly name for the key/session.
    /// </summary>
    public string? SessionName {get; init;}
}
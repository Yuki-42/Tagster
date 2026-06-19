using System.ComponentModel.DataAnnotations;

namespace Api.Dto.Auth;

/// <summary>
/// DTO for user signup.
/// </summary>
public class SignupDto
{
    /// <summary>
    /// Username for the account. Non-unique, I see no reason for it to be made unique. Only serves aesthetic purposes.
    /// </summary>
    [Required] public string Username { get; init; } = null!;
    
    /// <summary>
    /// Account email. Used for account recovery among other things.
    /// </summary>
    [Required, EmailAddress] public string Email { get; init; } = null!;
    
    /// <summary>
    /// Plaintext password for account. TODO: Make this hashed and salted on the client side in future. For now SSL is enough.
    /// </summary>
    [Required] public string Password { get; init; } = null!;
    
    /// <summary>
    /// OTP Code required to verify server owner is creating the account. This is required to prevent abuse of the
    /// signup endpoint, which is currently unprotected. In the future, this may be replaced with a more robust
    /// authentication mechanism for account creation, but for now it serves as a simple barrier to prevent
    /// unauthorized account creation.
    /// </summary>
    [Required] public string OwnerOtp {get; init;} = null!;
}
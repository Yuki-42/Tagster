using System.ComponentModel.DataAnnotations;

namespace Api.Dto.Auth;

public class UserDto
{
	[Required] public Guid Id { get; set; } = Guid.Empty!;
	[Required] public string Username { get; set; } = null!;
	[Required] [EmailAddress] public string Email { get; set; } = null!;
}
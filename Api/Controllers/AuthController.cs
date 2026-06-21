using System.Buffers.Text;
using System.Text;
using System.Text.Json;
using Api.Db.Models;
using Api.Db.Repos;
using Api.Dto;
using Api.Dto.Auth;
using Api.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OtpNet;

namespace Api.Controllers;

/// <summary>
/// Controls user authentication. 
/// </summary>
[ApiController]
public class AuthController(Config diConfig, IUsersRepo diUsersRepo, IApiKeyRepo diApiKeysRepo) : ControllerBase
{
	private Config _config = diConfig;
	private IUsersRepo _users = diUsersRepo;
	private IApiKeyRepo _apiKeys = diApiKeysRepo;

	private Totp _totp = new(Base32Encoding.ToBytes(diConfig.AuthRequirements.OtpSecret));

	/// <summary>
	/// Allows a user to sign up for a media account. Currently only included to allow server owner to
	/// create an account, but may be expanded in the future to allow anyone to create an account. For now,
	/// this endpoint is not protected, but in the future it may require an admin token or similar to prevent abuse.
	/// </summary>
	[HttpPost("/signup")]
	public async Task<ActionResult<UserDto>> Signup([FromBody] SignupDto dto)
	{
		// First perform config bound password checks
		if (dto.Password.Length < _config.AuthRequirements.PasswordLength)
			return BadRequest($"Password must be at least {_config.AuthRequirements.PasswordLength} characters long.");

		// Check submitted OTP against config bound OTP secret
		if (!_totp.VerifyTotp(dto.OwnerOtp, out _))
			return BadRequest("Invalid OTP.");

		// Check if the email is already in use
		if (await _users.Get(dto.Email) is not null)
			return BadRequest("Email already in use.");

		// We've verified that the user attempting to create the account is the server-owner
		// We can safely create the account in the DB now
		PasswordHasher<string> hasher = new(
			new OptionsWrapper<PasswordHasherOptions>(
				new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3 }
			)
		);

		string hashed = hasher.HashPassword(dto.Email, dto.Password);

		UserDbm newUser;
		try
		{
			newUser = await _users.Insert(new InsertUserDbm
			{
				Email = dto.Email,
				Username = dto.Username,
				Password = hashed
			});
		}
		catch (Exception e)
		{
			// Something has gone wrong in the database, return server error
			return StatusCode(500, "An error occurred while creating the account.");
		}

		return Ok(newUser);
	}

	/// <summary>
	/// Create a new session-associated API key. (i.e. log in). 
	/// </summary>
	/// <param name="dto">The login credentials.</param>
	/// <returns>The created session token.</returns>
	[HttpPost("/session-new")]
	[HttpPost("/login")]
	[NotAuthenticated]
	public async Task<ActionResult<string>> CreateSession([FromBody] CreateSessionDto dto)
	{
		// First, find the user by email
		UserDbm? user = await _users.Get(dto.Email);
		if (user is null)
			return BadRequest("Invalid email or password.");

		// Next, verify the password
		PasswordHasher<string> hasher = new(
			new OptionsWrapper<PasswordHasherOptions>(
				new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3 }
			)
		);

		PasswordVerificationResult result = hasher.VerifyHashedPassword(dto.Email, user.Password, dto.Password);
		if (result == PasswordVerificationResult.Failed)
			return BadRequest("Invalid email or password.");

		// If we get here, the credentials are valid. Create a new API key for the user and return it.
		ApiKeyDbm apiKey;
		try
		{
			apiKey = await _apiKeys.Insert(new InsertApiKeyDbm
			{
				UserId = user.Id,
				Expires = DateTime.UtcNow.AddDays(_config.AuthRequirements.SessionLifeDays),
				Permissions =
					ApiKeyPermissions
						.Admin, // TODO: Something other than this. For now this can stay since we're only allowing the server owner to create accounts.
				UserAgent = HttpContext.Request.Headers.UserAgent!,
				IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress!.ToString()
			});
		}
		catch (Exception e)
		{
			// Something has gone wrong in the database, return server error
			return StatusCode(500, "An error occurred while creating the session.");
		}

		return Convert.ToBase64String(
			Encoding.UTF8.GetBytes(JsonSerializer.Serialize(DtoMapper.Map<ApiKeyDbm, ApiKeyDto>(apiKey)))
		);
	}
}
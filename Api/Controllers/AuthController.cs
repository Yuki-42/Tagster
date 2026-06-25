using System.Security.Cryptography;
using System.Text.Json;
using Api.Db.Models;
using Api.Db.Repos;
using Api.Dto;
using Api.Dto.Auth;
using Api.Filters;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Npgsql;
using OtpNet;

namespace Api.Controllers;

/// <summary>
/// Controls user authentication. 
/// </summary>
[ApiController]
public class AuthController : ControllerBase
{
	private readonly ILogger<AuthController> _logger;
	private readonly Config _config;

	// Repos
	private readonly IUserRepo _user;
	private readonly IApiKeyRepo _apiKeys;

	// Crypto
	private readonly Totp _totp;
	private readonly RSA _rsa;

	/// <summary>
	/// Initialize a new Auth Controller.
	/// </summary>
	/// <param name="logger">DI	provided.</param>
	/// <param name="config">DI	provided.</param>
	/// <param name="user">DI provided.</param>
	/// <param name="apiKeys">DI provided.</param>
	public AuthController(
		ILogger<AuthController> logger,
		Config config,
		IUserRepo user,
		IApiKeyRepo apiKeys
	)
	{
		_logger = logger;
		_config = config;
		_user = user;
		_apiKeys = apiKeys;

		_totp = new Totp(Base32Encoding.ToBytes(config.AuthRequirements.OtpSecret));

		// RSA initialization is a little more complicated
		_rsa = RSA.Create();
		_rsa.ImportFromPem(config.AuthRequirements.RsaPrivateKey);
	}

	/// <summary>
	/// Allows a user to sign up for a media account. Currently only included to allow server owner to
	/// create an account, but may be expanded in the future to allow anyone to create an account. For now,
	/// this endpoint is not protected, but in the future it may require an admin token or similar to prevent abuse.
	/// </summary>
	/// <response code="503"/>
	[HttpPost("/signup")]
	public async Task<ActionResult<UserDto>> Signup([FromBody] SignupDto dto)
	{
		// First perform config bound password checks
		if (dto.Password.Length < _config.AuthRequirements.PasswordLength)
			return BadRequest($"Password must be at least {_config.AuthRequirements.PasswordLength} characters long.");

		// Check submitted OTP against config bound OTP secret
		if (!_totp.VerifyTotp(dto.OwnerOtp, out _))
			return Unauthorized("Invalid OTP.");

		// Check if the email is already in use
		UserDbm? user = await _user.Get(dto.Email);

		if (user is not null)
			return BadRequest("Email already in use.");

		// We've verified that the user attempting to create the account is the server-owner
		// We can safely create the account in the DB now
		PasswordHasher<string> hasher = new(
			new OptionsWrapper<PasswordHasherOptions>(
				new PasswordHasherOptions { CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3 }
			)
		);

		string hashed = hasher.HashPassword(dto.Email, dto.Password);

		user = await _user.Insert(new InsertUserDbm
		{
			Email = dto.Email,
			Username = dto.Username,
			Password = hashed
		});

		return DtoMapper.Map<UserDbm, UserDto>(user);
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
		UserDbm? user = await _user.Get(dto.Email);

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

		// Build API key primitive
		DateTime issued = DateTime.Now;
		DateTime expires = DateTime.UtcNow.AddDays(_config.AuthRequirements.SessionLifeDays);

		// Calculate signature
		string signature = Convert.ToBase64String(
			_rsa.SignData(
				JsonSerializer.SerializeToUtf8Bytes(new { Issued = issued, Expires = expires }),
				HashAlgorithmName.SHA512, RSASignaturePadding.Pkcs1
			));

		// If we get here, the credentials are valid. Create a new API key for the user and return it.
		ApiKeyDbm apiKey = await _apiKeys.Insert(new InsertApiKeyDbm
			{
				Signature = signature,
				UserId = user.Id,
				Issued = issued,
				Expires = expires,
				Permissions =
					ApiKeyPermissions
						.Admin, // TODO: Something other than this. For now this can stay since we're only allowing the server owner to create accounts.
				UserAgent = HttpContext.Request.Headers.UserAgent!,
				IpAddress = HttpContext.Request.HttpContext.Connection.RemoteIpAddress!.ToString()
			});

		return DtoMapper.Map<ApiKeyDbm, ApiKeyDto>(apiKey).ToString();
	}
}
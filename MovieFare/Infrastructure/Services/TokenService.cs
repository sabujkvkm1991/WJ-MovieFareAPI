using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MovieFare.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MovieFare.Infrastructure.Services
{
	public class TokenService : ITokenService
	{
		private readonly JwtSettings _jwtSettings;

		public TokenService(IOptions<JwtSettings> jwtSettings)
		{
			_jwtSettings = jwtSettings.Value;
		}

		/// <summary>
		/// Generates a JWT token for the given username.
		/// </summary>
		/// <param name="username"></param>
		/// <returns></returns>
		public string GenerateToken(string username)
		{
			var claims = new[]
			{
				new Claim(JwtRegisteredClaimNames.Sub, username),
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				issuer: _jwtSettings.Issuer,
				audience: _jwtSettings.Audience,
				claims: claims,
				expires: DateTime.UtcNow.AddHours(1),
				signingCredentials: creds
			);

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}

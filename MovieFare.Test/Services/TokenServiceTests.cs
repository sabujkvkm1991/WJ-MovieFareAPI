using Microsoft.Extensions.Options;
using Moq;
using MovieFare.Infrastructure.Services;
using MovieFare.Infrastructure.Settings;
using System.IdentityModel.Tokens.Jwt;

namespace MovieFare.Test.Services
{
	public class TokenServiceTests
	{
		[Fact]
		public void GenerateToken_ReturnsValidJwtToken()
		{
			// Arrange
			var jwtSettings = new JwtSettings
			{
				Key = "ThisIsASecretKeyForJwtMustBeLongEnough123!",
				Issuer = "TestIssuer",
				Audience = "TestAudience"
			};

			var optionsMock = new Mock<IOptions<JwtSettings>>();
			optionsMock.Setup(o => o.Value).Returns(jwtSettings);

			var tokenService = new TokenService(optionsMock.Object);
			string testUsername = "testuser";

			// Act
			var token = tokenService.GenerateToken(testUsername);

			// Assert
			Assert.NotNull(token);
			var handler = new JwtSecurityTokenHandler();
			var jwtToken = handler.ReadJwtToken(token);
			Assert.Equal(jwtSettings.Issuer, jwtToken.Issuer);
			Assert.Equal(jwtSettings.Audience, jwtToken.Audiences.First());
			Assert.Contains(jwtToken.Claims, c => c.Type == "sub" && c.Value == testUsername);
		}
	}
}

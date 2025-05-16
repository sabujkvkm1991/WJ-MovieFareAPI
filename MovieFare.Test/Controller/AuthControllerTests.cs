using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieFare.Controllers;
using MovieFare.Infrastructure.Models;
using MovieFare.Infrastructure.Services;
using Newtonsoft.Json.Linq;

namespace MovieFare.Test.Controller
{
	public class AuthControllerTests
	{
		[Fact]
		public void Login_WithValidCredentials_ReturnsOkWithToken()
		{
			// Arrange
			var mockTokenService = new Mock<ITokenService>();
			var expectedToken = "fake-jwt-token";
			mockTokenService.Setup(service => service.GenerateToken("admin"))
							.Returns(expectedToken);

			var controller = new AuthController(mockTokenService.Object);
			var loginRequest = new LoginRequest
			{
				Username = "admin",
				Password = "password"
			};

			// Act
			var result = controller.Login(loginRequest);

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			var json = JObject.FromObject(okResult?.Value);
			string token = json["token"]?.ToString();
			Assert.Equal(expectedToken, token);
		}

		[Fact]
		public void Login_WithInvalidCredentials_ReturnsUnauthorized()
		{
			// Arrange
			var mockTokenService = new Mock<ITokenService>();
			var controller = new AuthController(mockTokenService.Object);
			var loginRequest = new LoginRequest
			{
				Username = "user",
				Password = "wrongpassword"
			};

			// Act
			var result = controller.Login(loginRequest);

			// Assert
			Assert.IsType<UnauthorizedResult>(result);
		}
	}
}

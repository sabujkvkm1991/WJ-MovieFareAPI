using Microsoft.AspNetCore.Mvc;
using MovieFare.Infrastructure.Services;
using MovieFare.Infrastructure.Models;

namespace MovieFare.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly ITokenService _tokenService;

		public AuthController(ITokenService tokenService)
		{
			_tokenService = tokenService;
		}

		[HttpPost("login")]
		public IActionResult Login([FromBody] LoginRequest request)
		{
			if (request.Username == "admin" && request.Password == "password")
			{
				var token = _tokenService.GenerateToken(request.Username);
				return Ok(new { token });
			}

			return Unauthorized();
		}
	}
}

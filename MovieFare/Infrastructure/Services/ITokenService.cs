namespace MovieFare.Infrastructure.Services
{
	public interface ITokenService
	{
		string GenerateToken(string username);
	}
}

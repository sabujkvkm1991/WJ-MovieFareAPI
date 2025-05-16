using MediatR;
using MovieFare.Application.Models;

namespace MovieFare.Application.Query
{
	public class GetAllMoviesQuery : IRequest<List<Movies>>
	{
	}
}

using MediatR;
using MovieFare.Application.Models;

namespace MovieFare.Application.Commands
{
	public class GetMovieDetailsByIdCommand : IRequest<MovieDetails>
	{
		public string MovieId { get; set; }

		public GetMovieDetailsByIdCommand(string movieId)
		{
			MovieId = movieId;
		}
	}
}

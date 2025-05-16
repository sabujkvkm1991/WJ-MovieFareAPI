using MediatR;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.Application.Commands
{
	public class GetMovieDetailsByIdCommandHandler : IRequestHandler<GetMovieDetailsByIdCommand, MovieDetails>
	{
		private readonly IMovieService _movieService;
		public GetMovieDetailsByIdCommandHandler(IMovieService movieService)
		{
			_movieService = movieService;
		}
		public async Task<MovieDetails> Handle(GetMovieDetailsByIdCommand request, CancellationToken cancellationToken)
		{
			MovieDetails? movieDetails = null;

			try
			{
				var provider = request.MovieId.ToLowerInvariant() switch
				{
					var id when id.StartsWith("cw") => "cinemaWrld",
					var id when id.StartsWith("fw") => "filmWrld",
					_ => throw new ArgumentException("Invalid movie ID format.", nameof(request.MovieId))
				};

				return movieDetails = await _movieService.GetMovieDetailsById(request.MovieId, provider);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");				
			}

			return movieDetails!;
		}
	}
}

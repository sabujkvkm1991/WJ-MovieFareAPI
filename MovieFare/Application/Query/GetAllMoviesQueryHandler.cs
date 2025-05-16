using MediatR;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.Application.Query
{
	public class GetAllMoviesQueryHandler : IRequestHandler<GetAllMoviesQuery, List<Movies>>
	{
		private readonly IMovieService _movieService;

		public GetAllMoviesQueryHandler(IMovieService movieService)
		{
			_movieService = movieService;
		}

		public async Task<List<Movies>> Handle(GetAllMoviesQuery request, CancellationToken cancellationToken)
		{
			List<Movies> movies = new List<Movies>();

			try
			{
				var cinemaWorldMovies = await _movieService.GetCinemaWorldMovies();

				if (cinemaWorldMovies != null && cinemaWorldMovies.Count > 0)
				{
					movies.AddRange(cinemaWorldMovies);
				}

				var filmWorldMovies = await _movieService.GetfilmworldMovies();

				if (filmWorldMovies != null && filmWorldMovies.Count > 0)
				{
					movies.AddRange(filmWorldMovies);
				}

				return movies.DistinctBy(m => m.Title).ToList();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Error: {ex.Message}");
			}

			return movies; 
		}
	}
}

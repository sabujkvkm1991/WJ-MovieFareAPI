using MovieFare.Application.Models;

namespace MovieFare.Application.Services
{
	public interface IMovieService
	{
		/// <summary>
		/// Get the Cinema World movie details.
		/// </summary>
		/// <returns></returns>
		Task<List<Movies>> GetCinemaWorldMovies();

		/// <summary>
		/// Get the movie details from film world API.
		/// </summary>
		/// <returns></returns>
		Task<List<Movies>> GetfilmworldMovies();

		/// <summary>
		/// Get the movie fare by ID.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		Task<MovieDetails> GetMovieDetailsById(string movieId, string provider);
	}
}

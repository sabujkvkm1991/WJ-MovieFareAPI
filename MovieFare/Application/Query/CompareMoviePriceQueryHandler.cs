using MediatR;
using MovieFare.Application.Models;
using MovieFare.Application.Services;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MovieFare.Application.Query
{
	public class CompareMoviePriceQueryHandler : IRequestHandler<CompareMoviePriceQuery, PriceComparisonResult>
	{
		private readonly IMovieService _service;
		public CompareMoviePriceQueryHandler(IMovieService service)
		{
			_service = service;
		}

		public async Task<PriceComparisonResult> Handle(CompareMoviePriceQuery request, CancellationToken cancellationToken)
		{
			PriceComparisonResult? result = null;

			try
			{
				string movieId = Regex.Replace(request.MovieId, @"\D", "");
				var cinemaWorld = await _service.GetMovieDetailsById("cw" + movieId, "cinemaWrld");
				var filmWorld = await _service.GetMovieDetailsById("fw"+ movieId, "filmWrld");
				var cinemaWorldPrice = decimal.Zero;
				var filmWorldPrice = decimal.Zero;
				var cheapest = new MovieDetails();

				if (cinemaWorld == null && filmWorld == null)
				{
					throw new InvalidOperationException("Movie details not found.");
				}

				if(cinemaWorld != null && !string.IsNullOrEmpty(cinemaWorld.ID) && filmWorld != null && !string.IsNullOrEmpty(filmWorld.ID))
				{
					if(!string.IsNullOrEmpty(cinemaWorld.Price))
					{
						cinemaWorldPrice = ParsePrice(cinemaWorld.Price);
					}

					if (!string.IsNullOrEmpty(filmWorld.Price))
					{
						filmWorldPrice = ParsePrice(filmWorld.Price);
					}

					cheapest = cinemaWorldPrice <= filmWorldPrice ? cinemaWorld : filmWorld;
					var provider = cheapest == cinemaWorld ? "Cinema World" : "Film World";

					result = ToResult(cheapest, provider);
				}
				else
				{
					if (cinemaWorld != null && !string.IsNullOrEmpty(cinemaWorld.ID))
					{
						result = ToResult(cinemaWorld, "Cinema World");
					}
					else if (filmWorld != null && !string.IsNullOrEmpty(filmWorld.ID))
					{
						result = ToResult(filmWorld, "Film World");
					}
				}				
			}
			catch (FormatException ex)
			{
				throw new InvalidOperationException("Invalid price format encountered.", ex);
			}

			return result!;
		}

		private static decimal ParsePrice(string? price)
		{
			if (decimal.TryParse(price, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
				return value;

			throw new FormatException("Invalid price format.");
		}

		private static PriceComparisonResult ToResult(MovieDetails movie, string provider) => new()
		{
			MovieId = movie.ID,
			Title = movie.Title,
			CheapestPrice = ParsePrice(movie.Price),
			Provider = provider
		};
	}
}

using MediatR;
using MovieFare.Application.Models;

namespace MovieFare.Application.Query
{
	public class CompareMoviePriceQuery : IRequest<PriceComparisonResult>
	{
		public string MovieId { get; set; }
		public CompareMoviePriceQuery(string movieId)
		{
			MovieId = movieId;
		}
	}
}

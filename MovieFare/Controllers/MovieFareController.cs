using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieFare.Application.Commands;
using MovieFare.Application.Query;

namespace MovieFare.Controllers
{
	[Authorize]
	[Route("api/[controller]")]
	[ApiController]
	public class MovieFareController : ControllerBase
	{
		private readonly IMediator _mediator;
		public MovieFareController(IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// Get the movie details.
		/// </summary>
		/// <returns></returns>
		[HttpGet("GetAllMovies")]
		public async Task<IActionResult> GetAllMoviesDetails()
		{
			return Ok(await _mediator.Send(new GetAllMoviesQuery()));
		}

		[HttpGet("GetMovieDetailsById/{movieId}")]
		public async Task<IActionResult> GetMovieDetailsById(string movieId)
		{
			var movieDetails = await _mediator.Send(new GetMovieDetailsByIdCommand(movieId));

			if (movieDetails == null)
			{
				return NotFound();
			}

			return Ok(movieDetails);
		}

		[HttpGet("compare/{movieId}")]
		public async Task<IActionResult> ComparePrice(string movieId)
		{
			var result = await _mediator.Send(new CompareMoviePriceQuery(movieId));

			if (result == null)
			{
				return NotFound();
			}

			return Ok(result);
		}
	}
}

using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MovieFare.Application.Commands;
using MovieFare.Application.Models;
using MovieFare.Application.Query;
using MovieFare.Controllers;

namespace MovieFare.Test.Controller
{
	public class MovieFareControllerTests
	{
		private readonly Mock<IMediator> _mediatorMock;
		private readonly MovieFareController _controller;

		public MovieFareControllerTests()
		{
			_mediatorMock = new Mock<IMediator>();
			_controller = new MovieFareController(_mediatorMock.Object);
		}

		[Fact]
		public async Task GetAllMoviesDetails_ReturnsOk_WithListOfMovies()
		{
			// Arrange
			var expectedMovies = new List<Movies>
			{
				new Movies { ID = "1", Title = "Movie1" },
				new Movies { ID = "2", Title = "Movie2" }
			};
			_mediatorMock
				.Setup(m => m.Send(It.IsAny<GetAllMoviesQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedMovies);

			// Act
			var result = await _controller.GetAllMoviesDetails();

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Same(expectedMovies, okResult.Value);
		}

		[Fact]
		public async Task GetMovieDetailsById_ReturnsOk_WhenMovieExists()
		{
			// Arrange
			var expectedDetail = new MovieDetails { ID = "1", Title = "Movie1" };
			_mediatorMock
				.Setup(m => m.Send(It.IsAny<GetMovieDetailsByIdCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedDetail);

			// Act
			var result = await _controller.GetMovieDetailsById("1");

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Same(expectedDetail, okResult.Value);
		}

		[Fact]
		public async Task GetMovieDetailsById_ReturnsNotFound_WhenMovieDoesNotExist()
		{
			// Arrange
			MovieDetails? movie = null;
			_mediatorMock
				.Setup(m => m.Send(It.IsAny<GetMovieDetailsByIdCommand>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((movie));

			// Act
			var result = await _controller.GetMovieDetailsById("99");

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}

		[Fact]
		public async Task ComparePrice_ReturnsOk_WithComparisonResult()
		{
			// Arrange
			var expectedComparison = new PriceComparisonResult
			{
				CheapestPrice = 10.99m,
				MovieId = "1",
				Title = "Movie1",
				Provider = "CinemaWrld"
			};

			_mediatorMock
				.Setup(m => m.Send(It.IsAny<CompareMoviePriceQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync(expectedComparison);

			// Act
			var result = await _controller.ComparePrice("1");

			// Assert
			var okResult = Assert.IsType<OkObjectResult>(result);
			Assert.Same(expectedComparison, okResult.Value);
		}

		[Fact]
		public async Task ComparePrice_ReturnsNotFound_WhenComparisonResultIsNull()
		{
			// Arrange
			_mediatorMock
				.Setup(m => m.Send(It.IsAny<CompareMoviePriceQuery>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((PriceComparisonResult)null);

			// Act
			var result = await _controller.ComparePrice("1");

			// Assert
			Assert.IsType<NotFoundResult>(result);
		}
	}
}

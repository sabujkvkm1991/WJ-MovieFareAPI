using Moq;
using MovieFare.Application.Commands;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.Test.Handlers.Commands
{
	public class GetMovieDetailsByIdCommandHandlerTests
	{
		private readonly Mock<IMovieService> _movieServiceMock;
		private readonly GetMovieDetailsByIdCommandHandler _handler;

		public GetMovieDetailsByIdCommandHandlerTests()
		{
			_movieServiceMock = new Mock<IMovieService>();
			_handler = new GetMovieDetailsByIdCommandHandler(_movieServiceMock.Object);
		}

		[Fact]
		public async Task Handle_ReturnsMovieDetails_ForCinemaWrldPrefix()
		{
			// Arrange
			var command = new GetMovieDetailsByIdCommand("cw123");
			var expected = new MovieDetails { ID = "cw123", Title = "Test Movie" };
			_movieServiceMock
				.Setup(s => s.GetMovieDetailsById("cw123", "cinemaWrld"))
				.ReturnsAsync(expected);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.Equal(expected, result);
			_movieServiceMock.Verify(s => s.GetMovieDetailsById("cw123", "cinemaWrld"), Times.Once);
		}

		[Fact]
		public async Task Handle_ReturnsMovieDetails_ForFilmWrldPrefix()
		{
			// Arrange
			var command = new GetMovieDetailsByIdCommand("fw456");
			var expected = new MovieDetails { ID = "fw456", Title = "Film Test" };
			_movieServiceMock
				.Setup(s => s.GetMovieDetailsById("fw456", "filmWrld"))
				.ReturnsAsync(expected);

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.Equal(expected, result);
			_movieServiceMock.Verify(s => s.GetMovieDetailsById("fw456", "filmWrld"), Times.Once);
		}

		[Fact]
		public async Task Handle_ReturnsNull_ForInvalidPrefix()
		{
			// Arrange
			var command = new GetMovieDetailsByIdCommand("xx789");

			// Act
			var result = await _handler.Handle(command, CancellationToken.None);

			// Assert
			Assert.Null(result);
			_movieServiceMock.Verify(s => s.GetMovieDetailsById(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
		}
	}
}

using Moq;
using MovieFare.Application.Query;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.Test.Handlers.Querys
{
    public class CompareMoviePriceQueryHandlerTests
    {
        private readonly Mock<IMovieService> _movieServiceMock;
        private readonly CompareMoviePriceQueryHandler _handler;

        public CompareMoviePriceQueryHandlerTests()
        {
            _movieServiceMock = new Mock<IMovieService>();
            _handler = new CompareMoviePriceQueryHandler(_movieServiceMock.Object);
        }

        [Fact]
        public async Task Handle_WhenNeitherMovieExists_ThrowsInvalidOperationException()
        {
            // Arrange
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((MovieDetails)null);

            var request = new CompareMoviePriceQuery("123");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(request, CancellationToken.None));

            Assert.Equal("Movie details not found.", ex.Message);
        }

        [Fact]
        public async Task Handle_WhenBothExist_CinemaCheaper_ReturnsCinemaResult()
        {
            // Arrange
            var cinema = new MovieDetails { ID = "cw123", Title = "Cinema", Price = "5.00" };
            var film   = new MovieDetails { ID = "fw123", Title = "Film",   Price = "10.00" };

            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("cw123", "cinemaWrld"))
                .ReturnsAsync(cinema);
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("fw123", "filmWrld"))
                .ReturnsAsync(film);

            var request = new CompareMoviePriceQuery("123");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("cw123",      result.MovieId);
            Assert.Equal("Cinema",     result.Title);
            Assert.Equal(5.00m,        result.CheapestPrice);
            Assert.Equal("Cinema World", result.Provider);
        }

        [Fact]
        public async Task Handle_WhenBothExist_FilmCheaper_ReturnsFilmResult()
        {
            // Arrange
            var cinema = new MovieDetails { ID = "cw123", Title = "Cinema", Price = "15.00" };
            var film   = new MovieDetails { ID = "fw123", Title = "Film",   Price = "10.00" };

            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("cw123", "cinemaWrld"))
                .ReturnsAsync(cinema);
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("fw123", "filmWrld"))
                .ReturnsAsync(film);

            var request = new CompareMoviePriceQuery("123");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("fw123",      result.MovieId);
            Assert.Equal("Film",       result.Title);
            Assert.Equal(10.00m,       result.CheapestPrice);
            Assert.Equal("Film World", result.Provider);
        }

        [Fact]
        public async Task Handle_WhenOnlyCinemaExists_ReturnsCinemaResult()
        {
            // Arrange
            var cinema = new MovieDetails { ID = "cw456", Title = "CinemaOnly", Price = "7.50" };

            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("cw456", "cinemaWrld"))
                .ReturnsAsync(cinema);
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("fw456", "filmWrld"))
                .ReturnsAsync((MovieDetails)null);

            var request = new CompareMoviePriceQuery("456");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("cw456",       result.MovieId);
            Assert.Equal("CinemaOnly",  result.Title);
            Assert.Equal(7.50m,         result.CheapestPrice);
            Assert.Equal("Cinema World", result.Provider);
        }

        [Fact]
        public async Task Handle_WhenOnlyFilmExists_ReturnsFilmResult()
        {
            // Arrange
            var film = new MovieDetails { ID = "fw789", Title = "FilmOnly", Price = "8.25" };

            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("cw789", "cinemaWrld"))
                .ReturnsAsync((MovieDetails)null);
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("fw789", "filmWrld"))
                .ReturnsAsync(film);

            var request = new CompareMoviePriceQuery("789");

            // Act
            var result = await _handler.Handle(request, CancellationToken.None);

            // Assert
            Assert.Equal("fw789",      result.MovieId);
            Assert.Equal("FilmOnly",   result.Title);
            Assert.Equal(8.25m,        result.CheapestPrice);
            Assert.Equal("Film World", result.Provider);
        }

        [Fact]
        public async Task Handle_InvalidPriceFormat_ThrowsInvalidOperationException()
        {
            // Arrange
            var cinema = new MovieDetails { ID = "cw321", Title = "BadCinema", Price = "notdecimal" };
            var film   = new MovieDetails { ID = "fw321", Title = "BadFilm",   Price = "10.00" };

            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("cw321", "cinemaWrld"))
                .ReturnsAsync(cinema);
            _movieServiceMock
                .Setup(s => s.GetMovieDetailsById("fw321", "filmWrld"))
                .ReturnsAsync(film);

            var request = new CompareMoviePriceQuery("321");

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _handler.Handle(request, CancellationToken.None));

            Assert.Equal("Invalid price format encountered.", ex.Message);
            Assert.IsType<FormatException>(ex.InnerException);
        }
    }
}

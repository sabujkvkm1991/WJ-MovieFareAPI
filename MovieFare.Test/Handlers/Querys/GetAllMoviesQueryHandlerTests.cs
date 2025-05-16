using Moq;
using MovieFare.Application.Query;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.Test.Handlers.Querys
{
    public class GetAllMoviesQueryHandlerTests
    {
        private readonly Mock<IMovieService> _movieServiceMock;
        private readonly GetAllMoviesQueryHandler _handler;

        public GetAllMoviesQueryHandlerTests()
        {
            _movieServiceMock = new Mock<IMovieService>();
            _handler = new GetAllMoviesQueryHandler(_movieServiceMock.Object);
        }

        [Fact]
        public async Task Handle_MergesDistinctMovies_WhenServicesReturnOverlap()
        {
            // Arrange
            var cinemaList = new List<Movies>
            {
                new Movies { ID = "1", Title = "A" },
                new Movies { ID = "2", Title = "B" }
            };
            var filmList = new List<Movies>
            {
                new Movies { ID = "3", Title = "B" },
                new Movies { ID = "4", Title = "C" }
            };
            _movieServiceMock
                .Setup(s => s.GetCinemaWorldMovies())
                .ReturnsAsync(cinemaList);
            _movieServiceMock
                .Setup(s => s.GetfilmworldMovies())
                .ReturnsAsync(filmList);

            var query = new GetAllMoviesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var titles = result.Select(m => m.Title).ToList();
            Assert.Equal(3, result.Count);
            Assert.Contains("A", titles);
            Assert.Contains("B", titles);
            Assert.Contains("C", titles);
        }

        [Fact]
        public async Task Handle_ReturnsEmptyList_WhenServicesReturnNullOrEmpty()
        {
            // Arrange: both services return null
            _movieServiceMock
                .Setup(s => s.GetCinemaWorldMovies())
                .ReturnsAsync((List<Movies>)null);
            _movieServiceMock
                .Setup(s => s.GetfilmworldMovies())
                .ReturnsAsync((List<Movies>)null);

            var query = new GetAllMoviesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_CatchesExceptionAndReturnsEmptyList()
        {
            // Arrange: service throws exception
            _movieServiceMock
                .Setup(s => s.GetCinemaWorldMovies())
                .ThrowsAsync(new Exception("Service failure"));

            var query = new GetAllMoviesQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
            // Optionally, verify that film service was not called due to exception
            _movieServiceMock.Verify(s => s.GetfilmworldMovies(), Times.Never);
        }
    }
}

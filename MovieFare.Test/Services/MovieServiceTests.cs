using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using MovieFare.Application.Settings;
using MovieFare.Application.Models;
using MovieFare.Application.Services;

namespace MovieFare.UnitTests.Services
{
	public class MovieServiceTests
	{
		private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
		private readonly IMemoryCache _memoryCache;
		private readonly Mock<ILogger<MovieService>> _loggerMock;
		private readonly MovieSettings _settings;
		private readonly MovieService _service;

		public MovieServiceTests()
		{
			_httpClientFactoryMock = new Mock<IHttpClientFactory>();
			_memoryCache = new MemoryCache(new MemoryCacheOptions());
			_loggerMock = new Mock<ILogger<MovieService>>();

			_settings = new MovieSettings
			{
				AccessToken = "test-token",
				CinemaWorldBaseUrl = "https://cinemaworld.test",
				FilmWorldBaseUrl = "https://filmworld.test",
				CinemaworldCacheKey = "cw",
				FilmworldCacheKey = "fw",
				CacheTimeOut = 20
			};

			_service = new MovieService(
				_httpClientFactoryMock.Object,
				_settings,
				_memoryCache,
				_loggerMock.Object);
		}

		[Fact]
		public async Task GetCinemaWorldMovies_ReturnsFromCache_WhenPrepopulated()
		{
			// Arrange: pre-populate the real cache
			var cachedList = new List<Movies> { new Movies { ID = "1", Title = "Cached Movie" } };
			_memoryCache.Set(
				_settings.CinemaworldCacheKey,
				cachedList,
				TimeSpan.FromMinutes(_settings.CacheTimeOut));

			// Act
			var result = await _service.GetCinemaWorldMovies();

			// Assert
			Assert.Same(cachedList, result);
		}

		[Fact]
		public async Task GetCinemaWorldMovies_FetchesAndThenCaches_WhenNotInCache()
		{
			// No setup of cache ? it's empty by default

			// Arrange HTTP response
			var movieResponse = new MovieResponse
			{
				Movies = new List<Movies> { new Movies { ID = "2", Title = "Fresh Movie" } }
			};
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock
			  .Protected()
			  .Setup<Task<HttpResponseMessage>>(
				"SendAsync",
				ItExpr.Is<HttpRequestMessage>(req =>
					req.Method == HttpMethod.Get &&
					req.RequestUri == new Uri($"{_settings.CinemaWorldBaseUrl}/movies")),
				ItExpr.IsAny<CancellationToken>()
			  )
			  .ReturnsAsync(new HttpResponseMessage
			  {
				  StatusCode = HttpStatusCode.OK,
				  Content = new StringContent(JsonSerializer.Serialize(movieResponse))
			  });

			var client = new HttpClient(handlerMock.Object);
			_httpClientFactoryMock
				.Setup(f => f.CreateClient(It.IsAny<string>()))
				.Returns(client);

			// Act
			var result = await _service.GetCinemaWorldMovies();

			// Assert the service returned the data
			Assert.Single(result);
			Assert.Equal("Fresh Movie", result[0].Title);

			// And now verify it was added to the real cache
			Assert.True(_memoryCache.TryGetValue(
				_settings.CinemaworldCacheKey,
				out List<Movies> cachedMovies));
			Assert.Equal("Fresh Movie", cachedMovies![0].Title);
		}

		[Fact]
		public async Task GetfilmworldMovies_ReturnsFromCache_WhenPrepopulated()
		{
			// Arrange: put a list into the real cache
			var cachedList = new List<Movies> { new Movies { ID = "3", Title = "Film Cached" } };
			_memoryCache.Set(
				_settings.FilmworldCacheKey,
				cachedList,
				TimeSpan.FromMinutes(_settings.CacheTimeOut));

			// Act
			var result = await _service.GetfilmworldMovies();

			// Assert
			Assert.Same(cachedList, result);
		}

		[Fact]
		public async Task GetfilmworldMovies_FetchesAndCaches_WhenNotInCache()
		{
			// Arrange: leave cache empty
			var movieResponse = new MovieResponse
			{
				Movies = new List<Movies> { new Movies { ID = "4", Title = "Film Fresh" } }
			};
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock.Protected()
			   .Setup<Task<HttpResponseMessage>>(
				   "SendAsync",
				   ItExpr.Is<HttpRequestMessage>(req =>
					   req.Method == HttpMethod.Get &&
					   req.RequestUri == new Uri($"{_settings.FilmWorldBaseUrl}/movies")),
				   ItExpr.IsAny<CancellationToken>()
			   )
			   .ReturnsAsync(new HttpResponseMessage
			   {
				   StatusCode = HttpStatusCode.OK,
				   Content = new StringContent(JsonSerializer.Serialize(movieResponse))
			   });

			var client = new HttpClient(handlerMock.Object);
			_httpClientFactoryMock
				.Setup(f => f.CreateClient(It.IsAny<string>()))
				.Returns(client);

			// Act
			var result = await _service.GetfilmworldMovies();

			// Assert service returns the data
			Assert.Single(result);
			Assert.Equal("Film Fresh", result[0].Title);

			// Assert it was cached
			Assert.True(_memoryCache.TryGetValue(
				_settings.FilmworldCacheKey,
				out List<Movies> cachedMovies));
			Assert.Equal("Film Fresh", cachedMovies![0].Title);
		}

		[Fact]
		public async Task GetMovieDetailsById_ReturnsFromCache_WhenPrepopulated()
		{
			// Arrange: seed the cache
			var cachedDetail = new MovieDetails { ID = "5", Title = "Details Cached",
				Actors = "Actor1, Actor2",
				Country = "USA",
				Director = "Director Name",
				Genre = "Action",
				Language = "English",
				Metascore = "85",
				Plot = "Plot summary here",
				Poster = "https://example.com/poster.jpg",
				Price = "10.00",
				Rated = "PG-13",
				Rating = "8.5",
				Runtime = "120 min",
				Released = "2023-01-01",
				Type = "movie",
				Votes = "1000",
				Year = "2023"
			};
			var cacheKey = $"cinemaWrld:5";
			_memoryCache.Set(
				cacheKey,
				cachedDetail,
				TimeSpan.FromMinutes(_settings.CacheTimeOut));

			// Act
			var result = await _service.GetMovieDetailsById("5", "cinemaWrld");

			// Assert
			Assert.Same(cachedDetail, result);
		}

		[Theory]
		[InlineData("cinemaWrld", "https://cinemaworld.test/movie/10", "10")]
		[InlineData("filmWrld", "https://filmworld.test/movie/20", "20")]
		public async Task GetMovieDetailsById_FetchesAndCaches_WhenNotInCache(
			string provider, string expectedUri, string movieId)
		{
			// Arrange: empty cache
			// Set up HTTP handler for the correct URL
			var detail = new MovieDetails { ID = movieId, Title = "Detail Title" };
			var handlerMock = new Mock<HttpMessageHandler>();
			handlerMock.Protected()
			   .Setup<Task<HttpResponseMessage>>(
				   "SendAsync",
				   ItExpr.Is<HttpRequestMessage>(req =>
					   req.Method == HttpMethod.Get &&
					   req.RequestUri == new Uri(expectedUri)),
				   ItExpr.IsAny<CancellationToken>()
			   )
			   .ReturnsAsync(new HttpResponseMessage
			   {
				   StatusCode = HttpStatusCode.OK,
				   Content = new StringContent(JsonSerializer.Serialize(detail))
			   });

			var client = new HttpClient(handlerMock.Object);
			_httpClientFactoryMock
				.Setup(f => f.CreateClient(It.IsAny<string>()))
				.Returns(client);

			// Act
			var result = await _service.GetMovieDetailsById(movieId, provider);

			// Assert service returns deserialized data
			Assert.Equal("Detail Title", result.Title);

			// Assert it was cached
			var cacheKey = $"{provider}:{movieId}";
			Assert.True(_memoryCache.TryGetValue(
				cacheKey,
				out MovieDetails cachedDetail));
			Assert.Equal(movieId, cachedDetail!.ID);
		}

		[Fact]
		public async Task GetMovieDetailsById_InvalidProvider_ThrowsArgumentException()
		{
			await Assert.ThrowsAsync<ArgumentException>(() =>
				_service.GetMovieDetailsById("1", "invalid"));
		}
	}
}

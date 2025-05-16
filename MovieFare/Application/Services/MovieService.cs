using Microsoft.Extensions.Caching.Memory;
using MovieFare.Application.Models;
using MovieFare.Application.Settings;
using Polly;
using System.Text.Json;

namespace MovieFare.Application.Services
{
	public class MovieService : IMovieService
	{
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly MovieSettings _settings;
		private readonly IMemoryCache _cache;
		private readonly ILogger<MovieService> _logger;

		public MovieService(IHttpClientFactory httpClientFactory,
			MovieSettings settings,
			IMemoryCache cache,
			ILogger<MovieService> logger)
		{
			_httpClientFactory = httpClientFactory;
			_settings = settings;
			_cache = cache;
			_logger = logger;
		}

		/// <summary>
		/// Get the movie details from Cinema World API.
		/// </summary>
		/// <returns></returns>
		public Task<List<Movies>> GetCinemaWorldMovies() =>
			GetMoviesAsync(_settings.CinemaworldCacheKey, $"{_settings.CinemaWorldBaseUrl}/movies");

		/// <summary>
		/// Get the movie details from film world API.
		/// </summary>
		/// <returns></returns>
		public Task<List<Movies>> GetfilmworldMovies() =>
			GetMoviesAsync(_settings.FilmworldCacheKey, $"{_settings.FilmWorldBaseUrl}/movies");

		/// <summary>
		/// Get the movie details from provider API.
		/// </summary>
		/// <param name="cacheKey"></param>
		/// <param name="requestUrl"></param>
		/// <returns></returns>
		private async Task<List<Movies>> GetMoviesAsync(string cacheKey, string requestUrl)
		{
			List<Movies> lstMovies = new List<Movies>();
			// Call the API to get movie details
			try
			{
				if (_cache.TryGetValue(cacheKey, out List<Movies>? cachedMovie) && cachedMovie != null)
				{
					return cachedMovie;
				}

				var req = new HttpRequestMessage(HttpMethod.Get, requestUrl);
				req.Headers.Add("x-access-token", _settings.AccessToken);

				var httpClient = _httpClientFactory.CreateClient();
				using var response = await httpClient.SendAsync(req);

				if (response.IsSuccessStatusCode)
				{
					var content = await response.Content.ReadAsStringAsync();
					var deserializedData = JsonSerializer.Deserialize<MovieResponse>(content, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (deserializedData != null)
					{
						lstMovies = deserializedData.Movies;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Something went wrong in GetMoviesAsync: {ex.Message} & StackTrace : {ex.StackTrace}");
			}

			if (lstMovies != null && lstMovies.Count() > 0)
				_cache.Set(cacheKey, lstMovies, TimeSpan.FromMinutes(_settings.CacheTimeOut));

			return lstMovies!;
		}

		/// <summary>
		/// Get the movie fare by ID.
		/// </summary>
		/// <param name="movieId"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public async Task<MovieDetails> GetMovieDetailsById(string movieId, string provider)
		{
			MovieDetails? movieDetails = null;
			string requestUrl = string.Empty;
			var cacheKey = $"{provider}:{movieId}";

			try
			{		
				if (_cache.TryGetValue(cacheKey, out MovieDetails? cachedMovie) && cachedMovie != null)
				{
					return cachedMovie;
				}

				requestUrl = provider switch
				{
					"cinemaWrld" => $"{_settings.CinemaWorldBaseUrl}/movie/{movieId}",
					"filmWrld" => $"{_settings.FilmWorldBaseUrl}/movie/{movieId}",
					_ => throw new ArgumentException("Invalid provider")
				};

				var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
				request.Headers.Add("x-access-token", _settings.AccessToken);

				var httpClient = _httpClientFactory.CreateClient();
				using var response = await httpClient.SendAsync(request, CancellationToken.None);

				if (response.IsSuccessStatusCode)
				{
					_logger.LogInformation($"Movie Details by Id {movieId} Fetched at {DateTime.UtcNow.ToString()}");
					var content = await response.Content.ReadAsStringAsync();
					var deserializedData = JsonSerializer.Deserialize<MovieDetails>(content, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});

					if (deserializedData != null)
					{
						movieDetails = deserializedData;
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogError($"Something went wrong in GetMovieDetailsById: {ex.Message} & StackTrace : {ex.StackTrace}");
				throw;
			}

			if(movieDetails != null && !string.IsNullOrEmpty(movieDetails.ID))
				_cache.Set(cacheKey, movieDetails, TimeSpan.FromMinutes(_settings.CacheTimeOut));

			return movieDetails!;
		}
	}
}

namespace MovieFare.Application.Settings
{
	public class MovieSettings
	{
		public string CinemaWorldBaseUrl { get; set; }
		public string FilmWorldBaseUrl { get; set; }
		public string AccessToken { get; set; }
		public int CacheTimeOut { get; set; } = 5;
		public string CinemaworldCacheKey { get; set; }
		public string FilmworldCacheKey { get; set; }
	}
}

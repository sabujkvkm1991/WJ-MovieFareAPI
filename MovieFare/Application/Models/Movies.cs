namespace MovieFare.Application.Models
{
	public class Movies
	{
		public string Title { get; set; }
		public string Year { get; set; }
		public string ID { get; set; }
		public string Type { get; set; }
		public string Poster { get; set; }
		
	}

	public class MovieResponse
	{
		public List<Movies> Movies { get; set; }
	}
}

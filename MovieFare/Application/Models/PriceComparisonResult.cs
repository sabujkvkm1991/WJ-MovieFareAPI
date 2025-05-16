namespace MovieFare.Application.Models
{
	public class PriceComparisonResult
	{
		public string MovieId { get; set; }
		public string Title { get; set; }
		public decimal CheapestPrice { get; set; }
		public string Provider { get; set; }
	}
}

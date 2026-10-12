namespace DataVision.Models.Statistics
{
	public class TextualStatisticsViewModel
	{
		public string MostExpensiveProduct { get; set; }
		public string CheapestProduct { get; set; }
		public string TopStockProduct { get; set; }
		public string LowestStockProduct { get; set; }
		public string LastAddedProduct { get; set; }
		public string LastAddedCustomer { get; set; }

		public string TopPaymentMethod { get; set; }
		public string TopOrderedProduct { get; set; }
		public string MinOrderedProduct { get; set; }

		public string TopCountry { get; set; }
		public string TopCity { get; set; }

		public string TopCategory { get; set; }
		public string LowestActiveCategory { get; set; }

		public string TopCustomer { get; set; }
		public string MostCompletedProduct { get; set; }
		public string TopReturnedProduct { get; set; }
	}
}
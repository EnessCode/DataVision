namespace DataVision.Models.Statistics
{
	public class GeneralStatisticsViewModel
	{
		public int TotalCategories { get; set; }
		public int TotalCustomers { get; set; }
		public int TotalProducts { get; set; }
		public int TotalOrders { get; set; }

		public int CountryCount { get; set; }
		public int CityCount { get; set; }

		public int CompletedOrders { get; set; }
		public int CancelledOrders { get; set; }

		public int TodayOrders { get; set; }
		public int CurrentMonthOrders { get; set; }

		public decimal AverageProductPrice { get; set; }
		public double AverageProductQuantity { get; set; }
	}
}

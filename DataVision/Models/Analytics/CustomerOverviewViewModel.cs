namespace DataVision.Models.Analytics
{
	public class CustomerOverviewViewModel
	{
		public int TotalCustomerCount { get; set; }
		public double AverageOrderPerCustomer { get; set; }
		public int ActiveCustomersLast3Months { get; set; }
		public int InactiveCustomersLast3Months { get; set; }
		public double ActiveCustomerPercentage { get; set; }
		public double InactiveCustomerPercentage { get; set; }
	}
}

namespace DataVision.Models.Dashboard
{
	public class DashboardKpiTargetsViewModel
	{
		public double FulfillmentRate { get; set; }
		public int CompletedOrdersCount { get; set; }
		public int TotalOrdersCount { get; set; }

		public double CriticalStockRate { get; set; }
		public int CriticalStockProductsCount { get; set; }
		public int TotalProductsCount { get; set; }

		public double CancellationRate { get; set; }
		public int CancelledOrdersCount { get; set; }
	}
}

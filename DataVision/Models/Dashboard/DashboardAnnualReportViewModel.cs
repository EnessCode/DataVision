namespace DataVision.Models.Dashboard
{
	public class DashboardAnnualReportViewModel
	{
		public decimal TotalRevenue { get; set; }
		public decimal MonthlyRevenue { get; set; }
		public decimal AverageOrderValue { get; set; }

		public List<AnnualChartItem> ChartData { get; set; } = new();
	}
	public class AnnualChartItem
	{
		public string Period { get; set; } 
		public int Completed { get; set; }
		public int Pending { get; set; }
		public int Cancelled { get; set; }
	}
}

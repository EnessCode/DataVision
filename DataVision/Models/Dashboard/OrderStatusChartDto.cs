namespace DataVision.Models.Dashboard
{
	public class OrderStatusChartViewModel
	{
		public string Title { get; set; }
		public int Percentage { get; set; }
		public string ChangeText { get; set; }
		public bool IsPositive { get; set; }
		public string Color { get; set; }
	}
}

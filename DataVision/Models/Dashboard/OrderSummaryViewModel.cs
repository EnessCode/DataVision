namespace DataVision.Models.Dashboard
{
	public class OrderSummaryViewModel
	{
		public string CustomerFullName { get; set; }
		public string ProductName { get; set; }
		public int? Quantity { get; set; }
		public string OrderTime { get; set; }
		public string PaymentMethod { get; set; }
		public string OrderStatus { get; set; }
		public string StatusBadgeClass { get; set; }
	}
}

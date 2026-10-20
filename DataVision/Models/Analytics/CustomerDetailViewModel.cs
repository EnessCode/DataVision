using DataVision.Entities;

namespace DataVision.Models.Analytics
{
	public class CustomerDetailViewModel
	{
		public Customer Customer { get; set; }
		public int TotalOrders { get; set; }
		public int CompletedOrders { get; set; }
		public int CancelledOrders { get; set; }
		public decimal TotalSpent { get; set; }
		public List<Order> RecentOrders { get; set; }
		public List<Review> Reviews { get; set; }
		public string AiAnalysisHtml { get; set; }
	}
}

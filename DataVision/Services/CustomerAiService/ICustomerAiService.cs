using DataVision.Entities;

namespace DataVision.Services.CustomerAiService
{
	public interface ICustomerAiService
	{
		Task<string> GetOrGenerateCustomerAnalysisAsync(Customer customer, List<Order> validOrders);
	}
}

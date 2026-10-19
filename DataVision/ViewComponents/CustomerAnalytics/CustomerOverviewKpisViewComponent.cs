using DataVision.Context;
using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents
{
	public class CustomerOverviewKpisViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CustomerOverviewKpisViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			int totalCustomers = await _context.Customers
				.AsNoTracking()
				.CountAsync();

			int totalOrders = await _context.Orders
				.AsNoTracking()
				.CountAsync();

			double avgOrdersPerCustomer = totalCustomers > 0
				? Math.Round((double)totalOrders / totalCustomers, 1)
				: 0;

			var maxOrderDate = await _context.Orders
				.AsNoTracking()
				.MaxAsync(o => o.OrderDate) ?? DateTime.Now;

			var threeMonthsAgo = maxOrderDate.AddMonths(-3);

			int activeCustomersLast3Months = await _context.Orders
				.AsNoTracking()
				.Where(o => o.CustomerId.HasValue && o.OrderDate.HasValue && o.OrderDate.Value >= threeMonthsAgo)
				.Select(o => o.CustomerId.Value)
				.Distinct()
				.CountAsync();

			int inactiveCustomersLast3Months = Math.Max(0, totalCustomers - activeCustomersLast3Months);

			double activePercentage = totalCustomers > 0
				? Math.Round(((double)activeCustomersLast3Months / totalCustomers) * 100, 1)
				: 0;

			double inactivePercentage = totalCustomers > 0
				? Math.Round(((double)inactiveCustomersLast3Months / totalCustomers) * 100, 1)
				: 0;

			var model = new CustomerOverviewViewModel
			{
				TotalCustomerCount = totalCustomers,
				AverageOrderPerCustomer = avgOrdersPerCustomer,
				ActiveCustomersLast3Months = activeCustomersLast3Months,
				InactiveCustomersLast3Months = inactiveCustomersLast3Months,
				ActiveCustomerPercentage = activePercentage,
				InactiveCustomerPercentage = inactivePercentage
			};

			return View(model);
		}
	}
}

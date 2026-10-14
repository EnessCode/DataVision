using DataVision.Context;
using DataVision.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents.Dashboard
{
	public class DashboardAnnualReportViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public DashboardAnnualReportViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var today = DateTime.Today;
			var startOfMonth = new DateTime(today.Year, today.Month, 1);
			var startOfNextMonth = startOfMonth.AddMonths(1);

			var totalRevenue = await _context.Orders.AsNoTracking()
				.Where(x => x.OrderStatus == "Tamamlandı" && x.Product != null && x.Quantity != null)
				.SumAsync(x => (decimal)x.Quantity.Value * x.Product.UnitPrice);

			var monthlyRevenue = await _context.Orders.AsNoTracking()
				.Where(x => x.OrderStatus == "Tamamlandı"
						 && x.OrderDate >= startOfMonth && x.OrderDate < startOfNextMonth
						 && x.Product != null && x.Quantity != null)
				.SumAsync(x => (decimal)x.Quantity.Value * x.Product.UnitPrice);

			var completedOrdersCount = await _context.Orders.AsNoTracking()
				.CountAsync(x => x.OrderStatus == "Tamamlandı");

			var averageOrderValue = completedOrdersCount > 0
				? totalRevenue / completedOrdersCount
				: 0;

			var rawChartData = await _context.Orders.AsNoTracking()
				.Where(x => x.OrderDate != null)
				.GroupBy(x => new { x.OrderDate.Value.Year, x.OrderDate.Value.Month })
				.OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
				.Select(g => new
				{
					Year = g.Key.Year,
					Month = g.Key.Month,
					Completed = g.Count(x => x.OrderStatus == "Tamamlandı"),
					Cancelled = g.Count(x => x.OrderStatus == "İptal Edildi"),
					Pending = g.Count(x => x.OrderStatus != "Tamamlandı" && x.OrderStatus != "İptal Edildi")
				})
				.ToListAsync();

			var chartData = rawChartData
				.TakeLast(12) 
				.Select(x => new AnnualChartItem
				{
					Period = $"{x.Year}-{x.Month:D2}", 
					Completed = x.Completed,
					Pending = x.Pending,
					Cancelled = x.Cancelled
				}).ToList();

			var model = new DashboardAnnualReportViewModel
			{
				TotalRevenue = Math.Round(totalRevenue, 2),
				MonthlyRevenue = Math.Round(monthlyRevenue, 2),
				AverageOrderValue = Math.Round(averageOrderValue, 2),
				ChartData = chartData
			};

			return View(model);
		}
	}
}

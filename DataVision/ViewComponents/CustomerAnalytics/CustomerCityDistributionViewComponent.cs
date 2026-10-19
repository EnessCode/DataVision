using DataVision.Context;
using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents
{
	public class CustomerCityDistributionViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CustomerCityDistributionViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			int totalOrders = await _context.Orders.AsNoTracking().CountAsync();

			var cityData = await _context.Orders
				.AsNoTracking()
				.Where(o => o.Customer != null && !string.IsNullOrEmpty(o.Customer.CustomerCity))
				.GroupBy(o => o.Customer.CustomerCity)
				.Select(g => new CityDistributionViewModel
				{
					CityName = g.Key,
					OrderCount = g.Count()
				})
				.OrderByDescending(x => x.OrderCount)
				.Take(5)
				.ToListAsync();

			foreach (var item in cityData)
			{
				item.Percentage = totalOrders > 0 ? Math.Round((double)item.OrderCount / totalOrders * 100, 1) : 0;
			}

			return View(cityData);
		}
	}
}

using DataVision.Context;
using DataVision.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents.Dashboard
{
	public class DashboardEntityCountsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public DashboardEntityCountsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var model = new DashboardEntityCountsViewModel
			{
				TotalCategories = await _context.Categories.AsNoTracking().CountAsync(),
				TotalProducts = await _context.Products.AsNoTracking().CountAsync(),
				TotalCustomers = await _context.Customers.AsNoTracking().CountAsync(),
				TotalOrders = await _context.Orders.AsNoTracking().CountAsync(),
				CountryCount = await _context.Customers.AsNoTracking()
					.Where(x => x.CustomerCountry != null)
					.Select(x => x.CustomerCountry)
					.Distinct()
					.CountAsync(),
				CityCount = await _context.Customers.AsNoTracking()
					.Where(x => x.CustomerCity != null)
					.Select(x => x.CustomerCity)
					.Distinct()
					.CountAsync()
			};

			return View(model);
		}
	}
}

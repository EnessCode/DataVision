using DataVision.Context;
using DataVision.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DataVision.ViewComponents.Dashboard
{
	public class OrdersByCountryViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public OrdersByCountryViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var countryData = await _context.Orders
				.AsNoTracking()
				.Where(o => o.Customer != null && !string.IsNullOrEmpty(o.Customer.CustomerCountry))
				.GroupBy(o => o.Customer.CustomerCountry)
				.Select(g => new
				{
					Country = g.Key,
					OrderCount = g.Count()
				})
				.OrderByDescending(x => x.OrderCount)
				.Take(5)
				.ToListAsync();

			int totalTopOrders = countryData.Sum(x => x.OrderCount);

			var model = countryData.Select(x => new CountryOrderStatViewModel
			{
				Country = x.Country,
				OrderCount = x.OrderCount,
				Percentage = totalTopOrders > 0 ? Math.Round((double)x.OrderCount / totalTopOrders * 100, 1) : 0
			}).ToList();

			return View(model);
		}
	}
}

using DataVision.Context;
using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents
{
	public class CustomerTopVipsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CustomerTopVipsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var topVips = await _context.Orders
				.AsNoTracking()
				.Where(o => o.CustomerId.HasValue)
				.GroupBy(o => new
				{
					o.CustomerId,
					Name = o.Customer != null ? o.Customer.CustomerName : "Bilinmiyor",
					Surname = o.Customer != null ? o.Customer.CustomerSurname : ""
				})
				.Select(g => new TopVipCustomerViewModel 
				{
					CustomerName = g.Key.Name + " " + g.Key.Surname,
					TotalOrderCount = g.Count(),
					LastOrderDate = g.Max(o => o.OrderDate)
				})
				.OrderByDescending(x => x.TotalOrderCount)
				.Take(5)
				.ToListAsync();

			return View(topVips);
		}
	}
}

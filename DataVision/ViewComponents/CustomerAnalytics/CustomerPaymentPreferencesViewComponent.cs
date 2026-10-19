using DataVision.Context;
using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents
{
	public class CustomerPaymentPreferencesViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CustomerPaymentPreferencesViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			int totalOrders = await _context.Orders.AsNoTracking().CountAsync();

			var prefs = await _context.Orders
				.AsNoTracking()
				.Where(o => !string.IsNullOrEmpty(o.PaymentMethod))
				.GroupBy(o => o.PaymentMethod)
				.Select(g => new PaymentPreferenceViewModel 
				{
					PaymentMethod = g.Key,
					Count = g.Count()
				})
				.OrderByDescending(x => x.Count)
				.Take(4)
				.ToListAsync();

			foreach (var p in prefs)
			{
				p.Percentage = totalOrders > 0 ? Math.Round((double)p.Count / totalOrders * 100, 1) : 0;
			}

			return View(prefs);
		}
	}
}

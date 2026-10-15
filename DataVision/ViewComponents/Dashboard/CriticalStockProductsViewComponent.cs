using DataVision.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataVision.ViewComponents.Dashboard
{
	public class CriticalStockProductsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CriticalStockProductsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var criticalProducts = await _context.Products
				.AsNoTracking()
				.Where(p => p.StockQuantity < 20)
				.OrderBy(p => p.StockQuantity)
				.Take(12)
				.ToListAsync();

			return View(criticalProducts);
		}
	}
}

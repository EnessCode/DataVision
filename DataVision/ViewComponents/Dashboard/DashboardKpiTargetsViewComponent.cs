using DataVision.Context;
using DataVision.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents.Dashboard
{
	public class DashboardKpiTargetsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public DashboardKpiTargetsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var totalOrders = await _context.Orders.CountAsync();
			var completedOrders = await _context.Orders.CountAsync(x => x.OrderStatus == "Tamamlandı");
			var cancelledOrders = await _context.Orders.CountAsync(x => x.OrderStatus == "İptal Edildi");

			var totalProducts = await _context.Products.CountAsync();
			var criticalStockProducts = await _context.Products.CountAsync(x => x.StockQuantity < 20);

			var model = new DashboardKpiTargetsViewModel
			{
				TotalOrdersCount = totalOrders,
				CompletedOrdersCount = completedOrders,
				CancelledOrdersCount = cancelledOrders,

				TotalProductsCount = totalProducts,
				CriticalStockProductsCount = criticalStockProducts,

				FulfillmentRate = totalOrders > 0 ? Math.Round((double)completedOrders / totalOrders * 100, 1) : 0,
				CancellationRate = totalOrders > 0 ? Math.Round((double)cancelledOrders / totalOrders * 100, 1) : 0,
				CriticalStockRate = totalProducts > 0 ? Math.Round((double)criticalStockProducts / totalProducts * 100, 1) : 0
			};

			return View(model);
		}
	}
}

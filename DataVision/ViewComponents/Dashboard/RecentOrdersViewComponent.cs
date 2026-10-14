using DataVision.Context;
using DataVision.Models.Dashboard;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents.Dashboard
{
	public class RecentOrdersViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public RecentOrdersViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var oneWeekAgo = DateTime.Today.AddDays(-7);

			var values = await _context.Orders.AsNoTracking()
				.Include(x => x.Product)
				.Include(x => x.Customer)
				.Where(x => x.OrderDate >= oneWeekAgo)
				.OrderByDescending(x => x.OrderDate)
				.Take(10) 
				.Select(x => new OrderSummaryViewModel
				{
					CustomerFullName = x.Customer != null ? x.Customer.CustomerName + " " + x.Customer.CustomerSurname : "Bilinmeyen Müşteri",
					ProductName = x.Product != null ? x.Product.ProductName : "Ürün Silinmiş",
					Quantity = x.Quantity ?? 0,
					OrderTime = x.OrderDate != null ? x.OrderDate.Value.ToString("dd.MM.yyyy HH:mm") : "Tarih Yok",
					PaymentMethod = x.PaymentMethod ?? "Belirtilmemiş",
					OrderStatus = x.OrderStatus ?? "İşleniyor",
					StatusBadgeClass = x.OrderStatus == "Tamamlandı" ? "badge-success" :
									   x.OrderStatus == "İptal Edildi" ? "badge-danger" : "badge-warning"
				})
				.ToListAsync();

			return View(values);
		}
	}
}

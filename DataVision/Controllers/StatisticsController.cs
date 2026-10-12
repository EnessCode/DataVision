using DataVision.Context;
using DataVision.Models.Statistics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.Controllers
{
	public class StatisticsController : Controller
	{
		private readonly DataVisionContext _context;

		public StatisticsController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var today = DateTime.Today; 
			var startOfMonth = new DateTime(today.Year, today.Month, 1);
			var startOfNextMonth = startOfMonth.AddMonths(1);

			var model = new GeneralStatisticsViewModel
			{
				TotalCategories = await _context.Categories.CountAsync(),
				TotalCustomers = await _context.Customers.CountAsync(),
				TotalProducts = await _context.Products.CountAsync(),
				TotalOrders = await _context.Orders.CountAsync(),

				CountryCount = await _context.Customers.Select(x => x.CustomerCountry).Distinct().CountAsync(),
				CityCount = await _context.Customers.Select(x => x.CustomerCity).Distinct().CountAsync(),

				CompletedOrders = await _context.Orders.CountAsync(x => x.OrderStatus == "Tamamlandı"),
				CancelledOrders = await _context.Orders.CountAsync(x => x.OrderStatus == "İptal Edildi"),

				TodayOrders = await _context.Orders.CountAsync(x => x.OrderDate >= today),
				CurrentMonthOrders = await _context.Orders.CountAsync(x => x.OrderDate >= startOfMonth && x.OrderDate < startOfNextMonth),

				AverageProductPrice = await _context.Products.AverageAsync(x => x.UnitPrice),
				AverageProductQuantity = await _context.Products.AverageAsync(x => x.StockQuantity)
			};

			return View(model);
		}

		public async Task<IActionResult> TextualStatistics()
		{
			var model = new TextualStatisticsViewModel
			{
				MostExpensiveProduct = await _context.Products.AsNoTracking()
					.OrderByDescending(x => x.UnitPrice)
					.Select(x => x.ProductName).FirstOrDefaultAsync() ?? "Belirsiz",

				CheapestProduct = await _context.Products.AsNoTracking()
					.OrderBy(x => x.UnitPrice)
					.Select(x => x.ProductName).FirstOrDefaultAsync() ?? "Belirsiz",

				TopStockProduct = await _context.Products.AsNoTracking()
					.OrderByDescending(x => x.StockQuantity)
					.Select(x => x.ProductName).FirstOrDefaultAsync() ?? "Belirsiz",

				LowestStockProduct = await _context.Products.AsNoTracking()
					.OrderBy(x => x.StockQuantity)
					.Select(x => x.ProductName).FirstOrDefaultAsync() ?? "Belirsiz",

				LastAddedProduct = await _context.Products.AsNoTracking()
					.OrderByDescending(x => x.ProductId)
					.Select(x => x.ProductName).FirstOrDefaultAsync() ?? "Belirsiz",

				LastAddedCustomer = await _context.Customers.AsNoTracking()
					.OrderByDescending(x => x.CustomerId)
					.Select(x => x.CustomerName + " " + x.CustomerSurname).FirstOrDefaultAsync() ?? "Belirsiz",


				TopPaymentMethod = await _context.Orders.AsNoTracking()
					.Where(x => x.PaymentMethod != null)
					.GroupBy(x => x.PaymentMethod)
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				TopOrderedProduct = await _context.Orders.AsNoTracking()
					.Where(x => x.Product != null)
					.GroupBy(x => x.Product.ProductName)
					.OrderByDescending(g => g.Sum(x => x.Quantity)) 
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				MinOrderedProduct = await _context.Orders.AsNoTracking()
					.Where(x => x.Product != null)
					.GroupBy(x => x.Product.ProductName)
					.OrderBy(g => g.Sum(x => x.Quantity))
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",


				TopCountry = await _context.Orders.AsNoTracking()
					.Where(x => x.Customer != null && x.Customer.CustomerCountry != null)
					.GroupBy(x => x.Customer.CustomerCountry)
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				TopCity = await _context.Orders.AsNoTracking()
					.Where(x => x.Customer != null && x.Customer.CustomerCity != null)
					.GroupBy(x => x.Customer.CustomerCity)
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				TopCategory = await _context.Orders.AsNoTracking()
					.Where(x => x.Product != null && x.Product.Category != null)
					.GroupBy(x => x.Product.Category.CategoryName)
					.OrderByDescending(g => g.Sum(x => x.Quantity))
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				LowestActiveCategory = await _context.Orders.AsNoTracking()
					.Where(x => x.Product != null && x.Product.Category != null)
					.GroupBy(x => x.Product.Category.CategoryName)
					.OrderBy(g => g.Sum(x => x.Quantity))
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",


				TopCustomer = await _context.Orders.AsNoTracking()
					.Where(x => x.Customer != null)
					.GroupBy(x => new { x.Customer.CustomerName, x.Customer.CustomerSurname })
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key.CustomerName + " " + g.Key.CustomerSurname).FirstOrDefaultAsync() ?? "Belirsiz",

				MostCompletedProduct = await _context.Orders.AsNoTracking()
					.Where(x => x.OrderStatus == "Tamamlandı" && x.Product != null)
					.GroupBy(x => x.Product.ProductName)
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz",

				TopReturnedProduct = await _context.Orders.AsNoTracking()
					.Where(x => x.OrderStatus == "İptal Edildi" && x.Product != null)
					.GroupBy(x => x.Product.ProductName)
					.OrderByDescending(g => g.Count())
					.Select(g => g.Key).FirstOrDefaultAsync() ?? "Belirsiz"
			};

			return View(model);
		}
	}
}

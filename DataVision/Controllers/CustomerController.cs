using DataVision.Context;
using DataVision.Entities;
using DataVision.Models.Analytics;
using DataVision.Services;
using DataVision.Services.CustomerAiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.Controllers
{
	public class CustomerController : Controller
	{
		private readonly DataVisionContext _context;
		private readonly ICustomerAiService _customerAiService;

		public CustomerController(DataVisionContext context, ICustomerAiService customerAiService)
		{
			_context = context;
			_customerAiService = customerAiService;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 12;
			var totalRecords = await _context.Customers.CountAsync();
			var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

			var customers = await _context.Customers
				.AsNoTracking()
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(customers);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Customer customer)
		{
			if (ModelState.IsValid)
			{
				_context.Customers.Add(customer);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(customer);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
				return NotFound();

			return View(customer);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Customer customer)
		{
			_context.Customers.Update(customer);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			var customer = await _context.Customers.FindAsync(id);
			if (customer == null)
				return NotFound();

			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Detail(int id)
		{
			var customer = await _context.Customers
				.AsNoTracking()
				.FirstOrDefaultAsync(c => c.CustomerId == id);

			if (customer == null)
				return NotFound();

			var orders = await _context.Orders
				.AsNoTracking()
				.Include(o => o.Product)
				.ThenInclude(p => p.Category)
				.Where(o => o.CustomerId == id && o.OrderDate != null)
				.OrderByDescending(o => o.OrderDate)
				.ToListAsync();

			var reviews = await _context.Reviews
				.AsNoTracking()
				.Include(r => r.Product)
				.Where(r => r.CustomerId == id)
				.OrderByDescending(r => r.ReviewDate)
				.ToListAsync();

			var validOrders = orders.Where(o => o.OrderStatus != "İptal Edildi").ToList();

			var viewModel = new CustomerDetailViewModel
			{
				Customer = customer,
				TotalOrders = orders.Count,
				CompletedOrders = orders.Count(o => o.OrderStatus == "Tamamlandı"),
				CancelledOrders = orders.Count(o => o.OrderStatus == "İptal Edildi"),
				TotalSpent = validOrders.Sum(o => (decimal)(o.Quantity ?? 1) * (o.Product?.UnitPrice ?? 0)),
				RecentOrders = orders.Take(10).ToList(),
				Reviews = reviews,
				AiAnalysisHtml = await _customerAiService.GetOrGenerateCustomerAnalysisAsync(customer, validOrders)
			};

			return View(viewModel);
		}
	}
}

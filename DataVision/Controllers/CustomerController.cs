using DataVision.Context;
using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataVision.Controllers
{
	public class CustomerController : Controller
	{
		private readonly DataVisionContext _context;

		public CustomerController(DataVisionContext context)
		{
			_context = context;
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
			{
				return NotFound();
			}

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
			{
				return NotFound();
			}
			_context.Customers.Remove(customer);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}

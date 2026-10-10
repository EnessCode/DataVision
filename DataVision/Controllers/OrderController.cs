using DataVision.Context;
using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DataVision.Controllers
{
	public class OrderController : Controller
	{
		private readonly DataVisionContext _context;

		public OrderController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 12;
			var totalRecords = await _context.Orders.CountAsync();
			var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

			var orders = await _context.Orders
				.AsNoTracking()
				.Include(o => o.Product)
				.Include(o => o.Customer)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(orders);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Order order)
		{
			if (ModelState.IsValid)
			{
				if (order.OrderDate == null)
				{
					order.OrderDate = DateTime.Now;
				}

				_context.Orders.Add(order);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			return View(order);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var order = await _context.Orders.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Order order)
		{
			ModelState.Remove("Product");
			ModelState.Remove("Customer");

			if (ModelState.IsValid)
			{
				_context.Orders.Update(order);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			return View(order);
		}

		public async Task<IActionResult> Delete(int id)
		{
			var order = await _context.Orders.FindAsync(id);
			if (order == null)
			{
				return NotFound();
			}
			_context.Orders.Remove(order);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}

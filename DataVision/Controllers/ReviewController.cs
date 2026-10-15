using DataVision.Context;
using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataVision.Controllers
{
	public class ReviewController : Controller
	{
		private readonly DataVisionContext _context;

		public ReviewController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 12;
			var totalRecords = await _context.Reviews.CountAsync();
			var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

			var reviews = await _context.Reviews
				.AsNoTracking()
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Include(r => r.Product)
				.Include(r => r.Customer)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(reviews);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Review review)
		{
			if (ModelState.IsValid)
			{
				_context.Reviews.Add(review);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			return View(review);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var review = await _context.Reviews.FindAsync(id);
			if (review == null)
			{
				return NotFound();
			}

			return View(review);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Review review)
		{
			if (ModelState.IsValid)
			{
				_context.Reviews.Update(review);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			return View(review);
		}

		public async Task<IActionResult> Delete(int id)
		{
			var review = await _context.Reviews.FindAsync(id);
			if (review == null)
			{
				return NotFound();
			}

			_context.Reviews.Remove(review);
			await _context.SaveChangesAsync();

			return RedirectToAction(nameof(Index));
		}
	}
}

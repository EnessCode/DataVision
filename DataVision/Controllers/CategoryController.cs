using DataVision.Context;
using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace DataVision.Controllers
{
	public class CategoryController : Controller
	{
		private readonly DataVisionContext _context;

		public CategoryController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var categories = await _context.Categories.AsNoTracking().ToListAsync();
			return View(categories);
		}

		[HttpGet]
		public IActionResult Create()
		{
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Category category)
		{
			if (ModelState.IsValid)
			{
				_context.Categories.Add(category);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			return View(category);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}

			return View(category);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Category category)
		{
			_context.Categories.Update(category);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category == null)
			{
				return NotFound();
			}
			_context.Categories.Remove(category);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}

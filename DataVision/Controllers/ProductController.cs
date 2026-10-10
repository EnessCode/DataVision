using DataVision.Context;
using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DataVision.Controllers
{
	public class ProductController : Controller
	{
		private readonly DataVisionContext _context;

		public ProductController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 12; 
			var totalRecords = await _context.Products.CountAsync();
			var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

			var products = await _context.Products
				.AsNoTracking()
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.Include(p => p.Category) 
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(products);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			var categories = await _context.Categories.AsNoTracking().ToListAsync();
			ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName");
			
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Product product)
		{
			if (ModelState.IsValid)
			{
				_context.Products.Add(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			var categories = await _context.Categories.AsNoTracking().ToListAsync();
			ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);

			return View(product);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}

			var categories = await _context.Categories.AsNoTracking().ToListAsync();
			ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);

			return View(product);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(Product product)
		{
			if (ModelState.IsValid)
			{
				_context.Products.Update(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			var categories = await _context.Categories.AsNoTracking().ToListAsync();
			ViewBag.Categories = new SelectList(categories, "CategoryId", "CategoryName", product.CategoryId);

			return View(product);
		}

		public async Task<IActionResult> Delete(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}
	}
}

using DataVision.Context;
using DataVision.Entities;
using DataVision.Services.MessageAiService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DataVision.Controllers
{
	public class MessageController : Controller
	{
		private readonly DataVisionContext _context;
		private readonly IMessageAiService _messageAiService;

		public MessageController(DataVisionContext context, IMessageAiService messageAiService)
		{
			_context = context;
			_messageAiService = messageAiService;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			int pageSize = 12;
			var totalRecords = await _context.Messages.CountAsync();
			var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

			var messages = await _context.Messages
				.AsNoTracking()
				.Include(m => m.Customer)
				.OrderByDescending(m => m.MessageId)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			return View(messages);
		}

		[HttpGet]
		public async Task<IActionResult> Create()
		{
			await PopulateCustomersDropdownAsync();
			return View();
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(Message message)
		{
			ModelState.Remove(nameof(message.SentimentLabel));

			if (ModelState.IsValid)
			{
				if (message.CreatedDate == default)
				{
					message.CreatedDate = DateTime.Now;
				}

				message.SentimentLabel = await _messageAiService.AnalyzeToxicityAsync(message.MessageText);

				_context.Messages.Add(message);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			await PopulateCustomersDropdownAsync(message.CustomerId);
			return View(message);
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var message = await _context.Messages.FindAsync(id);
			if (message == null)
			{
				return NotFound();
			}

			await PopulateCustomersDropdownAsync(message.CustomerId);
			return View(message);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(Message message)
		{
			ModelState.Remove(nameof(message.SentimentLabel));

			if (ModelState.IsValid)
			{
				message.SentimentLabel = await _messageAiService.AnalyzeToxicityAsync(message.MessageText);

				_context.Messages.Update(message);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}

			await PopulateCustomersDropdownAsync(message.CustomerId);
			return View(message);
		}

		public async Task<IActionResult> Delete(int id)
		{
			var message = await _context.Messages.FindAsync(id);
			if (message == null)
			{
				return NotFound();
			}

			_context.Messages.Remove(message);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private async Task PopulateCustomersDropdownAsync(object? selectedCustomer = null)
		{
			var customers = await _context.Customers
				.AsNoTracking()
				.Select(c => new
				{
					c.CustomerId,
					FullName = c.CustomerName + " " + c.CustomerSurname
				})
				.ToListAsync();

			ViewBag.Customers = new SelectList(customers, "CustomerId", "FullName", selectedCustomer);
		}
	}
}

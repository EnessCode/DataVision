using DataVision.Context;
using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataVision.ViewComponents
{
	public class CustomerCategoryTrendsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public CustomerCategoryTrendsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
			var startDate = endDate.AddMonths(-11);

			var rawData = await _context.Orders
				.AsNoTracking()
				.Where(o => o.OrderDate.HasValue && o.OrderDate.Value >= startDate && o.OrderDate.Value < endDate.AddMonths(1))
				.Where(o => o.Product != null && o.Product.Category != null)
				.GroupBy(o => new
				{
					CategoryName = o.Product.Category.CategoryName,
					Year = o.OrderDate.Value.Year,
					Month = o.OrderDate.Value.Month
				})
				.Select(g => new
				{
					g.Key.CategoryName,
					g.Key.Year,
					g.Key.Month,
					Count = g.Count()
				})
				.ToListAsync();

			var categories = rawData.Select(x => x.CategoryName).Distinct().ToList();

			var monthsList = new List<string>();
			var monthKeys = new List<(int Year, int Month)>();

			for (int i = 0; i < 12; i++)
			{
				var targetDate = startDate.AddMonths(i);
				monthsList.Add(targetDate.ToString("MMM yyyy"));
				monthKeys.Add((targetDate.Year, targetDate.Month));
			}

			var topCategories = rawData
				.GroupBy(x => x.CategoryName)
				.Select(g => new CategorySummaryDto
				{
					CategoryName = g.Key,
					TotalOrderCount = g.Sum(x => x.Count),
					ChangePercentage = Math.Round(new Random().NextDouble() * 20 - 5, 1)
				})
				.OrderByDescending(x => x.TotalOrderCount)
				.Take(3)
				.ToList();

			var seriesDict = new Dictionary<string, List<int>>();

			foreach (var cat in categories)
			{
				var monthlyCounts = new List<int>();
				foreach (var mk in monthKeys)
				{
					int count = rawData.FirstOrDefault(x => x.CategoryName == cat && x.Year == mk.Year && x.Month == mk.Month)?.Count ?? 0;
					monthlyCounts.Add(count);
				}
				seriesDict[cat] = monthlyCounts;
			}

			var model = new CustomerCategoryTrendViewModel
			{
				TopCategories = topCategories,
				Months = monthsList,
				CategorySeries = seriesDict
			};

			return View(model);
		}
	}
}

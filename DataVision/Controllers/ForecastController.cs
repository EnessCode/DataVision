using DataVision.Context;
using DataVision.Models.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace DataVision.Controllers
{
	public class ForecastController : Controller
	{
		private readonly DataVisionContext _context;

		public ForecastController(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Index()
		{
			var allMonthlyData = await _context.Orders
				.AsNoTracking()
				.Where(o => o.OrderDate.HasValue)
				.Where(o => o.OrderStatus != "İptal Edildi")
				.Where(o => o.OrderNotes == null || o.OrderNotes != "test")
				.GroupBy(o => new
				{
					o.PaymentMethod,
					Year = o.OrderDate.Value.Year,
					Month = o.OrderDate.Value.Month
				})
				.Select(g => new
				{
					PaymentMethod = g.Key.PaymentMethod,
					Year = g.Key.Year,
					Month = g.Key.Month,
					OrderCount = g.Count()
				})
				.ToListAsync();

			var activeMonths = allMonthlyData
				.GroupBy(x => new { x.Year, x.Month })
				.Where(g => g.Sum(x => x.OrderCount) > 50)
				.OrderByDescending(g => g.Key.Year)
				.ThenByDescending(g => g.Key.Month)
				.Take(24)
				.OrderBy(g => g.Key.Year)
				.ThenBy(g => g.Key.Month)
				.Select(g => g.Key)
				.ToList();

			if (!activeMonths.Any())
			{
				activeMonths = allMonthlyData
					.Select(x => new { x.Year, x.Month })
					.Distinct()
					.OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
					.Take(24)
					.OrderBy(x => x.Year).ThenBy(x => x.Month)
					.ToList();
			}

			var paymentMethods = allMonthlyData
				.Where(x => !string.IsNullOrEmpty(x.PaymentMethod))
				.Select(x => x.PaymentMethod)
				.Distinct()
				.ToList();

			var forecastList = new List<PaymentForecastViewModel>();

			var nextPeriodStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(1);
			ViewBag.NextMonth1Name = nextPeriodStart.ToString("MMMM yyyy");
			ViewBag.NextMonth2Name = nextPeriodStart.AddMonths(1).ToString("MMMM yyyy");
			ViewBag.NextMonth3Name = nextPeriodStart.AddMonths(2).ToString("MMMM yyyy");

			int monthCount = activeMonths.Count;

			foreach (var method in paymentMethods)
			{
				var timeSeries = new List<(int MonthIndex, double Count)>();

				for (int i = 0; i < monthCount; i++)
				{
					var targetMonth = activeMonths[i];
					var record = allMonthlyData.FirstOrDefault(x =>
						x.PaymentMethod == method &&
						x.Year == targetMonth.Year &&
						x.Month == targetMonth.Month);

					timeSeries.Add((i + 1, record?.OrderCount ?? 0));
				}

				double n = monthCount > 0 ? monthCount : 1;
				double sumX = timeSeries.Sum(t => t.MonthIndex);
				double sumY = timeSeries.Sum(t => t.Count);
				double sumXY = timeSeries.Sum(t => t.MonthIndex * t.Count);
				double sumX2 = timeSeries.Sum(t => t.MonthIndex * t.MonthIndex);

				double historicalAvg = Math.Round(sumY / n, 1);

				double denominator = (n * sumX2 - (sumX * sumX));
				double slope = denominator != 0 ? (n * sumXY - sumX * sumY) / denominator : 0;
				double intercept = (sumY - slope * sumX) / n;

				var last6Months = timeSeries.TakeLast(Math.Min(6, monthCount)).ToList();
				double recentAvg = last6Months.Any() ? last6Months.Average(x => x.Count) : historicalAvg;

				double rawPred1 = (slope * (n + 1) + intercept) * 0.6 + (recentAvg + slope) * 0.4;
				double rawPred2 = (slope * (n + 2) + intercept) * 0.6 + (recentAvg + slope * 2) * 0.4;
				double rawPred3 = (slope * (n + 3) + intercept) * 0.6 + (recentAvg + slope * 3) * 0.4;

				int predMonth1 = Math.Max((int)(historicalAvg * 0.2), (int)Math.Round(rawPred1));
				int predMonth2 = Math.Max((int)(historicalAvg * 0.2), (int)Math.Round(rawPred2));
				int predMonth3 = Math.Max((int)(historicalAvg * 0.2), (int)Math.Round(rawPred3));

				double pred3MonthAvg = Math.Round((predMonth1 + predMonth2 + predMonth3) / 3.0, 1);

				double changePercentage = historicalAvg > 0
					? Math.Round(((pred3MonthAvg - historicalAvg) / historicalAvg) * 100, 1)
					: 0;

				string trendStatus = changePercentage > 1.5
					? "Yükselişte"
					: (changePercentage < -1.5 ? "Düşüşte" : "Stabil");

				forecastList.Add(new PaymentForecastViewModel
				{
					PaymentMethod = method,
					TotalLast2Years = (int)sumY,
					MonthlyHistoricalAverage = historicalAvg,
					PredictedMonth1 = predMonth1,
					PredictedMonth2 = predMonth2,
					PredictedMonth3 = predMonth3,
					Predicted3MonthAverage = pred3MonthAvg,
					Predicted3MonthTotal = predMonth1 + predMonth2 + predMonth3,
					ChangePercentage = changePercentage,
					TrendStatus = trendStatus
				});
			}

			var sortedResult = forecastList.OrderByDescending(x => x.Predicted3MonthAverage).ToList();

			return View(sortedResult);
		}
	}
}

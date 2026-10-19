using DataVision.Context;
using DataVision.Models.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.ML;
using Microsoft.ML.Data;

namespace DataVision.Controllers
{
	public class LoyaltyController : Controller
	{
		private readonly DataVisionContext _context;
		private readonly IMemoryCache _cache;

		public LoyaltyController(DataVisionContext context, IMemoryCache cache)
		{
			_context = context;
			_cache = cache;
		}

		public async Task<IActionResult> Index(int page = 1)
		{
			const string cacheKey = "CustomerLoyaltySortedList";

			if (!_cache.TryGetValue(cacheKey, out List<CustomerLoyaltyViewModel> sortedResult))
			{
				var orderStats = await _context.Orders
					.AsNoTracking()
					.Where(o => o.CustomerId != null && o.OrderDate != null && o.OrderStatus != "İptal Edildi")
					.GroupBy(o => o.CustomerId.Value)
					.Select(g => new
					{
						CustomerId = g.Key,
						LastOrderDate = g.Max(o => o.OrderDate.Value),
						Frequency = g.Count(),
						Monetary = g.Sum(o => (decimal)(o.Quantity ?? 1) * (o.Product != null ? o.Product.UnitPrice : 0))
					})
					.ToListAsync();

				if (!orderStats.Any())
					return View(new List<CustomerLoyaltyViewModel>());

				var reviewStats = await _context.Reviews
					.AsNoTracking()
					.GroupBy(r => r.CustomerId)
					.Select(g => new
					{
						CustomerId = g.Key,
						AvgRating = g.Average(r => (double)r.Rating)
					})
					.ToDictionaryAsync(x => x.CustomerId, x => x.AvgRating);

				var customersDict = await _context.Customers
					.AsNoTracking()
					.Select(c => new
					{
						c.CustomerId,
						c.CustomerName,
						c.CustomerSurname,
						c.CustomerImageUrl,
						c.CustomerCity
					})
					.ToDictionaryAsync(c => c.CustomerId);

				var referenceDate = orderStats.Max(o => o.LastOrderDate);

				var rawMetrics = new List<(int CustomerId, string FullName, string ImageUrl, string City, int Recency, int Frequency, decimal Monetary, double AvgRating)>();

				foreach (var ord in orderStats)
				{
					if (!customersDict.TryGetValue(ord.CustomerId, out var cust))
						continue;

					int recency = (int)(referenceDate - ord.LastOrderDate).TotalDays;
					double avgRating = reviewStats.TryGetValue(ord.CustomerId, out var rating) ? rating : 3.0;

					rawMetrics.Add((
						cust.CustomerId,
						$"{cust.CustomerName} {cust.CustomerSurname}",
						cust.CustomerImageUrl,
						cust.CustomerCity,
						recency,
						ord.Frequency,
						ord.Monetary,
						avgRating
					));
				}

				double maxRecency = Math.Max(1, rawMetrics.Max(x => x.Recency));
				double maxFrequency = Math.Max(1, rawMetrics.Max(x => x.Frequency));
				double maxMonetary = Math.Max(1, (double)rawMetrics.Max(x => x.Monetary));

				var trainingDataList = new List<CustomerLoyaltyData>(rawMetrics.Count);
				var viewModelList = new List<CustomerLoyaltyViewModel>(rawMetrics.Count);

				foreach (var item in rawMetrics)
				{
					double rScore = (1.0 - (item.Recency / maxRecency)) * 25.0;
					double fScore = (item.Frequency / maxFrequency) * 35.0;
					double mScore = ((double)item.Monetary / maxMonetary) * 25.0;
					double revScore = (item.AvgRating / 5.0) * 15.0;

					float baseLoyaltyScore = (float)Math.Clamp(rScore + fScore + mScore + revScore, 0, 100);

					trainingDataList.Add(new CustomerLoyaltyData
					{
						CustomerId = item.CustomerId,
						Recency = item.Recency,
						Frequency = item.Frequency,
						Monetary = (float)item.Monetary,
						AverageRating = (float)item.AvgRating,
						LoyaltyScore = baseLoyaltyScore
					});

					viewModelList.Add(new CustomerLoyaltyViewModel
					{
						CustomerId = item.CustomerId,
						CustomerFullName = item.FullName,
						CustomerImageUrl = item.ImageUrl,
						CustomerCity = item.City,
						Recency = item.Recency,
						Frequency = item.Frequency,
						Monetary = item.Monetary,
						AverageRating = Math.Round(item.AvgRating, 1),
						CurrentLoyaltyScore = Math.Round(baseLoyaltyScore, 1)
					});
				}

				var mlContext = new MLContext(seed: 42);
				IDataView dataView = mlContext.Data.LoadFromEnumerable(trainingDataList);

				var pipeline = mlContext.Transforms.Concatenate("Features",
						nameof(CustomerLoyaltyData.Recency),
						nameof(CustomerLoyaltyData.Frequency),
						nameof(CustomerLoyaltyData.Monetary),
						nameof(CustomerLoyaltyData.AverageRating))
					.Append(mlContext.Transforms.NormalizeMinMax("Features"))
					.Append(mlContext.Regression.Trainers.Sdca(labelColumnName: "Label", featureColumnName: "Features", maximumNumberOfIterations: 50));

				var model = pipeline.Fit(dataView);

				IDataView predictionsDataView = model.Transform(dataView);
				float[] predictedScores = predictionsDataView.GetColumn<float>("Score").ToArray();

				for (int i = 0; i < viewModelList.Count; i++)
				{
					double predictedScore = Math.Clamp(Math.Round(predictedScores[i], 1), 0, 100);
					viewModelList[i].PredictedLoyaltyScore = predictedScore;

					if (predictedScore >= 80)
					{
						viewModelList[i].LoyaltySegment = "Şampiyon (VIP)";
						viewModelList[i].SegmentBadgeColor = "badge bg-success";
					}
					else if (predictedScore >= 60)
					{
						viewModelList[i].LoyaltySegment = "Sadık Müşteri";
						viewModelList[i].SegmentBadgeColor = "badge bg-primary";
					}
					else if (predictedScore >= 40)
					{
						viewModelList[i].LoyaltySegment = "Potansiyel / Gelişmekte";
						viewModelList[i].SegmentBadgeColor = "badge bg-info text-dark";
					}
					else if (predictedScore >= 25)
					{
						viewModelList[i].LoyaltySegment = "Risk Altında";
						viewModelList[i].SegmentBadgeColor = "badge bg-warning text-dark";
					}
					else
					{
						viewModelList[i].LoyaltySegment = "Kayıp Adayı (Churn)";
						viewModelList[i].SegmentBadgeColor = "badge bg-danger";
					}
				}

				sortedResult = viewModelList.OrderByDescending(x => x.PredictedLoyaltyScore).ToList();

				_cache.Set(cacheKey, sortedResult, TimeSpan.FromMinutes(15));
			}

			int pageSize = 10;
			int totalItems = sortedResult.Count;
			int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

			page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

			ViewBag.TotalCustomers = totalItems;
			ViewBag.VipCount = sortedResult.Count(x => x.PredictedLoyaltyScore >= 80);
			ViewBag.RiskCount = sortedResult.Count(x => x.PredictedLoyaltyScore < 40);
			ViewBag.AvgScore = totalItems > 0 ? Math.Round(sortedResult.Average(x => x.PredictedLoyaltyScore), 1) : 0;

			var segmentGroups = sortedResult.GroupBy(x => x.LoyaltySegment)
											.Select(g => new { Segment = g.Key, Count = g.Count() })
											.ToList();
			ViewBag.SegmentLabels = segmentGroups.Select(x => x.Segment).ToList();
			ViewBag.SegmentCounts = segmentGroups.Select(x => x.Count).ToList();

			var top10 = sortedResult.Take(10).ToList();
			ViewBag.Top10Names = top10.Select(x => x.CustomerFullName).ToList();
			ViewBag.Top10Scores = top10.Select(x => x.PredictedLoyaltyScore).ToList();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;

			var pagedResult = sortedResult.Skip((page - 1) * pageSize).Take(pageSize).ToList();

			return View(pagedResult);
		}
	}
}

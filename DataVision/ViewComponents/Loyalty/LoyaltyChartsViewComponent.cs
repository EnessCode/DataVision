using DataVision.Models.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DataVision.ViewComponents.Loyalty
{
	public class LoyaltyChartsViewComponent : ViewComponent
	{
		private readonly IMemoryCache _cache;

		public LoyaltyChartsViewComponent(IMemoryCache cache)
		{
			_cache = cache;
		}

		public IViewComponentResult Invoke()
		{
			_cache.TryGetValue("CustomerLoyaltySortedList", out List<CustomerLoyaltyViewModel> list);
			list ??= new List<CustomerLoyaltyViewModel>();

			var segmentGroups = list.GroupBy(x => x.LoyaltySegment)
									.Select(g => new { Segment = g.Key, Count = g.Count() })
									.ToList();

			ViewBag.SegmentLabels = segmentGroups.Select(x => x.Segment).ToList();
			ViewBag.SegmentCounts = segmentGroups.Select(x => x.Count).ToList();

			var top10 = list.Take(10).ToList();
			ViewBag.Top10Names = top10.Select(x => x.CustomerFullName).ToList();
			ViewBag.Top10Scores = top10.Select(x => x.PredictedLoyaltyScore).ToList();

			return View();
		}
	}
}

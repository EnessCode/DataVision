using DataVision.Models.ML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace DataVision.ViewComponents.Loyalty
{
	public class LoyaltyKpiViewComponent : ViewComponent
	{
		private readonly IMemoryCache _cache;

		public LoyaltyKpiViewComponent(IMemoryCache cache)
		{
			_cache = cache;
		}

		public IViewComponentResult Invoke()
		{
			_cache.TryGetValue("CustomerLoyaltySortedList", out List<CustomerLoyaltyViewModel> list);
			list ??= new List<CustomerLoyaltyViewModel>();

			ViewBag.TotalCustomers = list.Count;
			ViewBag.VipCount = list.Count(x => x.PredictedLoyaltyScore >= 80);
			ViewBag.RiskCount = list.Count(x => x.PredictedLoyaltyScore < 40);
			ViewBag.AvgScore = list.Any() ? Math.Round(list.Average(x => x.PredictedLoyaltyScore), 1) : 0;

			return View();
		}
	}
}

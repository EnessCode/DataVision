using Microsoft.AspNetCore.Mvc;

namespace DataVision.ViewComponents.CustomerAnalytics
{
	public class CustomerAnalyticsHeaderViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}

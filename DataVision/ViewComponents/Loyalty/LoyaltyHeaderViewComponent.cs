using Microsoft.AspNetCore.Mvc;

namespace DataVision.ViewComponents.Loyalty
{
	public class LoyaltyHeaderViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}

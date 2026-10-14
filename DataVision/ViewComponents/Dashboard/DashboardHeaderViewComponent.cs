using Microsoft.AspNetCore.Mvc;

namespace DataVision.ViewComponents.Dashboard
{
	public class DashboardHeaderViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke()
		{
			return View();
		}
	}
}

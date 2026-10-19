using Microsoft.AspNetCore.Mvc;

namespace DataVision.Controllers
{
	public class CustomerAnalyticsController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}

using Microsoft.AspNetCore.Mvc;

namespace DataVision.Controllers
{
	public class DashboardController : Controller
	{
		public IActionResult Index()
		{
			return View();
		}
	}
}

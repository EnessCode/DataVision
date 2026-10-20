using DataVision.Models.Analytics;
using Microsoft.AspNetCore.Mvc;

namespace DataVision.ViewComponents.CustomerDetail
{
	public class CustomerDetailStatsViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke(CustomerDetailViewModel model) => View(model);
	}
}

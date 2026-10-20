using DataVision.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DataVision.ViewComponents.CustomerDetail
{
	public class CustomerDetailBannerViewComponent : ViewComponent
	{
		public IViewComponentResult Invoke(Customer customer) => View(customer);
	}
}

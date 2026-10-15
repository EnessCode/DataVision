using DataVision.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace DataVision.ViewComponents.Dashboard
{
	public class RecentReviewsViewComponent : ViewComponent
	{
		private readonly DataVisionContext _context;

		public RecentReviewsViewComponent(DataVisionContext context)
		{
			_context = context;
		}

		public async Task<IViewComponentResult> InvokeAsync()
		{
			var recentReviews = await _context.Reviews
				.AsNoTracking()
				.Include(r => r.Customer)
				.Include(r => r.Product)
				.OrderByDescending(r => r.ReviewDate)
				.ThenByDescending(r => r.ReviewId)
				.Take(5)
				.ToListAsync();

			return View(recentReviews);
		}
	}
}

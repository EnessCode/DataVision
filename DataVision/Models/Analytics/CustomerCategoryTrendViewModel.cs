namespace DataVision.Models.Analytics
{
	public class CustomerCategoryTrendViewModel
	{
		public List<CategorySummaryDto> TopCategories { get; set; } = new();
		public List<string> Months { get; set; } = new(); 
		public Dictionary<string, List<int>> CategorySeries { get; set; } = new(); 
	}

	public class CategorySummaryDto
	{
		public string CategoryName { get; set; }
		public int TotalOrderCount { get; set; }
		public double ChangePercentage { get; set; } 
	}
}

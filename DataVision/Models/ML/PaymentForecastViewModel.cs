namespace DataVision.Models.ML
{
	public class PaymentForecastViewModel
	{
		public string PaymentMethod { get; set; }
		public int TotalLast2Years { get; set; }
		public double MonthlyHistoricalAverage { get; set; }
		public int PredictedMonth1 { get; set; }
		public int PredictedMonth2 { get; set; }
		public int PredictedMonth3 { get; set; }
		public double Predicted3MonthAverage { get; set; }
		public int Predicted3MonthTotal { get; set; }
		public double ChangePercentage { get; set; }
		public string TrendStatus { get; set; }
	}
}

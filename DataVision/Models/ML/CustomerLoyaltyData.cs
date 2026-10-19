using Microsoft.ML.Data;

namespace DataVision.Models.ML
{
	public class CustomerLoyaltyData
	{
		[LoadColumn(0)]
		public float CustomerId { get; set; }

		[LoadColumn(1)]
		public float Recency { get; set; }

		[LoadColumn(2)]
		public float Frequency { get; set; } 

		[LoadColumn(3)]
		public float Monetary { get; set; } 

		[LoadColumn(4)]
		public float AverageRating { get; set; }

		[LoadColumn(5), ColumnName("Label")]
		public float LoyaltyScore { get; set; } 
	}

	public class CustomerLoyaltyPrediction
	{
		[ColumnName("Score")]
		public float PredictedLoyaltyScore { get; set; }
	}

	public class CustomerLoyaltyViewModel
	{
		public int CustomerId { get; set; }
		public string CustomerFullName { get; set; }
		public string CustomerImageUrl { get; set; }
		public string CustomerCity { get; set; }

		public int Recency { get; set; }
		public int Frequency { get; set; }
		public decimal Monetary { get; set; }
		public double AverageRating { get; set; }

		public double CurrentLoyaltyScore { get; set; }
		public double PredictedLoyaltyScore { get; set; }
		public string LoyaltySegment { get; set; } 
		public string SegmentBadgeColor { get; set; }
	}
}
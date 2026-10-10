using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataVision.Entities
{
	public class Product
	{
		public int ProductId { get; set; }
		public string ProductName { get; set; }
		public string ProductDescription { get; set; }
		public decimal UnitPrice { get; set; }
		public short StockQuantity { get; set; }

		public int CategoryId { get; set; }
		public Category? Category { get; set; }

		public string CountryOfOrigin { get; set; }
		public string ProductImageUrl { get; set; }
	}
}

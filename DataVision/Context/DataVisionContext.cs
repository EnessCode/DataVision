using DataVision.Entities;
using Microsoft.EntityFrameworkCore;

namespace DataVision.Context
{
	public class DataVisionContext : DbContext
	{
		public DataVisionContext(DbContextOptions<DataVisionContext> options) : base(options)
		{
		}

		public DbSet<Category> Categories { get; set; }
		public DbSet<Product> Products { get; set; }
		public DbSet<Customer> Customers { get; set; }
		public DbSet<Order> Orders { get; set; }
	}
}

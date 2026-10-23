namespace DataVision.Entities
{
	public class Customer
	{
		public int CustomerId { get; set; }
		public string CustomerName { get; set; }
		public string CustomerSurname { get; set; }
		public string CustomerEmail { get; set; }
		public string CustomerPhone { get; set; }
		public string CustomerImageUrl { get; set; }
		public string CustomerCountry { get; set; }
		public string CustomerCity { get; set; }
		public string CustomerDistrict { get; set; }
		public string CustomerAddress { get; set; }
		public List<Message> Messages { get; set; }
	}
}

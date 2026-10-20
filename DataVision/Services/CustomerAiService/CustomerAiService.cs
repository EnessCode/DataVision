using DataVision.Entities;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace DataVision.Services.CustomerAiService
{
	public class CustomerAiService : ICustomerAiService
	{
		private readonly IConfiguration _configuration;
		private readonly IHttpClientFactory _httpClientFactory;
		private readonly IMemoryCache _cache;

		public CustomerAiService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IMemoryCache cache)
		{
			_configuration = configuration;
			_httpClientFactory = httpClientFactory;
			_cache = cache;
		}

		public async Task<string> GetOrGenerateCustomerAnalysisAsync(Customer customer, List<Order> validOrders)
		{
			string cacheKey = $"CustomerAiAnalysis_{customer.CustomerId}";

			if (_cache.TryGetValue(cacheKey, out string cachedHtml))
				return cachedHtml;

			string generatedHtml = await GenerateAnalysisInternalAsync(customer, validOrders);
			_cache.Set(cacheKey, generatedHtml, TimeSpan.FromMinutes(30));

			return generatedHtml;
		}

		private async Task<string> GenerateAnalysisInternalAsync(Customer customer, List<Order> validOrders)
		{
			if (!validOrders.Any())
				return "<p class='text-muted'>Bu müşteriye ait aktif sipariş verisi bulunmadığı için AI analizi oluşturulamadı.</p>";

			var orderAmounts = validOrders.Select(o => (decimal)(o.Quantity ?? 1) * (o.Product?.UnitPrice ?? 0)).ToList();
			decimal totalSpent = orderAmounts.Sum();
			decimal avgBasket = orderAmounts.Average();
			decimal maxOrder = orderAmounts.Max();
			decimal minOrder = orderAmounts.Min();

			var distinctMonths = validOrders.Select(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month }).Distinct().Count();
			double monthlyAvgOrders = Math.Round((double)validOrders.Count / Math.Max(1, distinctMonths), 1);

			var topCategories = validOrders
				.Where(o => o.Product?.Category != null)
				.GroupBy(o => o.Product.Category.CategoryName)
				.Select(g => new { Category = g.Key, Count = g.Count() })
				.OrderByDescending(x => x.Count)
				.Take(4)
				.ToList();

			var topProducts = validOrders
				.Where(o => o.Product != null)
				.GroupBy(o => o.Product.ProductName)
				.Select(g => new
				{
					Product = g.Key,
					TotalQty = g.Sum(x => x.Quantity ?? 1),
					TotalAmount = g.Sum(x => (decimal)(x.Quantity ?? 1) * x.Product.UnitPrice)
				})
				.OrderByDescending(x => x.TotalAmount)
				.Take(4)
				.ToList();

			var culture = new CultureInfo("tr-TR");
			string busiestMonth = validOrders.GroupBy(o => o.OrderDate.Value.ToString("MMMM", culture))
				.OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "-";

			string busiestDay = validOrders.GroupBy(o => o.OrderDate.Value.ToString("dd MMMM", culture))
				.OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault() ?? "-";

			int busiestHour = validOrders.GroupBy(o => o.OrderDate.Value.Hour)
				.OrderByDescending(g => g.Count()).Select(g => g.Key).FirstOrDefault();
			string favTimeRange = $"{busiestHour:00}:00 - {(busiestHour + 2) % 24:00}:00";

			string apiKey = _configuration["Gemini:ApiKey"];
			if (!string.IsNullOrWhiteSpace(apiKey) && !apiKey.Contains("SENIN_UCRETSIZ"))
			{
				try
				{
					string categoriesText = string.Join(", ", topCategories.Select(c => $"{c.Category} – {c.Count} sipariş"));
					string productsText = string.Join(", ", topProducts.Select(p => $"{p.Product} ({p.TotalQty} — {p.TotalAmount:N2})"));

					string prompt = $@"
Aşağıdaki müşteri verilerini kullanarak tam olarak belirtilen HTML yapısında Türkçe bir müşteri analizi oluştur. Kesinlikle ```html veya markdown etiketi kullanma, doğrudan saf HTML döndür.

Veriler:
Ad: {customer.CustomerName}, Soyad: {customer.CustomerSurname}, Toplam Sipariş: {validOrders.Count}, Toplam Harcama: {totalSpent:N2}
Kategoriler: {categoriesText}
Öne Çıkan Ürünler: {productsText}
En yoğun ay: {busiestMonth}, En yoğun gün: {busiestDay}, Favori saat aralığı: {favTimeRange}
Aylık ortalama sipariş: {monthlyAvgOrders}, Ortalama sepet tutarı: {avgBasket:N2}, En yüksek sipariş: {maxOrder:N2}, En düşük sipariş: {minOrder:N2}

Şu HTML şablonunu birebir doldur ve kategori isimlerinin başına uygun birer emoji ekle, en sona da 2 cümlelik '🤖 AI Pazarlama ve Kampanya Önerisi' başlığı ekle:
<h5 class='font-weight-bold mt-3'>👤 Müşteri Profili</h5>
<p>Ad: {customer.CustomerName}</p>
<p>Soyad: {customer.CustomerSurname}</p>
<p>Toplam Sipariş: {validOrders.Count}</p>
<p>Toplam Harcama: {totalSpent:N2}</p>
<h5 class='font-weight-bold mt-4'>🛍️ Ürün Tercihleri</h5>
<ul><!-- Kategorileri buraya <li> olarak uygun emojilerle ekle --></ul>
<p><strong>Öne çıkan ürünler:</strong></p>
<ul><!-- Ürünleri buraya <li> olarak ekle --></ul>
<h5 class='font-weight-bold mt-4'>⏰ Zaman Bazlı Alışveriş Davranışı</h5>
<p>En yoğun ay: {busiestMonth}</p>
<p>En yoğun gün: {busiestDay}</p>
<p>Favori saat aralığı: {favTimeRange}</p>
<h5 class='font-weight-bold mt-4'>💰 Ortalama Harcama ve Sıklık</h5>
<p>Aylık ortalama sipariş: {monthlyAvgOrders}</p>
<p>Ortalama sepet tutarı: {avgBasket:N2}</p>
<p>En yüksek sipariş: {maxOrder:N2}</p>
<p>En düşük sipariş: {minOrder:N2}</p>";

					var client = _httpClientFactory.CreateClient();
					var url = $"[https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=](https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=){apiKey}";

					var requestBody = new
					{
						contents = new[] { new { parts = new[] { new { text = prompt } } } }
					};

					var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
					var response = await client.PostAsync(url, content);

					if (response.IsSuccessStatusCode)
					{
						var responseString = await response.Content.ReadAsStringAsync();
						using var doc = JsonDocument.Parse(responseString);
						var aiText = doc.RootElement
							.GetProperty("candidates")[0]
							.GetProperty("content")
							.GetProperty("parts")[0]
							.GetProperty("text")
							.GetString();

						if (!string.IsNullOrWhiteSpace(aiText))
							return aiText.Replace("```html", "").Replace("```", "").Trim();
					}
				}
				catch
				{
				}
			}

			var sb = new StringBuilder();
			sb.AppendLine("<h5 class='font-weight-bold mt-3'>👤 Müşteri Profili</h5>");
			sb.AppendLine($"<p>Ad: {customer.CustomerName}</p>");
			sb.AppendLine($"<p>Soyad: {customer.CustomerSurname}</p>");
			sb.AppendLine($"<p>Toplam Sipariş: {validOrders.Count}</p>");
			sb.AppendLine($"<p>Toplam Harcama: {totalSpent:N2}</p>");

			sb.AppendLine("<h5 class='font-weight-bold mt-4'>🛍️ Ürün Tercihleri</h5><ul>");
			foreach (var c in topCategories)
				sb.AppendLine($"<li>📦 {c.Category} – {c.Count} sipariş</li>");
			sb.AppendLine("</ul><p><strong>Öne çıkan ürünler:</strong></p><ul>");
			foreach (var p in topProducts)
				sb.AppendLine($"<li>{p.Product} ({p.TotalQty} — {p.TotalAmount:N2})</li>");
			sb.AppendLine("</ul>");

			sb.AppendLine("<h5 class='font-weight-bold mt-4'>⏰ Zaman Bazlı Alışveriş Davranışı</h5>");
			sb.AppendLine($"<p>En yoğun ay: {busiestMonth}</p>");
			sb.AppendLine($"<p>En yoğun gün: {busiestDay}</p>");
			sb.AppendLine($"<p>Favori saat aralığı: {favTimeRange}</p>");

			sb.AppendLine("<h5 class='font-weight-bold mt-4'>💰 Ortalama Harcama ve Sıklık</h5>");
			sb.AppendLine($"<p>Aylık ortalama sipariş: {monthlyAvgOrders}</p>");
			sb.AppendLine($"<p>Ortalama sepet tutarı: {avgBasket:N2}</p>");
			sb.AppendLine($"<p>En yüksek sipariş: {maxOrder:N2}</p>");
			sb.AppendLine($"<p>En düşük sipariş: {minOrder:N2}</p>");

			return sb.ToString();
		}
	}
}

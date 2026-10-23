using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace DataVision.Services.MessageAiService
{
	public class MessageAiService : IMessageAiService
	{
		private readonly HttpClient _httpClient;
		private readonly string _apiKey;

		private static readonly string[] ModelEndpoints = new[]
		{
			"https://router.huggingface.co/hf-inference/models/unitary/toxic-bert",
			"https://api-inference.huggingface.co/models/unitary/toxic-bert",
			"https://router.huggingface.co/hf-inference/models/martin-ha/toxic-comment-model"
		};

		public MessageAiService(HttpClient httpClient, IConfiguration configuration)
		{
			_httpClient = httpClient;
			_apiKey = configuration["HuggingFace:ApiKey"]?.Trim() ?? "";
		}

		public async Task<string> AnalyzeToxicityAsync(string messageText)
		{
			if (string.IsNullOrWhiteSpace(messageText))
				return "Belirsiz";

			if (string.IsNullOrEmpty(_apiKey))
				return "API Key Eksik";

			try
			{
				string englishText = await TranslateTurkishToEnglishAsync(messageText);

				foreach (var endpoint in ModelEndpoints)
				{
					var result = await TryAnalyzeWithHuggingFaceAsync(endpoint, englishText);
					if (result != null)
					{
						return result;
					}
				}

				return "Analiz Edilemedi";
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.WriteLine($"[Toxic-BERT Exception]: {ex.Message}");
				return "Analiz Hatası";
			}
		}

		private async Task<string?> TryAnalyzeWithHuggingFaceAsync(string url, string englishText)
		{
			var requestBody = new { inputs = englishText };
			string jsonPayload = JsonSerializer.Serialize(requestBody);

			for (int attempt = 1; attempt <= 2; attempt++)
			{
				try
				{
					using var request = new HttpRequestMessage(HttpMethod.Post, url);
					request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
					request.Headers.Add("x-wait-for-model", "true");
					request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

					var response = await _httpClient.SendAsync(request);
					var responseString = await response.Content.ReadAsStringAsync();

					if ((int)response.StatusCode == 503 || responseString.Contains("currently loading"))
					{
						await Task.Delay(3000);
						continue;
					}

					if (!response.IsSuccessStatusCode)
					{
						System.Diagnostics.Debug.WriteLine($"[HF Model Hata - {url}]: {responseString}");
						return null;
					}

					return ParsePredictionResult(responseString);
				}
				catch
				{
					return null;
				}
			}

			return null;
		}

		private async Task<string> TranslateTurkishToEnglishAsync(string turkishText)
		{
			try
			{
				string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl=tr&tl=en&dt=t&q={Uri.EscapeDataString(turkishText)}";

				using var request = new HttpRequestMessage(HttpMethod.Get, url);
				request.Headers.Add("User-Agent", "Mozilla/5.0");

				using var response = await _httpClient.SendAsync(request);
				if (!response.IsSuccessStatusCode)
					return turkishText;

				var json = await response.Content.ReadAsStringAsync();
				using var doc = JsonDocument.Parse(json);

				var sb = new StringBuilder();
				foreach (var item in doc.RootElement[0].EnumerateArray())
				{
					if (item[0].ValueKind == JsonValueKind.String)
					{
						sb.Append(item[0].GetString());
					}
				}

				var translated = sb.ToString();
				return string.IsNullOrWhiteSpace(translated) ? turkishText : translated;
			}
			catch
			{
				return turkishText;
			}
		}

		private string ParsePredictionResult(string json)
		{
			using var doc = JsonDocument.Parse(json);
			var root = doc.RootElement;

			if (root.ValueKind != JsonValueKind.Array || root.GetArrayLength() == 0)
				return "Temiz / Normal";

			var firstElement = root[0];
			var predictionsArray = firstElement.ValueKind == JsonValueKind.Array ? firstElement : root;

			string topLabel = "";
			double maxScore = 0;

			foreach (var item in predictionsArray.EnumerateArray())
			{
				if (item.TryGetProperty("label", out var labelProp) &&
					item.TryGetProperty("score", out var scoreProp))
				{
					string label = (labelProp.GetString() ?? "").ToLower();
					double score = scoreProp.GetDouble();

					if (label == "non-toxic" || label == "non_toxic" || label == "neutral" || label == "label_0" || label == "ok")
						continue;

					if (score > maxScore)
					{
						maxScore = score;
						topLabel = label;
					}
				}
			}

			if (maxScore >= 0.40) 
			{
				return MapLabelToTurkish(topLabel);
			}

			return "Temiz / Normal";
		}

		private string MapLabelToTurkish(string label)
		{
			return label.ToLower() switch
			{
				"toxic" or "label_1" => "Toksik",
				"severe_toxic" => "Ağır Toksik",
				"obscene" => "Müstehcen",
				"threat" => "Tehdit",
				"insult" => "Hakaret",
				"identity_hate" => "Nefret Söylemi",
				_ => "Toksik İçerik"
			};
		}
	}
}

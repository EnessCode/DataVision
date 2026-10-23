namespace DataVision.Services.MessageAiService
{
	public interface IMessageAiService
	{
		Task<string> AnalyzeToxicityAsync(string messageText);
	}
}

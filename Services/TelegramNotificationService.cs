using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CryptoBot.Services
{
    public static class TelegramNotificationService
    {
        public static async Task SendNotification(string token, string chatId, string message)
        {
            string url = $"https://api.telegram.org/bot{token}/sendMessage";
            var payload = new { chat_id = chatId, text = message };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            using HttpClient client = new();
            await client.PostAsync(url, content);
        }
    }
}
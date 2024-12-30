using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CryptoBot.Indicators;
using CryptoBot.Services;
using Newtonsoft.Json;

namespace CryptoBot.Services
{
    public static class BinanceWebSocketService
    {
        private static readonly Dictionary<string, DateTime> LastSignalTimes = new();
        private const int CooldownMinutes = 10;

        public static async Task HandleWebSocket(string pair, string baseUrl, string telegramToken, string chatId)
        {
            using var webSocket = new ClientWebSocket();
            string streamUrl = $"{baseUrl}/ws/{pair}@kline_15m";

            try
            {
                await webSocket.ConnectAsync(new Uri(streamUrl), CancellationToken.None);
                Console.WriteLine($"Connected to {pair} stream.");

                var buffer = new byte[4096];
                var closePrices = new List<double>();
                var volumes = new List<double>();

                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        Console.WriteLine($"WebSocket closed for {pair}: {result.CloseStatusDescription}");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    dynamic data = JsonConvert.DeserializeObject(message);
                    if (data?.k == null) continue;

                    double closePrice = Math.Round(Convert.ToDouble(data.k.c), 6);
                    double volume = Math.Round(Convert.ToDouble(data.k.v), 6);

                    closePrices.Add(closePrice);
                    volumes.Add(volume);

                    if (closePrices.Count >= 3)
                    {
                        var signal = SignalGenerator.GenerateSignal(pair, closePrices, volumes, LastSignalTimes, CooldownMinutes);
                        if (signal != null)
                        {
                            await TelegramNotificationService.SendNotification(telegramToken, chatId, signal.Message);
                            LastSignalTimes[pair] = DateTime.UtcNow;
                        }

                        closePrices.RemoveAt(0);
                        volumes.RemoveAt(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error for {pair}: {ex.Message}");
            }
        }
    }
}

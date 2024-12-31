using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        private const int CooldownMinutes = 30;

        public static async Task HandleWebSocket(string pair, string baseUrl, string telegramToken, string chatId)
        {
            var closePrices = new List<double>();
            var volumes = new List<double>();

            // Fetch historical data
            await FetchHistoricalData(pair, baseUrl, "15m", closePrices, volumes);
            await FetchHistoricalData(pair, baseUrl, "1h", closePrices, volumes);

            DetectPatterns(closePrices);

            using var webSocket = new ClientWebSocket();
            string streamUrl = $"{baseUrl}/ws/{pair}@kline_15m";

            try
            {
                await webSocket.ConnectAsync(new Uri(streamUrl), CancellationToken.None);
                Console.WriteLine($"Connected to {pair} stream.");

                var buffer = new byte[4096];

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

                    if (closePrices.Count >= 750)
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

        private static async Task FetchHistoricalData(string pair, string baseUrl, string interval, List<double> closePrices, List<double> volumes)
        {
            string url = $"https://api.binance.com/api/v3/klines?symbol={pair.ToUpper()}&interval={interval}&limit=500";

            using var httpClient = new HttpClient();
            try
            {
                var response = await httpClient.GetStringAsync(url);
                dynamic candles = JsonConvert.DeserializeObject(response);

                foreach (var candle in candles)
                {
                    double closePrice = Math.Round(Convert.ToDouble(candle[4]), 6);
                    double volume = Math.Round(Convert.ToDouble(candle[5]), 6);  

                    closePrices.Add(closePrice);
                    volumes.Add(volume);
                }

                Console.WriteLine($"Fetched historical data for {pair} ({interval})");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to fetch historical data for {pair} ({interval}): {ex.Message}");
            }
        }
        private static void DetectPatterns(List<double> closePrices)
        {
            const int detectionRange = 100; // Increased range for stricter pattern validation

            if (closePrices.Count < detectionRange)
            {
                Console.WriteLine("[DEBUG] Not enough data to detect patterns.");
                return;
            }

            // Stricter Uptrend Detection (Consistent Higher Highs)
            bool isUptrend = closePrices.Skip(closePrices.Count - detectionRange)
                .Select((price, index) => index == 0 || price > closePrices[closePrices.Count - detectionRange + index - 1] * 1.001)
                .All(x => x);
            if (isUptrend)
            {
                Console.WriteLine("[INFO] Detected strong uptrend in historical data.");
            }

            // Stricter Downtrend Detection (Consistent Lower Lows)
            bool isDowntrend = closePrices.Skip(closePrices.Count - detectionRange)
                .Select((price, index) => index == 0 || price < closePrices[closePrices.Count - detectionRange + index - 1] * 0.999)
                .All(x => x);
            if (isDowntrend)
            {
                Console.WriteLine("[INFO] Detected strong downtrend in historical data.");
            }
        }
    }
}

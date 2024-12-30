using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CryptoBot.Services;

namespace CryptoBot
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Crypto Futures Bot Started!");

            string binanceWebSocketUrl = "wss://stream.binance.com:9443/ws";
            var currencyPairs = new[]
            {
                "lpt", "dash", "ftm", "one", "dot", "gtc", "ark", "gala", "crv", "movr", "sxp", "ava", "zen", "cgpt", "the", "ace", "aixbt"
            }.Select(coin => coin + "usdt").ToList();

            string telegramToken = "7028159057:AAF0rse0nKGDFrUlgS6ChlF59Yx_0-6lWvU";
            string chatId = "6638910105";

            var tasks = currencyPairs.Select(pair => BinanceWebSocketService.HandleWebSocket(pair, binanceWebSocketUrl, telegramToken, chatId))
                .ToList();
            await Task.WhenAll(tasks);
        }
    }
}
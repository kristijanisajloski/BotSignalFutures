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
                    // Popular Coins
                    "btc", "eth", "xrp", "ltc", "ada", "sol", "doge", "shib", "bnb", "matic", "uni", "link", "aave", "xlm", "atom", "algo", "vet",

                    // Layer 1 Projects
                    "avax", "near", "dot", "rose", "hbar", "ftm", "egld", "flow", "icp", "ksm",

                    // Layer 2 & Scaling Solutions
                    "matic", "arbitrum", "op", "stark", "imx", "loopring", "metis",

                    // Meme Coins
                    "doge", "shib", "floki", "babydoge", "elon", "akita", "samoyed",

                    // DeFi Tokens
                    "crv", "sushi", "cake", "snx", "comp", "bal", "1inch", "dydx", "yfi",

                    // Gaming/Metaverse Tokens
                    "mana", "sand", "axs", "gala", "enj", "theta", "waxp", "rfox", "ufo", "ilv",
                    "xmr", "zec", "dash", "grin", "beam",
                    "usdt", "usdc", "busd", "dai", "ustc",
                    "cgpt", "ace", "lpt", "the", "zen", "ark", "aixbt", "movr",
                    "ava", "sxp", "gtc", "chz", "iotx", "zil", "qnt", "btt", "hive",
                    "ocean", "fet", "agi", "grt", "rndr", "nmr", "phb", "dxdao", "mln",
                    "ankr", "rune", "ksm", "waves", "hot", "lrc", "bat", "icx", "omg", "zil"
                }
                .Select(coin => coin + "usdt").ToList();



            string telegramToken = "7028159057:AAF0rse0nKGDFrUlgS6ChlF59Yx_0-6lWvU";
            string chatId = "6638910105";

            var tasks = currencyPairs.Select(pair => BinanceWebSocketService.HandleWebSocket(pair, binanceWebSocketUrl, telegramToken, chatId))
                .ToList();
            await Task.WhenAll(tasks);
        }
    }
}
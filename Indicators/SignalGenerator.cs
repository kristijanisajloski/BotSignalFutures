using System;
using System.Collections.Generic;
using System.Linq;
using CryptoBot.Models;

namespace CryptoBot.Indicators
{
    public static class SignalGenerator
    {
        public static Signal GenerateSignal(string pair, List<double> prices, List<double> volumes, Dictionary<string, DateTime> lastSignalTimes, int cooldownMinutes)
        {
            Console.WriteLine($"[INFO] Generating signal for {pair} at {DateTime.UtcNow}");

            if (lastSignalTimes.ContainsKey(pair) &&
                (DateTime.UtcNow - lastSignalTimes[pair]).TotalMinutes < cooldownMinutes)
            {
                Console.WriteLine($"[DEBUG] Skipping signal for {pair} due to cooldown period.");
                return null; // Skip during cooldown
            }

            // Calculate Indicators
            double ema50 = TechnicalIndicators.CalculateEMA(prices, 50);
            Console.WriteLine($"[DEBUG] EMA50: {ema50}");

            double ema200 = TechnicalIndicators.CalculateEMA(prices, 200);
            Console.WriteLine($"[DEBUG] EMA200: {ema200}");

            double rsi = TechnicalIndicators.CalculateRSI(prices);
            Console.WriteLine($"[DEBUG] RSI: {rsi}");

            var (macd, signalLine) = TechnicalIndicators.CalculateMACD(prices);
            Console.WriteLine($"[DEBUG] MACD: {macd}, Signal Line: {signalLine}");

            var (upperBand, lowerBand) = TechnicalIndicators.CalculateBollingerBands(prices);
            Console.WriteLine($"[DEBUG] Bollinger Bands - Upper: {upperBand}, Lower: {lowerBand}");

            double latestPrice = prices.Last();
            Console.WriteLine($"[DEBUG] Latest Price: {latestPrice}");

            double recentVolume = volumes.Last();
            Console.WriteLine($"[DEBUG] Recent Volume: {recentVolume}");

            double avgVolume = volumes.Average();
            Console.WriteLine($"[DEBUG] Average Volume: {avgVolume}");

            double stochastic = TechnicalIndicators.CalculateStochastic(prices);
            Console.WriteLine($"[DEBUG] Stochastic: {stochastic}");

            // Support and Resistance (Simple Implementation)
            double supportLevel = prices.Min();
            Console.WriteLine($"[DEBUG] Support Level: {supportLevel}");

            double resistanceLevel = prices.Max();
            Console.WriteLine($"[DEBUG] Resistance Level: {resistanceLevel}");

            // Relaxed Conditions for Testing
            if (ema50 > ema200 * 0.95 && rsi > 20 && rsi < 80 && macd > signalLine - 0.5 &&
                recentVolume > avgVolume * 1.2 && stochastic < 90 &&
                latestPrice > lowerBand * 0.9 && latestPrice < upperBand * 1.1)
            {
                double entry = latestPrice;
                double target = Math.Round(entry * 1.01, 6); // 1% profit target
                double stopLoss = Math.Round(entry * 0.99, 6); // 1% stop-loss

                Console.WriteLine($"[INFO] Signal generated for {pair}: Entry: {entry}, Target: {target}, Stop-Loss: {stopLoss}");

                return new Signal
                {
                    Pair = pair,
                    Message = $"Test Signal 🚀\n${pair.ToUpper()} Long Signal\nEntry: {entry}\nTarget: {target}\nStop-Loss: {stopLoss}\nRSI: {rsi:F2}, MACD: {macd:F2}, Volume Spike!"
                };
            }

            Console.WriteLine($"[DEBUG] No signal generated for {pair} based on the current conditions.");
            return null;
        }
    }
}

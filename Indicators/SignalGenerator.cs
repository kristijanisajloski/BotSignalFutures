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
                return null;
            }

            // Calculate Indicators
            double ema50 = TechnicalIndicators.CalculateEMA(prices, 50);
            double ema200 = TechnicalIndicators.CalculateEMA(prices, 200);
            double rsi = TechnicalIndicators.CalculateRSI(prices);
            var (macd, signalLine) = TechnicalIndicators.CalculateMACD(prices);
            var (upperBand, lowerBand) = TechnicalIndicators.CalculateBollingerBands(prices);
            double latestPrice = prices.Last();
            double recentVolume = volumes.Last();
            double avgVolume = volumes.Average();
            double stochastic = TechnicalIndicators.CalculateStochastic(prices);

            // Support and Resistance
            double supportLevel = prices.Min();
            double resistanceLevel = prices.Max();

            // Stricter Signal Generation Conditions
            if (ema50 > ema200 * 0.995 && rsi > 22 && rsi < 78 && macd > signalLine - 0.4 &&
                recentVolume > avgVolume * 1.12 && stochastic < 85 &&
                latestPrice > supportLevel * 1.002 && latestPrice < resistanceLevel * 0.997)
            {
                double entry = latestPrice;
                double target = Math.Round(entry * 1.009, 6); // 0.9% profit target
                double stopLoss = Math.Round(entry * 0.991, 6); // 0.9% stop-loss

                Console.WriteLine($"[INFO] Signal generated for {pair}: Entry: {entry}, Target: {target}, Stop-Loss: {stopLoss}");

                return new Signal
                {
                    Pair = pair,
                    Message = $"Stricter Signal 🚀\n${pair.ToUpper()} Long Signal\nEntry: {entry}\nTarget: {target}\nStop-Loss: {stopLoss}\nRSI: {rsi:F2}, MACD: {macd:F2}, Volume Spike!"
                };
            }

            Console.WriteLine($"[DEBUG] No signal generated for {pair} based on stricter conditions.");
            return null;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;

namespace CryptoBot.Indicators
{
    public static class TechnicalIndicators
    {
        public static double CalculateEMA(List<double> prices, int period)
        {
            double multiplier = 2.0 / (period + 1);
            double ema = prices.Take(period).Average();
            foreach (var price in prices.Skip(period))
            {
                ema = (price - ema) * multiplier + ema;
            }
            return ema;
        }

        public static double CalculateRSI(List<double> prices)
        {
            var gains = new List<double>();
            var losses = new List<double>();

            for (int i = 1; i < prices.Count; i++)
            {
                double change = prices[i] - prices[i - 1];
                if (change > 0) gains.Add(change);
                else losses.Add(Math.Abs(change));
            }

            double avgGain = gains.Any() ? gains.Average() : 0;
            double avgLoss = losses.Any() ? losses.Average() : 0;

            if (avgLoss == 0) return 100;

            double rs = avgGain / avgLoss;
            return 100 - (100 / (1 + rs));
        }

        public static (double, double) CalculateMACD(List<double> prices)
        {
            double ema12 = CalculateEMA(prices, 12);
            double ema26 = CalculateEMA(prices, 26);
            double macd = ema12 - ema26;
            double signalLine = CalculateEMA(prices.Skip(prices.Count - 9).ToList(), 9);

            return (macd, signalLine);
        }

        public static (double, double) CalculateBollingerBands(List<double> prices)
        {
            double sma = prices.Average();
            double stdDev = Math.Sqrt(prices.Sum(p => Math.Pow(p - sma, 2)) / prices.Count);

            return (sma + 2 * stdDev, sma - 2 * stdDev);
        }
        
        public static double CalculateStochastic(List<double> prices)
        {
            double highestHigh = prices.TakeLast(14).Max();
            double lowestLow = prices.TakeLast(14).Min();
            double latestPrice = prices.Last();

            return (latestPrice - lowestLow) / (highestHigh - lowestLow) * 100;
        }

        public static (double, double) CalculateSupportResistance(List<double> prices)
        {
            double support = prices.Min();
            double resistance = prices.Max();
            return (support, resistance);
        }
    }
}
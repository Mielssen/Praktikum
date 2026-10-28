using BenchmarkDotNet.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace Praktikum542.Tests.PogrebnyakTests
{
    [MemoryDiagnoser]
    public class PogrebnyakBenchmarkTests
    {
        private readonly List<int> _ratings = Enumerable.Repeat(5, 500).ToList();

        [Benchmark]
        public double CalculateAverageRating_Linq()
        {
            return _ratings.Average();
        }

        [Benchmark]
        public double CalculateAverageRating_ManualLoop()
        {
            double sum = 0;
            for (int i = 0; i < _ratings.Count; i++)
            {
                sum += _ratings[i];
            }
            return sum / _ratings.Count;
        }
    }
}
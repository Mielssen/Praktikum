using BenchmarkDotNet.Running;

namespace Praktikum542.Benchmarks
{
    internal static class EntryPoint
    {
        // Запуск: dotnet run -c Release --project Praktikum542.Benchmarks
        public static void Main()
        {
            BenchmarkRunner.Run<FavoriteServiceBenchmarks>();
        }
    }
}

using BenchmarkDotNet.Attributes;

namespace Praktikum542.Tests.ToderikoTests;

[MemoryDiagnoser]
public class BookingBenchmarks
{
    private readonly List<int> _bookings = Enumerable.Range(1, 10000).ToList();

    [Benchmark(Baseline = true)]
    public List<int> GetAllBookings()
    {
        return _bookings.ToList();
    }

    [Benchmark]
    public List<int> GetBookingsWithStatus()
    {
        return _bookings.Where(x => x % 2 == 0).ToList();
    }
}

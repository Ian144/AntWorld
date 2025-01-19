using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AntWorldBenchmark;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class Benchmarks
{
    private IEnumerable<Types.AntWorld>? _antWorldSeq;

    [IterationSetup]
    public void IterSetup()
    {
        _antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(256, 1, 32, 64, 256);
    }

    [Benchmark]
    public List<Types.AntWorld> OneKIterations()
    {
        return _antWorldSeq.Take(1000).ToList();
    }
}
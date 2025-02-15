using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AntWorldBenchmark;

public enum FadeFunc
{
    FadeTrailsFold, //  PheromoneTrails.FadeTrailsFold
    FadeTrailsArray, // PheromoneTrails.FadeTrailsArray
    FadeTrailsMapFilter // PheromoneTrails.FadeTrailsMapFilter
}

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
public class Benchmarks
{
    private IEnumerable<Types.AntWorld>? _antWorldSeq;

    [Params(FadeFunc.FadeTrailsFold, FadeFunc.FadeTrailsArray, FadeFunc.FadeTrailsMapFilter)]

    public FadeFunc FadeTrailsOption { get; set; }

    [IterationSetup]
    public void IterSetup()
    {
        _antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(256, 4, 64, 64, 256, (int)FadeTrailsOption);
    }

    [Benchmark]
    public List<Types.AntWorld> OneKIterations()
    {
        return _antWorldSeq!.Take(128).ToList();
    }
}
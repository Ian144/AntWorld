using BenchmarkDotNet.Attributes;

namespace AntWorldBenchmark;

public enum FadeFunc
{
    FadeTrailsFold, //  PheromoneTrails.FadeTrailsFold
    FadeTrailsArray, // PheromoneTrails.FadeTrailsArray
    FadeTrailsMapFilter // PheromoneTrails.FadeTrailsMapFilter
}

[MemoryDiagnoser]
public class Benchmarks
{
    private IEnumerable<Types.AntWorld>? _antWorldSeq;

    //[Params(FadeFunc.FadeTrailsFold, FadeFunc.FadeTrailsArray, FadeFunc.FadeTrailsMapFilter)]
    [Params(FadeFunc.FadeTrailsFold)]
    public FadeFunc FadeTrailsOption { get; set; }
    
    [Params(256)]
    //[Params(1024)]
    public int NumTimeSteps { get; set; }
    
    [IterationSetup]
    public void IterSetup() => _antWorldSeq = AntWorldEntryPoint.MakeAntWorldSeq(256, 4, 64, 64, 256, (int)FadeTrailsOption);

    [Benchmark]
    public List<Types.AntWorld> RunWorldGeneration() => _antWorldSeq!.Take(NumTimeSteps).ToList();
}
using AntWorldBenchmark;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;



var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core80).WithGcServer(true).WithId("ServerGC"))
    .AddJob(Job.ShortRun.WithRuntime(CoreRuntime.Core80).WithGcServer(false).WithId("WorkstationGC"));


var _ = BenchmarkRunner.Run<Benchmarks>(config);



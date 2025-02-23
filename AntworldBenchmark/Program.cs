using AntWorldBenchmark;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;



var config = ManualConfig
    .Create(DefaultConfig.Instance)
    .AddColumn(StatisticColumn.Median) // Add Median column
    .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
    .AddJob(Job.LongRun.WithRuntime(CoreRuntime.Core80).WithGcServer(true).WithId("ServerGC"))
    .AddJob(Job.LongRun.WithRuntime(CoreRuntime.Core80).WithGcServer(false).WithId("WorkstationGC"));

// var config = DefaultConfig.Instance
//     .AddColumn(StatisticColumn.Median) // Add Median column
//     .WithSummaryStyle(SummaryStyle.Default.WithRatioStyle(RatioStyle.Trend))
//     .AddJob(Job.LongRun);


//Job.LongRun

var _ = BenchmarkRunner.Run<Benchmarks>(config);



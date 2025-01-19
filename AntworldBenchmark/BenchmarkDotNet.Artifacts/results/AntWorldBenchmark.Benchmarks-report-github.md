``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.26100.2894)
Unknown processor
.NET SDK=9.0.101
  [Host]   : .NET 8.0.12 (8.0.1224.60305), Arm64 RyuJIT AdvSIMD
  .NET 8.0 : .NET 8.0.12 (8.0.1224.60305), Arm64 RyuJIT AdvSIMD

Job=.NET 8.0  Runtime=.NET 8.0  InvocationCount=1  
UnrollFactor=1  

```
|         Method |     Mean |    Error |   StdDev |        Gen0 |       Gen1 |      Gen2 | Allocated |
|--------------- |---------:|---------:|---------:|------------:|-----------:|----------:|----------:|
| OneKIterations | 775.2 ms | 49.54 ms | 146.1 ms | 159000.0000 | 18000.0000 | 1000.0000 | 947.82 MB |

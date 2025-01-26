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
| OneKIterations | 721.6 ms | 45.41 ms | 133.9 ms | 146000.0000 | 21000.0000 | 1000.0000 | 872.49 MB |

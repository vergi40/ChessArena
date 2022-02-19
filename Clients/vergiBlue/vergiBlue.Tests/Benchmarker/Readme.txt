Benchmarking

Run Benchmarker.csproj in release build or from console:
dotnet run -c Release

Power should be plugged.

TODO
* Automate. 
  * Run all tests and benchmarks with single command/script
  * Collect results neatly formatted to one place
  * Append at least date and 5 last commit messages (to easily see recent changes)




19.2.2022 - baseline
|              Method | depth |    Mean |    Error |   StdDev |
|-------------------- |------ |--------:|---------:|---------:|
|        GoodPosition |     3 | 1.050 s | 0.0097 s | 0.0064 s |
|       StartPosition |     4 | 1.611 s | 0.0234 s | 0.0155 s |
| PromomotionPosition |     4 | 1.318 s | 0.0388 s | 0.0257 s |
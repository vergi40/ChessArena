# vergiBlue C# NET6 (Teemu)
[![dev](https://github.com/vergi40/ChessArena/actions/workflows/vergiblue-build-and-test.yaml/badge.svg?branch=dev)](https://github.com/vergi40/ChessArena/actions)  
General chess engine algorithms implemented in C#. Goal is to be easy to read and easy to maintain and upgrade. Main features:
* Minimax
* Alpha-beta pruning
* Iterative deepening
* Transposition tables
* Graphical interface for playing and testing
* ![image](https://user-images.githubusercontent.com/16613890/138566824-393fe1c0-8c0b-46e9-b3ea-437e76d23a3e.png)

As the engine needs to calculate millions of boards per second, every small optimization can speed up game greatly. [BenchMarkDotnet](https://github.com/dotnet/BenchmarkDotNet) is used to test various operations to decide best design (e.g. 2D board array vs 1D, List or Array or Dictionary). Custom optimizations:
* Board represented in 1D array
* All slider attacks (rook, bishop, queen) for each square calculated beforehand
* All pieces initialized for each square beforehand
* Allocate move lists on stack instead of heap memory
* Arrays instead of Dictionaries everywhere possible


## Design
* Future [TODO-list](vergiBlue/TODO-list.md)



## Testing
Due to complexity of multiple algorithms combined with multiple data structures, chess engines are keen to present regression bugs from even minor features
* Unit tests: Basic validation, move generation, board situations, transpositions
* Perft tests: Perft testing means generating all possible moves to depth n for given board situation, counting total moves generated, and matching these to a known chess engine generation. 
  * EDP perft suite has 124 board situations with expected move count for each search depth. These contain quite rare events, like en passant protecting king, all promotion types, castling under attack
  * Methods for running perft on any board and for dividing results to each possible move
* Benchmarking: 
  * [BenchMarkDotnet](https://github.com/dotnet/BenchmarkDotNet) produces quite stabile results for various runs
  * Design benchmarks to choose most efficienct design
  * Perft, search and evaluation benchmarks to spot problems in new changes. Example of basic benchmarking in v0.3.1:

Perft benchmark
|              Method | depth |     Mean |    Error |   StdDev |   Median |      Min |      Max |
|-------------------- |------ |---------:|---------:|---------:|---------:|---------:|---------:|
|        GoodPosition |     3 | 366.3 ms | 14.00 ms |  9.26 ms | 364.5 ms | 354.4 ms | 379.9 ms |
|       StartPosition |     4 | 707.6 ms | 37.93 ms | 25.09 ms | 693.4 ms | 684.5 ms | 746.1 ms |
| PromomotionPosition |     4 | 400.9 ms | 14.62 ms |  9.67 ms | 398.2 ms | 395.5 ms | 428.1 ms |


Search benchmark
|              Method | Depth | UseID | UseTT |         Mean |      Error |     StdDev |       Median |         Min |          Max |
|-------------------- |------ |------ |------ |-------------:|-----------:|-----------:|-------------:|------------:|-------------:|
|       StartPosition |     4 | False | False |    471.31 ms |  19.586 ms |  18.320 ms |    465.60 ms |   452.15 ms |    510.09 ms |
|        GoodPosition |     4 | False | False |  1,168.71 ms |  37.662 ms |  35.229 ms |  1,172.65 ms | 1,122.49 ms |  1,245.72 ms |
| PromomotionPosition |     4 | False | False |     56.78 ms |  14.887 ms |  13.926 ms |     50.94 ms |    45.60 ms |     87.76 ms |
|       StartPosition |     4 | False |  True |    109.34 ms |  21.151 ms |  19.785 ms |    101.31 ms |    92.73 ms |    157.35 ms |
|        GoodPosition |     4 | False |  True |    559.13 ms |  18.143 ms |  16.971 ms |    565.52 ms |   532.29 ms |    579.62 ms |
| PromomotionPosition |     4 | False |  True |     33.64 ms |   9.242 ms |   8.645 ms |     32.05 ms |    23.22 ms |     45.77 ms |
|       StartPosition |     4 |  True | False |    552.37 ms |  21.450 ms |  20.065 ms |    554.34 ms |   518.38 ms |    577.91 ms |
|        GoodPosition |     4 |  True | False |  1,321.92 ms |  37.171 ms |  34.770 ms |  1,308.63 ms | 1,274.25 ms |  1,384.54 ms |
| PromomotionPosition |     4 |  True | False |     79.39 ms |  22.146 ms |  20.715 ms |     69.42 ms |    64.08 ms |    127.69 ms |
|       StartPosition |     4 |  True |  True |     38.86 ms |   9.272 ms |   8.673 ms |     37.23 ms |    28.86 ms |     52.49 ms |
|        GoodPosition |     4 |  True |  True |    234.32 ms |  13.611 ms |  12.732 ms |    231.36 ms |   224.05 ms |    276.47 ms |
| PromomotionPosition |     4 |  True |  True |     35.39 ms |  10.390 ms |   9.718 ms |     29.32 ms |    25.82 ms |     51.31 ms |
|       StartPosition |     5 | False | False |  5,247.38 ms | 147.242 ms | 137.730 ms |  5,228.85 ms | 5,045.59 ms |  5,451.19 ms |
|        GoodPosition |     5 | False | False | 10,236.31 ms | 358.420 ms | 335.266 ms | 10,150.61 ms | 9,824.27 ms | 10,883.27 ms |
| PromomotionPosition |     5 | False | False |    218.83 ms |  18.935 ms |  17.712 ms |    214.15 ms |   207.26 ms |    279.17 ms |
|       StartPosition |     5 | False |  True |    474.87 ms |  28.800 ms |  26.940 ms |    462.64 ms |   445.96 ms |    526.54 ms |
|        GoodPosition |     5 | False |  True |  5,786.44 ms | 205.729 ms | 192.439 ms |  5,750.24 ms | 5,556.03 ms |  6,147.19 ms |
| PromomotionPosition |     5 | False |  True |    105.00 ms |  26.337 ms |  24.635 ms |     97.68 ms |    85.40 ms |    182.54 ms |
|       StartPosition |     5 |  True | False |  5,139.01 ms | 120.406 ms | 112.627 ms |  5,099.19 ms | 5,013.49 ms |  5,375.70 ms |
|        GoodPosition |     5 |  True | False |  5,148.81 ms | 137.308 ms | 128.438 ms |  5,095.87 ms | 5,006.88 ms |  5,408.67 ms |
| PromomotionPosition |     5 |  True | False |    271.26 ms |  10.674 ms |   9.984 ms |    271.89 ms |   251.78 ms |    286.78 ms |
|       StartPosition |     5 |  True |  True |    511.15 ms |  13.421 ms |  12.554 ms |    511.19 ms |   482.35 ms |    526.88 ms |
|        GoodPosition |     5 |  True |  True |  5,062.99 ms |  47.769 ms |  44.684 ms |  5,055.32 ms | 5,011.11 ms |  5,190.51 ms |
| PromomotionPosition |     5 |  True |  True |     88.60 ms |  21.883 ms |  20.469 ms |     80.07 ms |    72.60 ms |    130.44 ms |


Evaluation benchmark
|              Method |       N |        Mean |    Error |   StdDev |      Median |         Min |         Max |      Gen 0 | Allocated |
|-------------------- |-------- |------------:|---------:|---------:|------------:|------------:|------------:|-----------:|----------:|
|       StartPosition |  100000 |   108.81 ms | 16.41 ms | 15.35 ms |   104.58 ms |    92.70 ms |   150.61 ms |  3000.0000 |     30 MB |
|        GoodPosition |  100000 |   116.52 ms | 27.43 ms | 25.66 ms |   104.53 ms |   100.85 ms |   197.82 ms |  3000.0000 |     30 MB |
| PromomotionPosition |  100000 |    61.19 ms | 14.16 ms | 13.25 ms |    55.80 ms |    49.31 ms |    83.51 ms |  3000.0000 |     30 MB |
|       StartPosition | 1000000 | 1,033.46 ms | 61.60 ms | 57.62 ms | 1,015.94 ms |   965.70 ms | 1,154.79 ms | 37000.0000 |    298 MB |
|        GoodPosition | 1000000 | 1,150.94 ms | 95.47 ms | 89.30 ms | 1,106.26 ms | 1,066.63 ms | 1,352.62 ms | 37000.0000 |    298 MB |
| PromomotionPosition | 1000000 |   528.91 ms | 84.97 ms | 79.48 ms |   508.09 ms |   445.26 ms |   757.39 ms | 37000.0000 |    298 MB |


## Try out
* Includes [test server](Clients/vergiBlue/TestServer/) and console app where AI can play against itself
	* Full demo with two independent AI's hosted on TestServer over gRPC connection: [startlocalservergame.bat](Clients/vergiBlue/startlocalservergame.bat). Script builds all projects and starts new console for each app.
	* Graphical chess demo: Open solution and run "vergiBlueDesktop" project. Desktop chess has some settings, normal black or white start and some test game situations.



using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using vergiBlue.Pieces;

namespace Benchmarker.Design;

[SimpleJob(RunStrategy.Throughput)]
public class BoolDictVsArrayBenchmark
{
    [Params(10000, 100000)]
    public int N;

    private readonly Dictionary<bool, Pawn> _dict;
    private readonly Pawn[] _array;
    public BoolDictVsArrayBenchmark()
    {
        _dict = new Dictionary<bool, Pawn>()
        {
            { true, new Pawn(true, (0, 0)) },
            { false, new Pawn(false, (0, 0)) },
        };

        _array = new[]
        {
            new Pawn(true, (0, 0)),
            new Pawn(false, (0, 0))
        };
    }


    [Benchmark]
    public void ArrayOperations()
    {
        for (int i = 0; i < N; i++)
        {
            _ = _array[0];
            _ = _array[1];
        }
    }

    [Benchmark]
    public void ArrayWithConversionOperations()
    {
        for (int i = 0; i < N; i++)
        {
            _ = _array[BoolToInt(true)];
            _ = _array[BoolToInt(false)];
        }
    }

    private int BoolToInt(bool index)
    {
        // true -> white
        return index ? 1 : 0;
    }

    [Benchmark]
    public void BoolDictOperations()
    {
        for (int i = 0; i < N; i++)
        {
            _ = _dict[true];
            _ = _dict[false];
        }
    }

    // TODO byte array
}
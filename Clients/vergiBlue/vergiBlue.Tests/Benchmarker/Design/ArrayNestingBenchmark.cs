using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using vergiBlue.Pieces;

namespace Benchmarker.Design;

[SimpleJob(RunStrategy.Throughput)]
public class ArrayNestingBenchmark
{
    [Params(10000, 100000)]
    public int N;

    private readonly Pawn[] _singleArray = new Pawn[128];
    private readonly ArrayForPieceType[] _nestedArray = new ArrayForPieceType[2];

    class ArrayForPieceType
    {
        public Pawn[] SingleArray = new Pawn[64];
    }
    public ArrayNestingBenchmark()
    {
        var blackPawns = new Pawn[64];
        var whitePawns = new Pawn[64];

        for (int i = 0; i < 64; i++)
        {
            var piece = new Pawn(false, (0, 0));
            blackPawns[i] = piece;
            _singleArray[i] = piece;
        }

        for (int i = 0; i < 64; i++)
        {
            var piece = new Pawn(true, (0, 0));
            whitePawns[i] = piece;
            _singleArray[i + 64] = piece;
        }

        _nestedArray[0] = new ArrayForPieceType() { SingleArray = blackPawns };
        _nestedArray[1] = new ArrayForPieceType() { SingleArray = whitePawns };
    }


    [Benchmark]
    public void SingleLargeArrayOperations()
    {
        for (int i = 0; i < N; i++)
        {
            _ = _singleArray[0];
            _ = _singleArray[64];
        }
    }

    [Benchmark]
    public void NestedArrayOperations()
    {
        for (int i = 0; i < N; i++)
        {
            _ = _nestedArray[0].SingleArray[0];
            _ = _nestedArray[1].SingleArray[0];
        }
    }
}
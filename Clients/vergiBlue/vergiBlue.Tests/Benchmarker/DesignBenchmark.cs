using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using PerftTests;
using vergiBlue;
using vergiBlue.BoardModel;
using vergiBlue.Pieces;

namespace Benchmarker
{
    public class DesignBenchmark
    {
        [SimpleJob(RunStrategy.Throughput)]
        [MeanColumn, MedianColumn, MinColumn, MaxColumn]
        public class BoardArrayTypeBenchmark
        {
            [Params(10000,20000)]
            public int N { get; set; }

            private PieceBase?[] _board1D;
            private PieceBase?[,] _board2D;

            [GlobalSetup]
            public void GlobalSetup()
            {
                _board1D = new PieceBase[64];
                _board2D = new PieceBase[8, 8];
            }

            [Benchmark]
            public void BoardArray1D()
            {
                // (2,2) = 2*8 + 2 = 18
                // (2,6) = 6*8 + 2 = 50
                var pos1 = 18;
                var pos2 = 50;
                var piece1 = new Rook(true, (2, 2));
                var piece2 = new Rook(true, (2, 6));
                _board1D[pos1] = piece1;

                for (int i = 0; i < N; i++)
                {
                    var pieceA = _board1D[pos1];
                    _board1D[pos2] = piece2;
                    _board1D[pos1] = null;

                    var pieceB = _board1D[pos2];
                    _board1D[pos1] = piece1;
                    _board1D[pos2] = null;

                    var posA = _board1D[0];
                    var posB = _board1D[4];
                    var posC = _board1D[36];
                    var posD = _board1D[32];
                }
            }

            [Benchmark]
            public void BoardArray1DTransformed()
            {
                var pos1 = (2, 2);
                var pos2 = (2, 6);
                var piece1 = new Rook(true, pos1);
                var piece2 = new Rook(true, pos2);
                _board1D[pos1.To1DimensionArray()] = piece1;

                for (int i = 0; i < N; i++)
                {
                    var pieceA = _board1D[pos1.To1DimensionArray()];
                    _board1D[pos2.To1DimensionArray()] = piece2;
                    _board1D[pos1.To1DimensionArray()] = null;

                    var pieceB = _board1D[pos2.To1DimensionArray()];
                    _board1D[pos1.To1DimensionArray()] = piece1;
                    _board1D[pos2.To1DimensionArray()] = null;

                    var posA = _board1D[(0, 0).To1DimensionArray()];
                    var posB = _board1D[(4, 0).To1DimensionArray()];
                    var posC = _board1D[(4, 4).To1DimensionArray()];
                    var posD = _board1D[(0, 4).To1DimensionArray()];
                }
            }

            [Benchmark]
            public void BoardArray2D()
            {
                var pos1 = (2, 2);
                var pos2 = (2, 6);
                var piece1 = new Rook(true, pos1);
                var piece2 = new Rook(true, pos2);
                _board2D[pos1.Item1, pos1.Item2] = piece1;
                
                for (int i = 0; i < N; i++)
                {
                    var pieceA = _board2D[pos1.Item1, pos1.Item2];
                    _board2D[pos2.Item1, pos2.Item2] = piece2;
                    _board2D[pos1.Item1, pos1.Item2] = null;

                    var pieceB = _board2D[pos2.Item1, pos2.Item2];
                    _board2D[pos1.Item1, pos1.Item2] = piece1;
                    _board2D[pos2.Item1, pos2.Item2] = null;

                    var posA = _board2D[0, 0];
                    var posB = _board2D[4, 0];
                    var posC = _board2D[4, 4];
                    var posD = _board2D[0, 4];
                }
            }
        }

        public class MakeMoveOrConstructorBenchmark
        {

        }
    }
}

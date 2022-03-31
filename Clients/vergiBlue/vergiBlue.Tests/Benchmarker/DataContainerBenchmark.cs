using System.Collections.Generic;
using System.Reflection.Metadata;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using vergiBlue.Pieces;

namespace Benchmarker
{
    [SimpleJob(RunStrategy.Throughput)]
    public class DataContainerTypeBenchmark
    {

        [Params(10000, 100000)]
        public int N;

        private readonly ClassTypeContainer _classContainer;
        //private static readonly StaticClassTypeContainer _staticClassContainer;
        private readonly StructTypeContainer _structContainer;

        public DataContainerTypeBenchmark()
        {
            _classContainer = new ClassTypeContainer();
            _structContainer = new StructTypeContainer();

            // Invoke lazy init
            _ = StaticClassTypeContainer.Get(0);
        }

        [Benchmark]
        public void ClassContainer()
        {
            for (int i = 0; i < N; i++)
            {
                _ = _classContainer.Get(0);
                _ = _classContainer.Get(1);
            }
        }

        [Benchmark]
        public void StaticClassContainer()
        {
            for (int i = 0; i < N; i++)
            {
                _ = StaticClassTypeContainer.Get(0);
                _ = StaticClassTypeContainer.Get(1);
            }
        }

        [Benchmark]
        public void StructContainer()
        {
            for (int i = 0; i < N; i++)
            {
                _ = _structContainer.Get(0);
                _ = _structContainer.Get(1);
            }
        }

        public class ClassTypeContainer
        {
            private readonly Pawn[] _array;
            public ClassTypeContainer()
            {
                _array = new[]
                {
                    new Pawn(true, (0, 0)),
                    new Pawn(false, (0, 0))
                };
            }

            public Pawn Get(int index) => _array[index];
        }

        public static class StaticClassTypeContainer
        {
            private static readonly Pawn[] _array = new[]
            {
                new Pawn(true, (0, 0)),
                new Pawn(false, (0, 0))
            };

            public static Pawn Get(int index) => _array[index];
        }

        public readonly struct StructTypeContainer
        {
            private readonly Pawn[] _array;
            public StructTypeContainer()
            {
                _array = new[]
                {
                    new Pawn(true, (0, 0)),
                    new Pawn(false, (0, 0))
                };
            }

            public Pawn Get(int index) => _array[index];
        }
    }

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
}

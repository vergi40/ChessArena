using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using vergiBlue.Pieces;

namespace Benchmarker.Design
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
}

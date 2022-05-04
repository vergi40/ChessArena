using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace Benchmarker.Design
{
    [SimpleJob(RunStrategy.Throughput)]
    [MeanColumn, MedianColumn, MinColumn, MaxColumn, MemoryDiagnoser]
    public class SortBenchmark
    {
        [Params(10000, 100000)]
        public int N;

        private IReadOnlyList<int> _listToSort { get; }

        public SortBenchmark()
        {
            _listToSort = new List<int>
            {
                8, 14, 5, 0, 17, 11, 3, 15, 6, 13, 1, 4, 3, 18, 9, 12, 16, 2, 7, 11, 10
            };
        }

        [Benchmark]
        public void InputReadonlyOutputList()
        {
            for (int i = 0; i < N; i++)
            {
                var target = _listToSort.ToList();
                var sorted = Sort(target);
            }
        }

        private List<int> Sort(IReadOnlyList<int> input)
        {
            var result = input.ToList();
            result.Sort();
            return result;
        }

        [Benchmark]
        public void InputRefList()
        {
            for (int i = 0; i < N; i++)
            {
                var target = _listToSort.ToList();
                Sort(ref target);
            }
        }

        private void Sort(ref List<int> input)
        {
            input.Sort();
        }

        [Benchmark]
        public void InputSpan()
        {
            for (int i = 0; i < N; i++)
            {
                AllocateAndSort(_listToSort);
            }
        }

        private void AllocateAndSort(IReadOnlyList<int> input)
        {
            Span<int> targetSpan = stackalloc int[input.Count];
            FillSpan(targetSpan, input);
            Sort(targetSpan);
        }

        private void Sort(Span<int> input)
        {
            input.Sort();
        }

        private static void FillSpan<T>(Span<T> span, IReadOnlyList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                span[i] = list[i];
            }
        }
    }
}

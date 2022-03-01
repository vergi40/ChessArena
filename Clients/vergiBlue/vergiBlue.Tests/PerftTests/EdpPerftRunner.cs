using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using vergiBlue.BoardModel;

namespace PerftTests
{
    [TestFixture]
    class EdpPerftRunner
    {
        private List<string> _testList { get; set; }

        [OneTimeSetUp]
        public void Setup()
        {
            var filePath = Path.Combine(GetProjectPath(), "perftsuite.epd");
            _testList = File.ReadAllLines(filePath).ToList();

            // 126 tests
        }


        [Test]
        public void RunTestSuite([Range(0, 125)] int index, [Range(1,4)] int depth)
        {
            var testCase = ReadTestCase(index);
            var result = Perft.PerftRec(testCase.Board, depth, testCase.WhiteStarts);
            Assert.AreEqual(testCase.ResultForDepth(depth), result, 0.0, $"Perft failed for [{testCase.Fen}] depth {depth}");
        }

        private EdbCase ReadTestCase(int index)
        {
            // Example line
            // 4k3/8/8/8/8/8/8/4K2R w K - 0 1 ;D1 15 ;D2 66 ;D3 1197 ;D4 7059 ;D5 133987 ;D6 764643
            var fields = _testList[index].Split(" ;");
            var board = BoardFactory.CreateFromFen(fields[0], out var isWhiteTurn);
            var expected = ReadExpected(fields.ToList());

            var edbCase = new EdbCase()
            {
                Board = board,
                Fen = fields[0],
                WhiteStarts = isWhiteTurn,
                Expected = expected
            };

            return edbCase;

        }

        private Dictionary<int, long> ReadExpected(List<string> inputArray)
        {
            var dict = new Dictionary<int, long>();
            for (int i = 1; i < inputArray.Count; i++)
            {
                var depthAndResult = inputArray[i].Split(" ");
                if (depthAndResult[0].Contains(i.ToString()))
                {
                    dict.Add(i, long.Parse(depthAndResult[1]));
                }
            }

            return dict;
        }

        class EdbCase
        {
            public IBoard Board { get; set; }
            public string Fen { get; set; }
            public bool WhiteStarts { get; set; }
            public Dictionary<int, long> Expected { get; set; } = new();

            public long ResultForDepth(int depth)
            {
                return Expected[depth];
            }
        }

        public static string GetAssemblyPath()
        {
            var exePath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            return exePath;
        }

        public static string GetProjectPath()
        {
            // Hack
            var exePath = GetAssemblyPath();
            var solution = Path.Combine(exePath, @"..\..\..");
            return Path.GetFullPath(solution);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace PerftTests
{
    [TestFixture]
    class BasicCases
    {
        [Test]
        [TestCase(0, ExpectedResult = 1)]
        [TestCase(1, ExpectedResult = 20)]
        [TestCase(2, ExpectedResult = 400)]
        [TestCase(3, ExpectedResult = 8902)]
        [TestCase(4, ExpectedResult = 197281)]
        //[TestCase(5, ExpectedResult = 4865609)]
        public long StartPosition(int depth)
        {
            return Cases.StartPosition(depth);
        }

        /// <summary>
        /// Promotion, castling, en passant
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        [Test]
        [TestCase(1, ExpectedResult = 48)]
        [TestCase(2, ExpectedResult = 2039)]
        [TestCase(3, ExpectedResult = 97862)]
        [TestCase(4, ExpectedResult = 4085603)]
        //[TestCase(5, ExpectedResult = 193690690)]
        public long GoodPositions_AndrewWagner(int depth)
        {
            return Cases.GoodPositions_AndrewWagner(depth);
        }

        /// <summary>
        /// http://www.rocechess.ch/perft.html
        /// </summary>
        [Test]
        [TestCase(1, ExpectedResult = 24)]
        [TestCase(2, ExpectedResult = 496)]
        [TestCase(3, ExpectedResult = 9483)]
        [TestCase(4, ExpectedResult = 182838)]
        [TestCase(5, ExpectedResult = 3605103)]
        public long Promotion_AndrewWagner(int depth)
        {
            return Cases.Promotion_AndrewWagner(depth);
        }
    }
}

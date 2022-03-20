using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using vergiBlue;
using vergiBlue.Algorithms.Basic;
using vergiBlue.Algorithms.IterativeDeepening;
using vergiBlue.BoardModel;
using vergiBlue.Logic;
using vergiBlue.Pieces;

namespace UnitTests
{
    [TestFixture]
    class EndGame_AlgorithmsWithDepths_Tests
    {
        [Test]
        public void DoubleRook_MiniMaxBasic_ShouldCheckMate([Range(2, 4)] int depth)
        {
            var preMoveContext = DoubleRookPreMoveContext(depth);
            var algo = new MiniMaxBasic();

            var expected = new SingleMove("c8", "a8");
            var result = algo.CalculateBestMove(preMoveContext);

            result.ToString().ShouldBe(expected.ToString());
        }

        [Test]
        public void DoubleRook_IDBasic_ShouldCheckMate([Range(2, 4)] int depth)
        {
            var preMoveContext = DoubleRookPreMoveContext(depth);
            var algo = new IDBasic();

            var expected = new SingleMove("c8", "a8");
            var result = algo.CalculateBestMove(preMoveContext);

            result.ToString().ShouldBe(expected.ToString());
        }

        internal static BoardContext DoubleRookPreMoveContext(int depth)
        {
            // Start situation
            // 8  r    k  
            // 7 r 
            // 6      
            // 5    
            // 4
            // 3K   
            // 2
            // 1       
            //  ABCDEFGH
            var pieces = new List<PieceBase>
            {
                new King(true, "a3"),
                new King(false, "h8"),
                new Rook(false, "b7"),
                new Rook(false, "c8"),
            };
            var board = BoardFactory.CreateFromPieces(pieces);

            var moves = board.MoveGenerator.MovesQuick(false, true).ToList();
            var context = new BoardContext()
            {
                CurrentBoard = board,
                IsWhiteTurn = false,
                MaxTimeMs = 2000,
                NominalSearchDepth = depth,
                ValidMoves = moves
            };

            return context;
        }
    }
}
